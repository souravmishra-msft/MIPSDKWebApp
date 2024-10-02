using Microsoft.AspNetCore.Mvc;
using MIPSDK_WebApp1.Models.Entities;
using System.Data;

namespace MIPSDK_WebApp1.Services
{
    public interface IExportFileService
    {
        public Stream GenerateExcelFile(DataTable dataTable);
        public Stream GenerateEmployeeExport(IEnumerable<Employee> employees);

        public List<Employee> ParseUpload(Stream upload);

    }
}
