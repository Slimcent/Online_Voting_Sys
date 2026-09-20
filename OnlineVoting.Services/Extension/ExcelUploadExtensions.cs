using OfficeOpenXml;
using System.Text;
using System.Text.RegularExpressions;

namespace OnlineVoting.Services.Extension
{
    public static class ExcelUploadExtensions
    {
        public static void Validate(this ExcelWorksheet worksheet, string[] fields, int columnCount, int headerRow = 1)
        {
            if (fields == null) return;
            foreach (var field in fields)
                if (!Headers(worksheet, columnCount, headerRow).Contains(field.ToLower()))
                    throw new InvalidDataException($"{field} Field Doesn't Exist");
        }

        private static List<string> Headers(ExcelWorksheet worksheet, int columnCount, int headerRow)
        {
            List<string> headers = new List<string>(columnCount);
            for (int column = 1; column <= columnCount; column++)
            {
                var header = worksheet.Cells[headerRow, column].Value.ToString()?.ToLower();
                headers.Add(header);
            }

            return headers;
        }

        public static bool IsValidAlphaNumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;
            return Regex.Match(text, @"^[A-Za-z0-9\-\'\/\.\:\,\!\`\*\+\)\(\@\&\s\u202D\u202E\u200E\u200F\u202A\u202B\u202C_]+$", RegexOptions.Singleline).Success;
        }

        public static (List<Dictionary<string, string>> data, string errMSg) Distinct(this List<Dictionary<string, string>> source, string[]? columns)
        {
            StringBuilder sb = new StringBuilder();
            List<Dictionary<string, string>> listWithoutDuplicates = new List<Dictionary<string, string>>();

            if (columns == null) return (source, string.Empty);

            foreach (string column in columns)
            {
                listWithoutDuplicates = new List<Dictionary<string, string>>();

                StringBuilder msgBuilder = new StringBuilder();


                foreach (Dictionary<string, string> item in source)
                {
                    if (listWithoutDuplicates.Any(i => i.ContainsValue(item[column])))
                    {
                        Dictionary<string, string> itemToRemove =
                            listWithoutDuplicates.SingleOrDefault(l => l.ContainsValue(item[column]));

                        int indexOfItemToRemove = listWithoutDuplicates.IndexOf(itemToRemove);

                        listWithoutDuplicates.RemoveAt(indexOfItemToRemove);
                        msgBuilder.AppendLine(item[column]);
                        continue;
                    }

                    if (!listWithoutDuplicates.Contains(item) && item.ContainsKey(column)) listWithoutDuplicates.Add(item);
                }

                source = listWithoutDuplicates.ToList();

                string duplicateRecordsErrorMsg = string.IsNullOrEmpty(msgBuilder.ToString())
                    ? string.Empty
                    : msgBuilder.Insert(0,
                            $"The following '{column}' values are duplicates and not uploaded !\nReview them and re-upload !\n")
                        .ToString();

                if (!string.IsNullOrEmpty(duplicateRecordsErrorMsg)) sb.AppendLine(duplicateRecordsErrorMsg);
            }

            return (listWithoutDuplicates, sb.ToString());
        }

        public static void ValidateFields(this List<Dictionary<string, string>> source, string[] fields)
        {
            var equal = source.FirstOrDefault()?.Select(c => c.Key).OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .SequenceEqual(fields.Select(c => c).OrderBy(c => c, StringComparer.OrdinalIgnoreCase));

            if (equal != null && !equal.Value)
                throw new InvalidOperationException("The excel sheet uploaded is not for this purpose, crosscheck your headers or download the sample excel provided");
        }

        public static void CheckDuplicate(this List<Dictionary<string, string>> source, string column)
        {
            List<string> list = new List<string>();

            foreach (var row in source)
            {
                list.Add(row[column]);
            }

            if (list.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                throw new InvalidOperationException("Data contains duplicate records");
            }
        }
    }
}
