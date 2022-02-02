using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Dtos.Request
{
    public class VoteRequestDto
    {
        public string ContestantRegNo { get; set; }
        public string VoterRegNo { get; set; }
    }
}
