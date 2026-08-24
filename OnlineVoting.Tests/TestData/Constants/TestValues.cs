namespace OnlineVoting.Tests.TestData.Constants
{
    public static class TestValues
    {
        public const string ValidEmail = "user@example.com";
        public const string InvalidEmail = "invalid-email";
        public const string ValidPassword = "Password123!";
        public const string ValidName = "John";
        public const string TooShortName = "J";
        public static readonly string TooLongName = new('A', 101);
        public const string ShortPassword = "Pass1!";
        public const string PasswordWithoutUppercase = "password123!";
        public const string PasswordWithoutLowercase = "PASSWORD123!";
        public const string PasswordWithoutNumber = "Password!";
        public const string PasswordWithoutSpecialCharacter = "Password123";
        public const string DifferentValidPassword = "NewPassword456!";
        public const string ValidPhoneNumber = "08012345678";
        public const string PhoneNumberWithoutLeadingZero = "80123456789";
        public const string TooShortPhoneNumber = "0801234567";
        public const string TooLongPhoneNumber = "080123456789";
        public const string PhoneNumberWithLetters = "08012ABC678";
        public const string ValidRegistrationNumber = "REG123456";
        public const string ValidRole = "Student";
        public const int ValidGenderId = 1;
        public const int ValidUserType = 2;
    }
}