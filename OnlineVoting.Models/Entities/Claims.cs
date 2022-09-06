namespace OnlineVoting.Models.Entities
{
    public class Claims
    {
        public long Id { get; set; }
        public string? Claim { get; set; }
        public string? MenuId { get; set; }
        public virtual Menu? Menu { get; set; }
    }
}
