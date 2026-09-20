namespace OnlineVoting.Models.Dtos.Request.BackgroundRequests
{
    public class ProcessStudentUploadRequest
    {
        public required CreateStudentRequest Student { get; set; }
    }
}