namespace OnlineVoting.Models.Enums
{
    public enum UserType
    {
        Student = 1,
        Official
    }


    public static class UserTypeExtension
    {
        public static string? GetStringValue(this UserType userType)
        {
            return userType switch
            {
                UserType.Student => "Student",
                UserType.Official => "Official",
                _ => null
            };
        }
    }
}
