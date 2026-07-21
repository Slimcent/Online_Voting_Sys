namespace OnlineVoting.Models.Dtos.Request
{
    public class CreateStudentRequest : CreateUserRequest
    {
        public required int DepartmentId { get; set; }
    }
}