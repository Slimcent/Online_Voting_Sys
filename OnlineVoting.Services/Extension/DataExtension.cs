

using OfficeOpenXml;
using OnlineVoting.Models.Dtos.Request;
using System.Reflection;
using System.Text;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Extension
{
    public static class DataExtension
    {
        public static (List<Dictionary<string, string>> data, string[] errMsgs) ReadFromExcel(this ExtractDataFromExcelRequest request, ILoggerMessage loggerMessage)
        {
            if (request.File == null)
            {
                loggerMessage.LogWarn("Excel data extraction rejected because the file was empty.");

                throw new InvalidOperationException("File is empty");
            }

            if (!Path.GetExtension(request.File.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                loggerMessage.LogWarn($"Excel data extraction rejected because file {request.File.FileName} has an invalid format.");

                throw new InvalidDataException("Incorrect file format, you can only upload a .xlsx file format");
            }

            loggerMessage.LogInfo($"Starting Excel data extraction for file {request.File.FileName}.");

            List<Dictionary<string, string>> excelData = new();

            List<string> possibleEmptyFields = new();

            if (request.NullableColumns != null)
                possibleEmptyFields.AddRange(request.NullableColumns);

            possibleEmptyFields = possibleEmptyFields.Select(c => c.ToLower()).ToList();

            ExcelPackage.License.SetNonCommercialPersonal("Achara Obinna Vincent");

            using MemoryStream stream = new();
            request.File.CopyTo(stream);

            using ExcelPackage ep = new(stream);
            ExcelWorksheet worksheet = ep.Workbook.Worksheets.First();

            int rowCount = worksheet.Dimension.Rows;
            int columnCount = worksheet.Dimension.Columns;
            request.NullableColumns = request.NullableColumns?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.ToLower()).ToArray();
            request.ColumnsToSkip = request.ColumnsToSkip?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.ToLower()).ToArray();

            worksheet.Validate(request.RequiredColumns, columnCount, request.HeaderRow);

            worksheet.Validate(request.ColumnsToSkip, columnCount);

            StringBuilder sb = new();

            for (int row = request.ContentRow; row <= rowCount; row++)
            {
                Dictionary<string, string> cell = new();
                for (int column = 1; column <= columnCount; column++)
                {
                    string headerCell = worksheet.Cells[request.HeaderRow, column].Value.ToString();

                    if (request.ColumnsToSkip != null && request.ColumnsToSkip.Contains(headerCell?.ToLower()))
                        continue;

                    string value;
                    ExcelRange currentCell = worksheet.Cells[row, column];

                    if (!string.IsNullOrEmpty(currentCell.Formula)) currentCell.Calculate();

                    if (possibleEmptyFields.Any(l => !string.IsNullOrWhiteSpace(headerCell) && l == headerCell.ToLower()))
                        value = currentCell.Value?.ToString();
                    else if (worksheet.Cells[row, column].Value == null)
                        break;
                    else
                        value = worksheet.Cells[row, column].Value.ToString();

                    if (headerCell != null && ExcelUploadExtensions.IsValidAlphaNumeric(value))
                        cell.Add(headerCell, value);
                    else
                        sb.AppendLine(value);
                }

                if (cell.Keys.Count == request.RequiredColumns.Length)
                    excelData.Add(cell);
            }

            (List<Dictionary<string, string>> data, string distictErrMSg) = excelData.Distinct(request.UniqueColumns);

            string invalidCharErrMsg = string.IsNullOrEmpty(sb.ToString())
                ? string.Empty
                : sb.Insert(0, "The following values contains invalid characters (special characters or symbols) and the affected rows not uploaded !\nReview them and re-upload !\n")
                    .ToString();

            List<string> errMsg = new List<string>(2);

            if (!string.IsNullOrEmpty(invalidCharErrMsg)) errMsg.Add(invalidCharErrMsg);

            if (!string.IsNullOrEmpty(distictErrMSg)) errMsg.Add(distictErrMSg);

            ep.Save();

            if (errMsg.Any())
                loggerMessage.LogWarn($"Excel data extraction for file {request.File.FileName} completed with {errMsg.Count} validation warning(s).");

            loggerMessage.LogInfo($"Excel data extraction completed successfully for file {request.File.FileName}. Extracted {data.Count} record(s).");

            return (data, errMsg.ToArray());
        }

        public static T DictionaryToObject<T>(IDictionary<string, string> dict)
        {
            object? instance = Activator.CreateInstance(typeof(T));

            if (instance == null)
                throw new InvalidOperationException($"Could not create an instance of {typeof(T).Name}");

            T t = (T)instance;
            PropertyInfo[] properties = t.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (!dict.Any(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                KeyValuePair<string, string> item = dict.First(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                // Find which property type (int, string, double? etc) the CURRENT property is...
                Type tPropertyType = t.GetType().GetProperty(property.Name).PropertyType;

                // Fix nullables...
                Type newT = Nullable.GetUnderlyingType(tPropertyType) ?? tPropertyType;

                // ...and change the type
                object newA = Convert.ChangeType(item.Value, newT);
                t.GetType().GetProperty(property.Name).SetValue(t, newA, null);
            }

            return t;
        }

        public static IEnumerable<T> DictionaryToObjects<T>(List<Dictionary<string, string>> dictItems)
        {
            List<T> item = new List<T>(dictItems.Count);
            item.AddRange(dictItems.Select(c => DictionaryToObject<T>(c)).ToList());
            return item;
        }
    }
}
