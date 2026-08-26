using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Extension;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation
{
    public class FileDataExtractorService : IFileDataExtractorService
    {
        private readonly IConverter _converter;
        private readonly ILoggerMessage _loggerMessage;

        public FileDataExtractorService(IConverter converter, ILoggerMessage loggerMessage)
        {
            _converter = converter;
            _loggerMessage = loggerMessage;
        }

        public List<Dictionary<string, string>> ExtractFromExcel(IFormFile file, string[] nullableFields = null, string[] ignoreFields = null, int headerRow = 1, int contentRow = 2)
        {
            _loggerMessage.LogInfo($"Excel extraction request received for file {file?.FileName}.");

            if (file == null)
            {
                _loggerMessage.LogWarn("Excel extraction failed because the file was empty.");

                throw new InvalidOperationException("File is empty");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                _loggerMessage.LogWarn($"Excel extraction failed because file {file.FileName} has an incorrect format.");

                throw new InvalidDataException("Incorrect file format");
            }

            List<Dictionary<string, string>> excelData = new();

            List<string> possibleEmptyFields = new();


            if (nullableFields != null)
                possibleEmptyFields.AddRange(nullableFields);

            possibleEmptyFields = possibleEmptyFields.Select(c => c.ToLower()).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("OnlineVoting");
            using MemoryStream stream = new();
            file.CopyTo(stream);
            using ExcelPackage ep = new(stream);
            ExcelWorksheet worksheet = ep.Workbook.Worksheets.First();
            int rowCount = worksheet.Dimension.Rows;
            int columnCount = worksheet.Dimension.Columns;
            nullableFields = nullableFields?.Select(c => c.ToLower()).ToArray();
            ignoreFields = ignoreFields?.Select(c => c.ToLower()).ToArray();

            worksheet.Validate(nullableFields, columnCount);

            worksheet.Validate(ignoreFields, columnCount);


            for (int row = contentRow; row <= rowCount; row++)
            {
                Dictionary<string, string> cell = new();
                for (int column = 1; column <= columnCount; column++)
                {
                    string headerCell = worksheet.Cells[headerRow, column].Value.ToString();


                    if (ignoreFields != null && ignoreFields.Contains(headerCell?.ToLower()))
                        continue;

                    string value;
                    if (possibleEmptyFields.Any(l => !string.IsNullOrWhiteSpace(headerCell) && l == headerCell.ToLower()))
                    {
                        value = worksheet.Cells[row, column].Value?.ToString();
                    }
                    else if (worksheet.Cells[row, column].Value == null)
                    {
                        _loggerMessage.LogWarn($"Excel extraction failed because file {file.FileName} contains an empty required field.");

                        throw new InvalidDataException("Excel has empty fields. Crosscheck it and submit again");
                    }
                    else
                    {
                        value = worksheet.Cells[row, column].Value.ToString();
                    }

                    if (headerCell != null)
                        cell.Add(headerCell, value);
                }

                excelData.Add(cell);
            }
            ep.Save();

            _loggerMessage.LogInfo($"{excelData.Count} records extracted successfully from file {file.FileName}.");

            return excelData;
        }

        public PDFDto ConvertToPDF(string htmlString, string fileName)
        {
            _loggerMessage.LogInfo($"PDF conversion request received for file {fileName}.");

            GlobalSettings globalSettings = new()
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4Extra,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report"
            };

            ObjectSettings objectSettings = new()
            {
                PagesCount = true,
                HtmlContent = htmlString,
                WebSettings =
                {
                    DefaultEncoding = "utf-8",
                    //UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css",
                    //    "gradesheet_stylesheet.css")
                },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },

            };

            HtmlToPdfDocument pdf = new()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            byte[] file = _converter.Convert(pdf);

            _loggerMessage.LogInfo($"PDF conversion completed successfully for file {fileName}.");

            return new PDFDto() { FileStream = file, FileName = fileName };
        }


    }
}