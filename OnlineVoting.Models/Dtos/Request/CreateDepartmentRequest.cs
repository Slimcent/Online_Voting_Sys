namespace OnlineVoting.Models.Dtos.Request
{
    public class CreateDepartmentRequest
    {
        public string? Name { get; set; }
        public List<string>? Names { get; set; }
        public int FacultyId { get; set; }
    }
}