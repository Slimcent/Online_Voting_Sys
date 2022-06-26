namespace OnlineVoting.Services.Exceptions
{
    public class StudentNotFoundException : NotFoundException
    {
        public StudentNotFoundException(string? regNo)
            : base($"The student with RegNo: {regNo} does't exist in the database.")
        {
        }
    }
}
