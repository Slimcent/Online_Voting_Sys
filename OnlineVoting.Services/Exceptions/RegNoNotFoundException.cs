using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Services.Exceptions
{
    public class RegNoNotFoundException : NotFoundException
    {
        public RegNoNotFoundException(string regNo)
            : base($"This RegNo: {regNo} does't exist in the database.")
        {
        }
    }
}
