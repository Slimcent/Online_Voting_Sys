namespace OnlineVoting.Models.Enums
{
    public enum EmailTemplatePath
    {
        Default = 1,
    }

    public static class EmailTemplatePathExtension
    {
        public static string GetStringValue(this EmailTemplatePath emailTemplatePath)
        {
            return emailTemplatePath switch
            {
                EmailTemplatePath.Default => Path.Combine("wwwroot", "html", "EmailTemplate.html"),
                _ => null
            };
        }
    }
}