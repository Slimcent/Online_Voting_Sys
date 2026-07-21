using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Response
{
    public class StaffRequestDto : RequestParameters
    {
        public StaffRequestDto()
        {
            OrderBy = "LastName";
        }
    }
}
