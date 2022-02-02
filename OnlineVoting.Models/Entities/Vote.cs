using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Entities
{
    public class Vote
    {
        public Guid Id { get; set; }
        public string StudentRegNo { get; set; }
        public  string ContestantRegNo { get; set; }

    }
}
