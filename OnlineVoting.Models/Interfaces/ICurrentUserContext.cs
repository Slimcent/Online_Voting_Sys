namespace OnlineVoting.Models.Interfaces
{
    public interface ICurrentUserContext
    {
        string? UserId { get; }

        string? Username { get; }
    }
}