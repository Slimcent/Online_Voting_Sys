namespace OnlineVoting.Models.Pagination
{
    public class PagedResponse<T> where T : class
    {
        public MetaData? MetaData { get; set; }

        public IEnumerable<T>? Items { get; set; }
    }
}
