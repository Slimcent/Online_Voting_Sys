using MimeKit;

namespace OnlineVoting.Models.Dtos.Request.Email
{
    public class EmailDataDto
    {
        public MimeMessage? MessageBody { get; set; }
    }
}
