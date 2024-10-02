using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.InformationProtection.Exceptions;
using MIPSDK_WebApp1.Data;
using MIPSDK_WebApp1.Models;
using MIPSDK_WebApp1.Models.Entities;
using MIPSDK_WebApp1.Services;
using System.Data;

namespace MIPSDK_WebApp1.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly MipService _mipService;
        private readonly IExportFileService _exportFileService;

        // Constructor
        public EmployeesController(ApplicationDBContext context, MipService mipService, IExportFileService exportFileService)
        {
            _context = context;
            _mipService = mipService;
            _exportFileService = exportFileService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddEmployeeViewModel viewModel)
        {
            // Generate a random 6-digit number for EmpId
            string empId;
            do
            {
                empId = GenerateRandomEmpId();
            } while (await _context.Employees.AnyAsync(e => e.EmpId == empId));

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmpId = empId,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Title = viewModel.Title,
                Dob = viewModel.Dob.Date,
                HireDate = viewModel.HireDate.Date,
                Salary = viewModel.Salary,
                PAN = viewModel.PAN
            };

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            // Set the success message
            viewModel.SuccessMessage = "Employee added successfully";

            // Clear the form fields
            ModelState.Clear();

            // Optionally, create a new instance of the viewModel
            viewModel = new AddEmployeeViewModel();

            return RedirectToAction("List", "Employees");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            // Fetch employees
            var employees = await _context.Employees.ToListAsync();
            
            // Fetch data policies
            var policies = await _context.DataPolicies.ToListAsync();

            // Fetch all Mip Labels for the user
            string? userId = User.Identity?.Name;
            if(string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            IList<MipLabel> labels = _mipService.GetMipLabels(userId);

            // Prepare the policy view model with label names
            var policyViewModels = policies.Select(policy => new PolicyViewModel
            {
                Id = policy.ID,
                PolicyName = policy.PolicyName,
                LabelName = labels.FirstOrDefault(label => label.Id == policy.MinLabelIdForAction)?.Name
            }).ToList();

            // Prepare the view model
            var viewModel = new AddEmployeeViewModel
            {
                Policies = policyViewModels,
            };

            ViewBag.Employees = employees;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Employee viewModel)
        {
            var employee = await _context.Employees.FindAsync(viewModel.Id);

            if(employee != null)
            {
                employee.FirstName = viewModel.FirstName;
                employee.LastName = viewModel.LastName;
                employee.Title = viewModel.Title;
                employee.Dob = viewModel.Dob.Date;
                employee.HireDate = viewModel.HireDate.Date;
                employee.Salary = viewModel.Salary;
                employee.PAN = viewModel.PAN;

                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction("List", "Employees");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Employee viewModel)
        {
            var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == viewModel.Id);

            if(employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("List", "Employees");
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToFile(string query)
        {
            try
            {
                // Fetch the employees from the DB
                var employees = await _context.Employees.ToListAsync();

                // Generate the file stream for excel
                var excelStream = _exportFileService.GenerateEmployeeExport(employees);

                // Get the label ID for the "Download Policy"
                string labelId = _context.DataPolicies.First(d => d.PolicyName == "Download Policy").MinLabelIdForAction;               

                // Apply the label to the file stream.
                MemoryStream mipStream = _mipService.ApplyMipLabel(excelStream, labelId);

                // Set stream position to the begining for both streams
                mipStream.Position = 0;

                // Define the filename for the exported file.
                var filename = "EmployeeList.xlsx";

                // Return the file for download
                return File(mipStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            } catch (LabelNotFoundException ex)
            {
                Console.WriteLine($"Label not found: {ex.Message}");

                // Return an error view with a specific error message
        return View("Error", new ErrorViewModel { ErrorMessage = "Label not found. Export failed." });
            } catch (Exception ex)
            {
                // Log other possible errors
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Return an error view or message
                return View("Error", new ErrorViewModel { ErrorMessage = "An unexpected error occurred. Export failed." });
            }
        }

        private string GenerateRandomEmpId()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

    }
}
