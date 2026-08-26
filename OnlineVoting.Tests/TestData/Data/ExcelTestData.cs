using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ExcelTestData
    {
        public static IFormFile CreateValidExcelFile()
        {
            ExcelPackage.License.SetNonCommercialPersonal("OnlineVoting.Tests");

            using MemoryStream stream = new();

            using (ExcelPackage package = new())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Students");

                worksheet.Cells[1, 1].Value = "FirstName";
                worksheet.Cells[1, 2].Value = "LastName";
                worksheet.Cells[1, 3].Value = "Email";

                worksheet.Cells[2, 1].Value = "Vincent";
                worksheet.Cells[2, 2].Value = "Achara";
                worksheet.Cells[2, 3].Value = "vincent@example.com";

                worksheet.Cells[3, 1].Value = "John";
                worksheet.Cells[3, 2].Value = "Doe";
                worksheet.Cells[3, 3].Value = "john@example.com";

                package.SaveAs(stream);
            }

            byte[] bytes = stream.ToArray();

            MemoryStream fileStream = new(bytes);

            return new FormFile(fileStream, 0, bytes.Length, "file", "students.xlsx");
        }

        public static IFormFile CreateExcelFileWithEmptyRequiredField()
        {
            ExcelPackage.License.SetNonCommercialPersonal("OnlineVoting.Tests");

            using MemoryStream stream = new();

            using (ExcelPackage package = new())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Students");

                worksheet.Cells[1, 1].Value = "FirstName";
                worksheet.Cells[1, 2].Value = "LastName";
                worksheet.Cells[1, 3].Value = "Email";

                worksheet.Cells[2, 1].Value = "Vincent";
                worksheet.Cells[2, 2].Value = "Achara";
                worksheet.Cells[2, 3].Value = null;

                package.SaveAs(stream);
            }

            byte[] bytes = stream.ToArray();

            MemoryStream fileStream = new(bytes);

            return new FormFile(fileStream, 0, bytes.Length, "file", "students.xlsx");
        }

        public static IFormFile CreateExcelFileWithNullableField()
        {
            ExcelPackage.License.SetNonCommercialPersonal("OnlineVoting.Tests");

            using MemoryStream stream = new();

            using (ExcelPackage package = new())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Students");

                worksheet.Cells[1, 1].Value = "FirstName";
                worksheet.Cells[1, 2].Value = "MiddleName";
                worksheet.Cells[1, 3].Value = "Email";

                worksheet.Cells[2, 1].Value = "Vincent";
                worksheet.Cells[2, 2].Value = null;
                worksheet.Cells[2, 3].Value = "vincent@example.com";

                package.SaveAs(stream);
            }

            byte[] bytes = stream.ToArray();

            MemoryStream fileStream = new(bytes);

            return new FormFile(fileStream, 0, bytes.Length, "file", "students.xlsx");
        }

        public static IFormFile CreateWrongFormatFile()
        {
            byte[] bytes = "test file"u8.ToArray();

            MemoryStream stream = new(bytes);

            return new FormFile(stream, 0, bytes.Length, "file", "students.csv");
        }
    }
}