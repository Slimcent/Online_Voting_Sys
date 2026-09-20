namespace OnlineVoting.Models.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }

        public required string TokenHash { get; set; }

        public required string FamilyId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime FamilyExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public string? RevokedReason { get; set; }

        public string? CreatedByIp { get; set; }

        public string? RevokedByIp { get; set; }

        public string? UserAgent { get; set; }

        public required string UserId { get; set; }

        public virtual User User { get; set; } = null!;

        public byte[] RowVersion { get; set; } = [];

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsFamilyExpired => DateTime.UtcNow >= FamilyExpiresAt;

        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsActive => !IsExpired && !IsFamilyExpired && !IsRevoked;
    }
}