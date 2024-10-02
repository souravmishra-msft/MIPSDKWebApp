using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Microsoft.InformationProtection;
using Microsoft.InformationProtection.File;
using Microsoft.InformationProtection.Protection;
using MIPSDK_WebApp1.Data;
using MIPSDK_WebApp1.Models.Entities;
using System.Reflection.Metadata.Ecma335;

namespace MIPSDK_WebApp1.Services
{
    public class MipService: IMipService
    {
        private readonly AuthDelegateImpl _authDelegate;
        private readonly IConfiguration _configuration;
        private readonly string _defaultEngineId;
        private readonly IFileProfile _fileProfile;
        private readonly MipContext _mipContext;
        private List<IFileEngine> _fileEngines;

        public MipService(IConfiguration configuration)
        {
            _fileEngines = new List<IFileEngine>();
            _configuration = configuration;

            // Create application info using config settings
            ApplicationInfo appInfo = new ApplicationInfo()
            {
                ApplicationId = _configuration.GetSection("AzureAd").GetValue<string>("ClientId"),
                ApplicationName = _configuration.GetSection("MipConfig").GetValue<string>("ApplicationName"),
                ApplicationVersion = _configuration.GetSection("MipConfig").GetValue<string>("Version"),
            };

            // Set the default engine Id to the clientId. This is what we'll use to cache any engines that aren't delegated or on-behalf-of.
            _defaultEngineId = configuration.GetSection("AzureAd").GetValue<string>("ClientId");

            // Initialize the auth delegate
            _authDelegate = new AuthDelegateImpl(_configuration);

            // Initialize the MIP context and create config
            MIP.Initialize(MipComponent.File);
            MipConfiguration mipConfig = new(appInfo, "mip_data", Microsoft.InformationProtection.LogLevel.Trace, false);
            _mipContext = MIP.CreateMipContext(mipConfig);

            // Initialize FileProfileSettings and FileProfile
            FileProfileSettings profileSettings = new FileProfileSettings(_mipContext, CacheStorageType.InMemory, new ConsentDelegateImpl());
            _fileProfile = MIP.LoadFileProfileAsync(profileSettings).Result;
        }

        // Applies the specified labelId to the provided inputStream. Returns the labeled stream.
        public MemoryStream ApplyMipLabel(Stream inputStream, string labelId)
        {
            IFileEngine engine = GetEngine(_defaultEngineId);

            // Create a handler with a hardcoded filename, using the input stream.
            IFileHandler handler = engine.CreateFileHandlerAsync(inputStream, "EmployeeData.xlsx", true).GetAwaiter().GetResult();

            LabelingOptions options = new()
            {
                AssignmentMethod = AssignmentMethod.Standard
            };

            // Set the label on the handler.
            handler.SetLabel(engine.GetLabelById(labelId), options, new ProtectionSettings());
            MemoryStream outputStream = new MemoryStream();

            // Commit the changes and write to the outputStream.
            handler.CommitAsync(outputStream).GetAwaiter().GetResult();

            // Ensure the output stream resets to the begining
            outputStream.Position = 0;

            return outputStream;
        }

        // Get the file label from the inputStream. Users userId parameter to perform the operation in the context of the input userId.
        public ContentLabel GetFileLabel(string userId, Stream inputStream)
        {
            // Fetch an engine for the provided user. If the user has rights to the file, method will return label. 
            // If the user doesn't have rights, it'll throw AccessDeniedException.
            IFileEngine engine = GetDelegatedEngine(userId);
            IFileHandler handler; 
            
            try
            {
                handler = engine.CreateFileHandlerAsync(inputStream, "fileUpload.xlxs", true).Result;
            } catch (Microsoft.InformationProtection.Exceptions.AccessDeniedException ex)
            {
                throw ex;
            } catch (AggregateException ex)
            {
                throw ex.GetBaseException();
            }
            
            return handler.Label;
        }

        // Returns true if the inputStream is labeled or protected.
        public bool IsLabeledOrProtected(Stream inputStream)
        {
            IFileStatus status = FileHandler.GetFileStatus(inputStream, "UploadedFile.xlxs", _mipContext);
            bool isLabeled = status.IsLabeled();
            bool isProtected = status.IsProtected();

            return (isLabeled || isProtected);
        }

        //Returns the protected status of the inputStream
        public bool IsProtected(Stream inputStream) {
            IFileStatus status = FileHandler.GetFileStatus(inputStream, "UploadedFile.xlxs", _mipContext);
            bool result = status.IsProtected();
            return result;
        }

        // Get the integer sensitivity value from the specified label. Used to evaluate policy for upload.
        public int GetLabelSensitivityValue(string labelGuid)
        {
            IFileEngine engine = GetEngine(_defaultEngineId);
            return engine.GetLabelById(labelGuid).Sensitivity;
        }

        // Get all labels for the specified userId.
        public IList<MipLabel> GetMipLabels(string userId)
        {
            IFileEngine engine = GetDelegatedEngine(userId);
            List<MipLabel> outputList = new List<MipLabel>();

            foreach (var label in engine.SensitivityLabels)
            {
                if(label.IsActive)
                {
                    outputList.Add(new MipLabel()
                    {
                        Id = label.Id,
                        Name = label.Name,
                        Sensitivity = label.Sensitivity
                    });
                }

                if (label.Children.Count() > 0)
                {
                    foreach (var child in label.Children)
                    {
                        if (child.IsActive)
                        {
                            outputList.Add(new MipLabel()
                            {
                                Id = child.Id,
                                Name = String.Join("-", label.Name, child.Name),
                                Sensitivity = child.Sensitivity
                            });
                        }

                    }
                }
            }

            return outputList;
        }

        // Gets the decrypted copy of the protected inputStream for the specified user.
        public Stream GetTemporaryDecryptedStream(Stream inputStream, string userId)
        {
            // Craete a delegated engine and a new handler.
            IFileEngine engine = GetDelegatedEngine(userId);
            IFileHandler handler = engine.CreateFileHandlerAsync(inputStream, "EmployeeData.xlxs", true).GetAwaiter().GetResult();

            // Validate that input isProtected
            if(handler.Protection != null)
            {
                // Validate that user has rights to remove protection from the file.
                // Rights enforcement is up to the application implementing the SDK! 
                if (handler.Protection.AccessCheck(Rights.Extract))
                {
                    // If user has Extract right, return decrypted copy. Otherwise, throw exception.
                    return handler.GetDecryptedTemporaryStreamAsync().GetAwaiter().GetResult();
                }
                throw new Microsoft.InformationProtection.Exceptions.AccessDeniedException("User lacks EXPORT right.");
            } else
            {
                throw new Exception("File Not Protected");
            }
        }

       
        // Get an engine of the specified engineId. If the engine doesn't exist in app cache, fetch from MIP cache or create a new one.
        private IFileEngine GetEngine(string engineId)
        {
            IFileEngine engine;

            // Check fo existing engine. If it doesnt exist in cache, create a new one.
            if(_fileEngines.Count == 0 || _fileEngines.Find(e => e.Settings.EngineId == engineId) == null)
            {
                FileEngineSettings settings = new(engineId, _authDelegate, "", "en-US")
                {
                    Cloud = Cloud.Commercial // Hardcoded to commercial cloud.
                };

                // Get engine and add to cache.
                engine = _fileProfile.AddEngineAsync(settings).Result;
                _fileEngines.Add(engine);
            } else
            {
                // Fetch engine from cache and return
                engine = _fileEngines.Where(e => e.Settings.EngineId == engineId).First();
            }

            return engine;
        }

        // Creates a delegated FileEngine for the specified user.All operations will be performed as that user.
        // Requires the Content.DelegatedReader and Content.DelegatedWriter permission.
        private IFileEngine GetDelegatedEngine(string userId)
        {
            IFileEngine engine;

            // Check cache for existing engine. If it doesn't exist, create a new one.
            if(_fileEngines.Count == 0 || _fileEngines.Where(e=> e.Settings.EngineId == userId).Count() == 0)
            {
                // Fix the engineId
                FileEngineSettings settings = new FileEngineSettings(userId, _authDelegate, "", "en-US")
                {
                    Cloud = Cloud.Commercial,
                    DelegatedUserEmail = userId
                };

                // Add async?
                engine = _fileProfile.AddEngineAsync(settings).Result;
                _fileEngines.Add(engine);
            } else
            {
                // Fetch existing engine from cache
                engine = _fileEngines.Where(e => e.Settings.EngineId == userId).First();
            }

            return engine;
        }


    }
}
