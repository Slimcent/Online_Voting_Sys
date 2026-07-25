namespace OnlineVoting.Models.Pagination
{
    /// <summary>
    /// Provides common pagination, sorting and search parameters.
    /// </summary>
    public abstract class RequestParameters
    {
        private const int MaxPageSize = 50;

        private int _pageSize = 10;

        /// <summary>
        /// The requested page number.
        /// </summary>
        /// <example>1</example>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// The number of records returned per page.
        /// The maximum permitted value is 50.
        /// </summary>
        /// <example>10</example>
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value > MaxPageSize ? MaxPageSize : value;
            }
        }

        /// <summary>
        /// The property used to order the results.
        /// </summary>
        /// <example>LastName</example>
        public string? OrderBy { get; set; }

        /// <summary>
        /// The text used to filter the results.
        /// </summary>
        /// <example>John</example>
        public string? SearchTerm { get; set; }
    }
}