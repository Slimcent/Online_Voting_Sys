namespace OnlineVoting.Models.Dtos.Response
{
    public class FileStreamDto
    {
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
    }

    public class PDFDto
    {
        public byte[]? FileStream { get; set; }
        public string? FileName { get; set; }
    }
}
