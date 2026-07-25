using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Response
{
    public class RoleRequest : RequestParameters
    {
        public RoleRequest()
        {
            OrderBy = "Name";
        }
    }
}
