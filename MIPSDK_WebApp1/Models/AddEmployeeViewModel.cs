using MIPSDK_WebApp1.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace MIPSDK_WebApp1.Models
{
    public class AddEmployeeViewModel
    {
        public Guid Id { get; set; }
        public string EmpId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string PAN { get; set; }
        public string SuccessMessage { get; set; }

        
        // Add policy-related properties
        public List<PolicyViewModel> Policies { get; set; }

    }
}
