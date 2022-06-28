using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    public class StaffRequestDto : RequestParameters
    {
        public StaffRequestDto()
        {
            OrderBy = "LastName";
        }
    }
}
