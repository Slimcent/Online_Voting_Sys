using MimeKit;
using OnlineVoting.Models.Dtos.Request.Email;

namespace OnlineVoting.Services.Extension
{
    public class EmailExtension
    {
        public static EmailDataDto SendVoterEmailData(EmailRequestDto request)
        {
            MimeMessage emailMessage = new MimeMessage();

            MailboxAddress from = new MailboxAddress(request.FromName, request.FromEmail);
            emailMessage.From.Add(from);

            MailboxAddress to = new MailboxAddress(request.ToName, request.ToEmail);
            emailMessage.To.Add(to);

            emailMessage.Subject = $"Voting Registration";

            emailMessage.Date = DateTime.Now;

            string filePath = Directory.GetCurrentDirectory() + "\\Template\\EmailTemplate.html";
            string MailText = GetFilePath(filePath);

            MailText = MailText.Replace("[Header]", $"Hello {request.ToName}").
                Replace("[Body]", $"Your voting registration was successful and your voting code is {request.VotingCode}.\n" +
                $"Please, do not disclose it to anyone").
                Replace("[Button-Text]", "Return to site");

            BodyBuilder emailBodyBuilder = new BodyBuilder();
            emailBodyBuilder.HtmlBody = MailText;
            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            EmailDataDto emailData = new EmailDataDto()
            {
                MessageBody = emailMessage
            };

            return emailData;
        }

        private static string GetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "File not found";

            StreamReader str = new StreamReader(path);
            string MailText = str.ReadToEnd();
            str.Close();

            return MailText;
        }
    }
}
