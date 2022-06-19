namespace OnlineVoting.Models.Dtos.Request
{
    public class EmailRequestDto
    {
        public string? ToEmail { get; set; }
        public string? FromEmail { get; set; }
        public string? ToName { get; set; }
        public string? FromName { get; set; }
        public string? AppUrl { get; set; }
    }
}
