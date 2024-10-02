using Microsoft.InformationProtection;
using MIPSDK_WebApp1.Models.Entities;

namespace MIPSDK_WebApp1.Services
{
    public interface IMipService
    {
        ContentLabel GetFileLabel(string userId, Stream inputStream);
        public IList<MipLabel> GetMipLabels(string userId);
        public int GetLabelSensitivityValue(string labelGuid);
        public bool IsLabeledOrProtected(Stream inputStream);
        public bool IsProtected(Stream inputStream);
        public MemoryStream ApplyMipLabel(Stream inputStream, string labelId);
        public Stream GetTemporaryDecryptedStream(Stream inputStream, string userId);
    }
}
