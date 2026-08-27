using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the pagination, sorting and search parameters
    /// used when retrieving staff members.
    /// </summary>
    public class StaffRequest : RequestParameters
    {
        public StaffRequest()
        {
            OrderBy = "LastName";
        }
    }
}