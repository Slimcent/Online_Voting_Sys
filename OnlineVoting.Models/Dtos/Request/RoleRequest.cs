using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    public class RoleRequest : RequestParameters
    {
        public RoleRequest()
        {
            OrderBy = "Name";
        }
    }
}
