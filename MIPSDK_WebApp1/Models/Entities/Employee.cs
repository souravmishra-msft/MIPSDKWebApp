using System.ComponentModel.DataAnnotations;

namespace MIPSDK_WebApp1.Models.Entities
{
    public class Employee
    {
        [Key]
        public Guid Id { get; set; }
        public string EmpId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string PAN { get; set; }
    }
}
