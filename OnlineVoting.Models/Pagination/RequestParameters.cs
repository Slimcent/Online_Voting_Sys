namespace OnlineVoting.Models.Pagination
{
    /// <summary>
    /// Provides common pagination, sorting and search parameters.
    /// </summary>
    public abstract class RequestParameters
    {
        private const int MaxPageSize = 100;

        private int _pageNumber = 1;
        private int _pageSize = 10;

        /// <summary>
        /// The requested page number.
        /// </summary>
        /// <example>1</example>
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                _pageNumber = value < 1 ? 1 : value;
            }
        }

        /// <summary>
        /// The number of records returned per page.
        /// The maximum permitted value is 100.
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
                if (value < 1)
                {
                    _pageSize = 1;
                    return;
                }

                _pageSize = value > MaxPageSize ? MaxPageSize : value;
            }
        }

        /// <summary>
        /// The property used to order the results.
        /// </summary>
        /// <example>Name</example>
        public string? OrderBy { get; set; }

        /// <summary>
        /// The text used to filter the results.
        /// </summary>
        /// <example>Item</example>
        public string? SearchTerm { get; set; }
    }
}