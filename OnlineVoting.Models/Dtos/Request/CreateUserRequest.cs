namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the common information required to create a user.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// The user's first name.
        /// </summary>
        /// <example>John</example>
        public required string FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        /// <example>Doe</example>
        public required string LastName { get; set; }

        /// <summary>
        /// The user's email address.
        /// </summary>
        /// <example>john.doe@example.com</example>
        public required string Email { get; set; }

        /// <summary>
        /// The user's telephone number.
        /// </summary>
        /// <example>+491234567890</example>
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// The identifier of the user's gender.
        /// </summary>
        /// <example>1</example>
        public required int GenderId { get; set; }

        /// <summary>
        /// The identifier of the user type.
        /// </summary>
        /// <example>2</example>
        public required int UserType { get; set; }

        /// <summary>
        /// The role assigned to the user.
        /// </summary>
        /// <example>Student</example>
        public required string Role { get; set; }
    }
}