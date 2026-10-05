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

        public static class Roles
        {
            public const string StudentRoleId = "e0bddae0-4027-4415-aeb0-458753b9a636";
            public const string SuperAdminRoleId = "cbdc547b-31ae-4406-97e6-e597a98028f2";
        }

        public static class UserTypes
        {
            public const int StudentUserTypeId = 3;
            public const int OfficialUserTypeId = 4;
        }

        public static class Authentication
        {
            public static class Messages
            {
                public const string InvalidCredentials = "Invalid email or password.";
                public const string InactiveAccount = "Account is not active. Contact the administrator.";
            }
        }

        public static class ElectionStatusCodes
        {
            public const string Draft = "DRAFT";
            public const string RegistrationOpen = "REGISTRATION_OPEN";
            public const string RegistrationClosed = "REGISTRATION_CLOSED";
            public const string VotingOpen = "VOTING_OPEN";
            public const string Completed = "COMPLETED";
            public const string Cancelled = "CANCELLED";
        }

        public static class ElectionScopeCodes
        {
            public const string University = "UNIVERSITY";
            public const string Faculty = "FACULTY";
            public const string Department = "DEPARTMENT";
        }

        public static class PositionApplicationStatuses
        {
            public const string PendingPayment = "PENDING_PAYMENT";
            public const string PendingReview = "PENDING_REVIEW";
            public const string Approved = "APPROVED";
            public const string Rejected = "REJECTED";
            public const string Withdrawn = "WITHDRAWN";
        }

        public static class InvoiceStatuses
        {
            public const string Unpaid = "UNPAID";
            public const string Paid = "PAID";
            public const string Cancelled = "CANCELLED";
        }

        public static class PaymentStatuses
        {
            public const string Pending = "PENDING";
            public const string Succeeded = "SUCCEEDED";
            public const string Failed = "FAILED";
            public const string Cancelled = "CANCELLED";
        }

        public static class IdempotencyStatuses
        {
            public const string Processing = "Processing";
            public const string Completed = "Completed";
            public const string Failed = "Failed";
        }

        public static class IdempotencyOperations
        {
            public const string CreatePositionApplication = "CreatePositionApplication";
            public const string InitiatePayment = "InitiatePayment";
        }

        public static class PaymentGateways
        {
            public const string Paystack = "PAYSTACK";
            public const string Flutterwave = "FLUTTERWAVE";
        }

        public static class GatewayPaymentStatuses
        {
            public const string Pending = "PENDING";
            public const string Succeeded = "SUCCEEDED";
            public const string Failed = "FAILED";
            public const string Cancelled = "CANCELLED";
        }

        public static class RoleNames
        {
            public const string Student = "student";
            public const string Admin = "admin";
            public const string SuperAdmin = "super-admin";
        }
    }
}