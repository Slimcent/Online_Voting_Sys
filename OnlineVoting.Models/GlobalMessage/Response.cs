using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.GlobalMessage
{
    public class Response
    {
        public Response()
        {

        }

        public Response(bool success, string message)
        {
            Success = success;

            Message = message;
        }

        public Response(bool success, string message, dynamic _object) : this(success, message)
        {
            Object = _object;
        }

        public bool Success { get; set; }

        public string Message { get; set; }

        public dynamic Object { get; set; }
    }
}
