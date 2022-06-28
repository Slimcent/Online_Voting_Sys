namespace OnlineVoting.Models.Enums
{
    public enum Gender
    {
        Male = 1,
        Female
    }

    public static class GenderExtension
    {
        public static string GetStringValue(this Gender gender)
        {
            return gender switch
            {
                Gender.Male => "Male",
                Gender.Female => "Female",
                _ => null
            };
        }
    }
}
