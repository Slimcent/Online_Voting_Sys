namespace OnlineVoting.Api.SeedData.Model
{
    public class Seed
    {
        public IEnumerable<string>? Roles { get; set; }

        public AdminUser? AdminUser { get; set; }
    }
}
