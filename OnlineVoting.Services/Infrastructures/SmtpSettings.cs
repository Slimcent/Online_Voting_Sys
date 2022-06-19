namespace OnlineVoting.Services.Infrastructures
{
    public class SmtpSettings
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? AppUrl { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? Password { get; set; }
    }
}
