using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Response
{
    public class RoleRequestDto : RequestParameters
    {
        public RoleRequestDto()
        {
            OrderBy = "Name";
        }
    }
}
