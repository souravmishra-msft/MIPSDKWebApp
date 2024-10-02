using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using MIPSDK_WebApp1.Models.Entities;
using System.Data;

namespace MIPSDK_WebApp1.Services
{
    public class ExportFileService : IExportFileService
    {
        public Stream GenerateEmployeeExport(IEnumerable<Employee> employees)
        {
            DataTable dataTable = new DataTable("Employees");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Id"),
                new DataColumn("EmpId"),
                new DataColumn("FirstName"),
                new DataColumn("LastName"),
                new DataColumn("Title"),
                new DataColumn("Dob"),
                new DataColumn("HireDate"),
                new DataColumn("Salary"),
                new DataColumn("PAN")
            });

            foreach (var employee in employees)
            {
                dataTable.Rows.Add(
                    employee.Id,
                    employee.EmpId,
                    employee.FirstName,
                    employee.LastName,
                    employee.Title,
                    employee.Dob,
                    employee.HireDate,
                    employee.Salary,
                    employee.PAN);
            }

            return GenerateExcelFile(dataTable);
        }

        public Stream GenerateExcelFile(DataTable dataTable) {
            var stream = new MemoryStream();
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                wb.SaveAs(stream);
            }
            // Set the position to the begining of the stream.
            stream.Position = 0;
            return stream;
        }

        public List<Employee> ParseUpload(Stream upload)
        {
            List<Employee> employees = new List<Employee>();
            using (var wb = new XLWorkbook(upload))
            {
                var ws = wb.Worksheet(1);
                ws.Name = "EmployeeData";
                var firstCell = ws.FirstCellUsed();
                var lastCell = ws.LastCellUsed();

                var range = ws.Range(firstCell.Address, lastCell.CellRight().Address);
                range.FirstRow().Delete();

                var table = ws.Tables.Table("EmployeeData");

                foreach (var row in table.Rows())
                {
                    employees.Add(new Employee()
                    {
                        Id = Guid.Parse(row.Cell(1).GetString()),
                        FirstName = row.Cell(2).GetString(),
                        LastName = row.Cell(3).GetString(),
                        Title = row.Cell(4).GetString(),
                        Dob = row.Cell(5).GetDateTime(),
                        HireDate = row.Cell(6).GetDateTime(),
                        Salary = row.Cell(7).GetValue<decimal>(),
                        PAN = row.Cell(8).GetString()
                    });
                }
            }
            return employees;
        }
    }
}
