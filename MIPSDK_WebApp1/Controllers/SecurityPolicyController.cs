using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIPSDK_WebApp1.Data;
using MIPSDK_WebApp1.Models;
using MIPSDK_WebApp1.Models.Entities;
using MIPSDK_WebApp1.Services;
using SQLitePCL;

namespace MIPSDK_WebApp1.Controllers
{
    public class SecurityPolicyController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly MipService _mipService;
        private readonly ILogger<SecurityPolicyController> _logger;

        public SecurityPolicyController(ApplicationDBContext context, MipService mipService, ILogger<SecurityPolicyController> logger)
        {
            _context = context;
            _mipService = mipService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Retrieve the user id from the authenticated user's context
            string? userId = User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                // Handle the case where userId is null or empty
                return Unauthorized();
            }

            // Fetch all Mip Labels for the user
            IList<MipLabel> mipLabels = _mipService.GetMipLabels(userId);
            _logger.LogInformation("Fetched Mip Labels: {@MipLabels}", mipLabels);

            // Fetch data policies
            IList<DataPolicy> dataPolicies = _context.DataPolicies.ToList();
            _logger.LogInformation("Fetched Data Policies: {@DataPolicies}", dataPolicies);

            // Create a viewModel to pass to the view
            var viewModel = dataPolicies.Select(policy => new PolicyViewModel
            {
                Id = policy.ID,
                PolicyName = policy.PolicyName,
                LabelName = mipLabels.FirstOrDefault(label => label.Id == policy.MinLabelIdForAction)?.Name
            }).ToList();

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Add()
        {
            string? userId = User.Identity?.Name;
            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Fetch all Mip Labels for the user
            IList<MipLabel> mipLabels = _mipService.GetMipLabels(userId);

            var viewModel = new PolicyViewModel
            {
                Labels = mipLabels.ToList() // Assign the list of labels to the view model.
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PolicyViewModel viewModel)
        {
            var policy = new DataPolicy
            {
                PolicyName = viewModel.PolicyName,
                MinLabelIdForAction = viewModel.SelectedLabelId
            };

            await _context.DataPolicies.AddAsync(policy);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index"); 
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _context.DataPolicies.FindAsync(id);         
            if(policy == null)
            {
                return NotFound();
            }

            string? userId = User.Identity?.Name;
            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Fetch all Mip Labels for the user
            IList<MipLabel> mipLabels = _mipService.GetMipLabels(userId);

            var viewModel = new PolicyViewModel
            {
                Id = policy.ID,
                PolicyName = policy.PolicyName,
                SelectedLabelId = policy.MinLabelIdForAction,
                Labels = mipLabels.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PolicyViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var policy = await _context.DataPolicies.FindAsync(viewModel.Id);

                if (policy == null)
                {
                    return NotFound();
                }

                policy.PolicyName = viewModel.PolicyName;
                policy.MinLabelIdForAction = viewModel.SelectedLabelId;

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            string? userId = User.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.Labels = _mipService.GetMipLabels(userId).ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DataPolicy viewModel)
        {
            var policy = await _context.DataPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.ID == viewModel.ID);
            if (policy != null)
            {
                _context.DataPolicies.Remove(policy);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

    }

    
}
