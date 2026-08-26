namespace OnlineVoting.Models.Dtos.Response
{
    public class DepartmentResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long FacultyId { get; set; }

        public string? FacultyName { get; set; }

        public bool Active { get; set; }
    }
}