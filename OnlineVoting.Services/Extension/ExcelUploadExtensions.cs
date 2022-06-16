using OfficeOpenXml;

namespace OnlineVoting.Services.Extension
{
    public static class ExcelUploadExtensions
    {
        public static void Validate(this ExcelWorksheet worksheet, string[] fields, int columnCount)
        {
            if (fields == null) return;
            foreach (var field in fields)
            {
                if (!Headers(worksheet, columnCount).Contains(field))
                    throw new InvalidDataException($"{field} Field Doesn't Exist");
            }
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

        private static List<string> Headers(ExcelWorksheet worksheet, int columnCount)
        {
            List<string> headers = new List<string>(columnCount);
            for (int column = 1; column <= columnCount; column++)
            {
                var header = worksheet.Cells[1, column].Value.ToString()?.ToLower();
                headers.Add(header);
            }
            return headers;
        }
    }
}
