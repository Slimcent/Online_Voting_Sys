using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Style;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Enums;
using OnlineVoting.Services.Utilities;

namespace OnlineVoting.Services.Extension
{
    public static class ExcelDownloadExtensions
    {
        public static FileStreamDto ConvertToExcel<T>(this IList<T> collection, ExcelDownloadConfig config)
        {
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var ep = new ExcelPackage(stream))
            {
                var worksheet = ep.Workbook.Worksheets.Add(config.Name);

                switch (config.ExcelType)
                {
                    case ExcelType.ClassSheet:
                        worksheet.ConfigureClassSheet(collection, config);

                        break;
                    case ExcelType.ScoreSheet:
                        worksheet.ConfigureScoreSheet(collection, config);

                        break;
                    default:
                        worksheet.Cells.LoadFromCollection(collection, true);
                        break;
                }


                worksheet.Protection.IsProtected = true;

                if (config.UnlockedColumns != null && config.UnlockedColumns.Any())
                {
                    foreach (var column in config.UnlockedColumns)
                        worksheet.Column(column).Style.Locked = false;

                }

                if (config.HiddenColumns != null && config.HiddenColumns.Any())
                {
                    foreach (var column in config.HiddenColumns)
                        worksheet.Column(column).Hidden = true;

                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                ep.Save();
            }
            stream.Position = 0;

            var fileName = $"{config.Name}-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";

            return new FileStreamDto { FileStream = stream, FileName = fileName };
        }

        private static void ConfigureScoreSheet<T>(this ExcelWorksheet worksheet, IList<T> collection, ExcelDownloadConfig config)
        {
            string[] titleDept = config.Title.Split(';');
            string title = titleDept.FirstOrDefault();
            string dept = titleDept.LastOrDefault();

            worksheet.Cells["C1:N1"].Merge = true;
            worksheet.Cells["C1:N1"].Value = dept;
            worksheet.Cells["C1:N1"].Style.Font.Bold = true;
            worksheet.Cells["C1:N1"].Style.Font.Size = 13;
            worksheet.Cells["C1:N1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["C2:N2"].Merge = true;
            worksheet.Cells["C2:N2"].Value = title;
            worksheet.Cells["C2:N2"].Style.Font.Bold = true;
            worksheet.Cells["C2:N2"].Style.Font.Size = 13;
            worksheet.Cells["C2:N2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["C3:N3"].Style.Font.Bold = true;

            worksheet.Cells[3, 1].LoadFromCollection(collection, true);

            int rowCount = worksheet.Dimension.Rows;

            for (int row = 4; row <= rowCount; row++)
            {
                worksheet.Cells[row, 9].Formula = "=IF(AND(F" + row + "=\"\", H" + row + "=\"\"), \"\",SUM(F" + row +
                                                   ":H" + row + "))";

                worksheet.Cells[row, 10].Formula = "=IF(I" + row + "= \"\", \"\", IF(OR(F" + row +
                                                   "=\"\", H" + row + "= \"\"), \"\", IF(I" + row + "<=" +
                                                   "39, \"F\", IF(I" + row + "<=" +
                                                   "45, \"E\", IF(I" + row + "<=" +
                                                   "49, \"D\", IF(I" + row + "<=" +
                                                   "59, \"C\", IF(I" + row + "<=" +
                                                   "69, \"B\", \"A\" " + ")))))))";

                worksheet.Cells[row, 14].Formula = "=IF(I" + row + "= \"\", \"ABS\", IF(OR(F" + row +
                                                   "=\"\",H" + row + "=\"\"),\"INC\", IF(I" + row + "<=" +
                                                   "39, \"Fail\", IF(I" + row + "<=" +
                                                   "45, \"Pass\", IF(I" + row + "<=" +
                                                   "49, \"Fair\", IF(I" + row + "<=" +
                                                   "59, \"Good\", IF(I" + row + "<=" +
                                                   "69, \"V.Good\", \"Excellent\" " + ")))))))";

                //Set the CA validation object
                IExcelDataValidationInt caVal = worksheet.Cells[row, 6].DataValidation.AddIntegerDataValidation();
                caVal.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                caVal.PromptTitle = "Enter a integer value here";
                caVal.Prompt = "Value should be between 0 and 30";
                caVal.ShowInputMessage = true;
                caVal.ErrorTitle = "An invalid value was entered";
                caVal.Error = "Value must be between 0 and 30";
                caVal.ShowErrorMessage = true;
                caVal.Operator = ExcelDataValidationOperator.between;
                caVal.Formula.Value = 0;
                caVal.Formula2.Value = 30;


                //Set the Exam validation object
                IExcelDataValidationInt examVal = worksheet.Cells[row, 8].DataValidation.AddIntegerDataValidation();
                examVal.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                examVal.PromptTitle = "Enter a integer value here";
                examVal.Prompt = "Value should be between 0 and 70";
                examVal.ShowInputMessage = true;
                examVal.ErrorTitle = "An invalid value was entered";
                examVal.Error = "Value must be between 0 and 70";
                examVal.ShowErrorMessage = true;
                examVal.Operator = ExcelDataValidationOperator.between;
                examVal.Formula.Value = 0;
                examVal.Formula2.Value = 70;


            }
        }

        private static void ConfigureClassSheet<T>(this ExcelWorksheet worksheet, IList<T> collection, ExcelDownloadConfig config)
        {

            string[] titleDept = config.Title.Split(';');
            string title = titleDept.FirstOrDefault();
            string dept = titleDept.LastOrDefault();

            worksheet.Cells["A1:C1"].Merge = true;
            worksheet.Cells["A1:C1"].Value = dept;
            worksheet.Cells["A1:C1"].Style.Font.Bold = true;
            worksheet.Cells["A1:C1"].Style.Font.Size = 13;
            worksheet.Cells["A1:C1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:C2"].Merge = true;
            worksheet.Cells["A2:C2"].Value = title;
            worksheet.Cells["A2:C2"].Style.Font.Bold = true;
            worksheet.Cells["A2:C2"].Style.Font.Size = 13;
            worksheet.Cells["A2:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A3:C3"].Style.Font.Bold = true;

            worksheet.Cells[3, 1].LoadFromCollection(collection, true);

        }
    }
}
