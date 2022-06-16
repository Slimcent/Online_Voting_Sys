using Microsoft.AspNetCore.Http;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IFileDataExtractorService
    {
        List<Dictionary<string, string>> ExtractFromExcel(IFormFile file, string[] nullableFields = null, string[] ignoreFields = null, int headerRow = 1, int contentRow = 2);
        PDFDto ConvertToPDF(string htmlString, string fileName);
    }
}
