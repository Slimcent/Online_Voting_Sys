namespace OnlineVoting.Services.Extension
{
    public static class LookupNormalizationExtension
    {
        public static string NormalizeLookupValue(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return string.Concat(value.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit));
        }

        public static string NormalizeGender(this string value)
        {
            string normalizedValue = value.NormalizeLookupValue();

            return normalizedValue switch
            {
                "m" => "male",
                "male" => "male",

                "f" => "female",
                "female" => "female",

                _ => normalizedValue
            };
        }
    }
}