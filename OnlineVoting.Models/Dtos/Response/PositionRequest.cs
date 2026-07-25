using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Response
{
    /// <summary>
    /// Represents pagination, sorting and search parameters
    /// used when retrieving positions.
    /// </summary>
    public class PositionRequest : RequestParameters
    {
        public PositionRequest()
        {
            OrderBy = "Name";
        }
    }
}