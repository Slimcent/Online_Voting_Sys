using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Response
{
    public class PositionRequestDto : RequestParameters
    {
        public PositionRequestDto()
        {
            OrderBy = "Name";
        }
    }
}
