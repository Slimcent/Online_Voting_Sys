using Microsoft.AspNetCore.Http;

namespace OnlineVoting.Models.Dtos.Request
{
    public class ExtractDataFromExcelRequest
    {
        public IFormFile File { get; set; }
        public string[]? NullableColumns { get; set; } = null;
        public string[]? ColumnsToSkip { get; set; } = null;
        public string[]? RequiredColumns { get; set; } = null;
        public string[]? UniqueColumns { get; set; } = null;
        public int HeaderRow { get; set; } = 1;
        public int ContentRow { get; set; } = 2;
    }
}
