using OnlineVoting.Models.Enums;

namespace OnlineVoting.Services.Utilities
{
    public class ExcelDownloadConfig
    {
        public string? Title { get; set; }
        public string? Name { get; set; }
        public int[]? UnlockedColumns { get; set; }
        public int[]? HiddenColumns { get; set; }
        public ExcelType ExcelType { get; set; }
    }
}
