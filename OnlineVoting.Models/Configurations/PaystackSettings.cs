namespace OnlineVoting.Models.Configurations
{
    public class PaystackSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string? CallbackUrl { get; set; }
    }
}
