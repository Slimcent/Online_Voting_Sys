namespace OnlineVoting.Models.Dtos.Response
{
    public class FacultyResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public IEnumerable<DepartmentResponse>? Departments { get; set; }
    }
}