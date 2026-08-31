namespace OnlineVoting.Models.Constants
{
    public static class ApplicationConstants
    {
        public static class Audit
        {
            public static class Events
            {
                public const string LoginSucceeded = "LoginSucceeded";
                public const string LoginFailed = "LoginFailed";
                public const string AccountLocked = "AccountLocked";
                public const string LoginRejectedLocked = "LoginRejectedLocked";
            }

            public static class Outcomes
            {
                public const string Success = "Success";
                public const string Failure = "Failure";
                public const string Denied = "Denied";
            }

            public static class Descriptions
            {
                public const string LoginSucceeded = "User logged in successfully.";
                public const string InvalidCredentials = "Login failed because invalid credentials were provided.";
                public const string AccountLocked = "Account temporarily locked after repeated failed login attempts.";
                public const string LoginRejectedLocked = "Login rejected because the account is temporarily locked.";
                public const string InactiveAccount = "Login rejected because the account is not active.";
            }

            public static class EntityTypes
            {
                public const string User = "User";
            }
        }

        public static class Authentication
        {
            public static class Messages
            {
                public const string InvalidCredentials = "Invalid email or password.";
                public const string InactiveAccount = "Account is not active. Contact the administrator.";
            }
        }
    }
}