using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Services.Exceptions
{
    public class RegNoExistException : NotFoundException
    {
        public RegNoExistException(string? regNo)
         : base($"This RegNo: {regNo} already exist in the database.")
        {
        }
    }
}
