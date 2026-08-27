using DinkToPdf;
using Microsoft.AspNetCore.Http;
using Moq;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class FileDataExtractorServiceTests
    {
        [Fact]
        public Task ExtractFromExcel_WithNullFile_ShouldThrowInvalidOperationException()
        {
            FileDataExtractorServiceFactory factory = new();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => factory.Service.ExtractFromExcel(null!));

            Assert.Equal("File is empty", exception.Message);

            return Task.CompletedTask;
        }

        [Fact]
        public Task ExtractFromExcel_WithWrongFileFormat_ShouldThrowInvalidDataException()
        {
            FileDataExtractorServiceFactory factory = new();

            IFormFile file = ExcelTestData.CreateWrongFormatFile();

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() => factory.Service.ExtractFromExcel(file));

            Assert.Equal("Incorrect file format", exception.Message);

            return Task.CompletedTask;
        }

        [Fact]
        public Task ExtractFromExcel_WithValidFile_ShouldReturnExcelData()
        {
            FileDataExtractorServiceFactory factory = new();

            IFormFile file = ExcelTestData.CreateValidExcelFile();

            List<Dictionary<string, string>> result = factory.Service.ExtractFromExcel(file);

            Assert.Equal(2, result.Count);

            Assert.Equal("Vincent", result[0]["FirstName"]);
            Assert.Equal("Achara", result[0]["LastName"]);
            Assert.Equal("vincent@example.com", result[0]["Email"]);

            Assert.Equal("John", result[1]["FirstName"]);
            Assert.Equal("Doe", result[1]["LastName"]);
            Assert.Equal("john@example.com", result[1]["Email"]);

            return Task.CompletedTask;
        }

        [Fact]
        public Task ExtractFromExcel_WithIgnoreFields_ShouldExcludeIgnoredFields()
        {
            FileDataExtractorServiceFactory factory = new();

            IFormFile file = ExcelTestData.CreateValidExcelFile();

            string[] ignoreFields = { "Email" };

            List<Dictionary<string, string>> result = factory.Service.ExtractFromExcel(file, ignoreFields: ignoreFields);

            Assert.Equal(2, result.Count);
            Assert.Contains("FirstName", result[0].Keys);
            Assert.Contains("LastName", result[0].Keys);
            Assert.DoesNotContain("Email", result[0].Keys);

            return Task.CompletedTask;
        }

        [Fact]
        public Task ExtractFromExcel_WithNullableField_ShouldAllowEmptyValue()
        {
            FileDataExtractorServiceFactory factory = new();

            IFormFile file = ExcelTestData.CreateExcelFileWithNullableField();

            string[] nullableFields = { "MiddleName" };

            List<Dictionary<string, string>> result = factory.Service.ExtractFromExcel(file, nullableFields: nullableFields);

            Assert.Single(result);
            Assert.Equal("Vincent", result[0]["FirstName"]);
            Assert.Null(result[0]["MiddleName"]);
            Assert.Equal("vincent@example.com", result[0]["Email"]);

            return Task.CompletedTask;
        }

        [Fact]
        public Task ExtractFromExcel_WithEmptyRequiredField_ShouldThrowInvalidDataException()
        {
            FileDataExtractorServiceFactory factory = new();

            IFormFile file = ExcelTestData.CreateExcelFileWithEmptyRequiredField();

            InvalidDataException exception = Assert.Throws<InvalidDataException>(() => factory.Service.ExtractFromExcel(file));

            Assert.Equal("Excel has empty fields. Crosscheck it and submit again", exception.Message);

            return Task.CompletedTask;
        }

        [Fact]
        public void ConvertToPDF_WithValidData_ShouldReturnPDF()
        {
            FileDataExtractorServiceFactory factory = new();

            string htmlString = "<html><body><h1>Election Result</h1></body></html>";
            string fileName = "election-result.pdf";
            byte[] pdfBytes = { 1, 2, 3, 4, 5 };

            factory.Converter.Setup(converter => converter.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(pdfBytes);

            PDFDto result = factory.Service.ConvertToPDF(htmlString, fileName);

            Assert.NotNull(result);
            Assert.Equal(fileName, result.FileName);
            Assert.Equal(pdfBytes, result.FileStream);

            factory.Converter.Verify(converter => converter.Convert(It.Is<HtmlToPdfDocument>(document => document.Objects.Count == 1 && document.Objects[0].HtmlContent == htmlString)), Times.Once);
        }
    }
}