using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace OnlineVoting.Services.Infrastructures
{
    public static class MessageEncoder
    {
        public static string EncodeString(string message)
        {
            byte[] encodedBytes = Encoding.UTF8.GetBytes(message);
            string encodedMessage = WebEncoders.Base64UrlEncode(encodedBytes);

            return encodedMessage;
        }

        public static string DecodeString(string message)
        {
            return ValidateAndDecodeString(message);
        }

        private static string ValidateAndDecodeString(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Encoded message cannot be empty.");

            try
            {
                byte[] decodedBytes = WebEncoders.Base64UrlDecode(message);
                string decodedMessage = Encoding.UTF8.GetString(decodedBytes);

                return decodedMessage;
            }
            catch (FormatException)
            {
                throw new ArgumentException($"Encoded message {message} is invalid base64url string.");
            }
        }
    }
}