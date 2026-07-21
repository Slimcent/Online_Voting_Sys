namespace OnlineVoting.Models.Dtos.Request
{
    public class CreateUserRequest
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required string PhoneNumber { get; set; }

        public required int GenderId { get; set; }

        public required int UserType { get; set; }

        public required string Role { get; set; }
    }
}