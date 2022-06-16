using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace OnlineVoting.Services.Infrastructures
{
    public static class MessageEncoder
    {
        public static string EncodeString(string message)
        {
            var encodedBytes = Encoding.UTF8.GetBytes(message);
            var encodedMessage = WebEncoders.Base64UrlEncode(encodedBytes);

            return encodedMessage;
        }

        public static string DecodeString(string message)
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(message);
            var decodedMessage = Encoding.UTF8.GetString(decodedBytes);

            return decodedMessage;
        }
    }
}
