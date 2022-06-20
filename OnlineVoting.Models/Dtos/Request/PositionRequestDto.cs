using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    public class PositionRequestDto : RequestParameters
    {
        public PositionRequestDto()
        {
            OrderBy = "Name";
        }
    }
}
