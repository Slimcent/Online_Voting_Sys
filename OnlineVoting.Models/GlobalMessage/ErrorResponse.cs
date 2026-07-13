using OnlineVoting.Models.Enums;
using System.Text.Json;

namespace OnlineVoting.Models.GlobalMessage
{
    public class ResponseError
    {
        public ResponseStatus Status { get; set; }
        public string? Message { get; set; }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
