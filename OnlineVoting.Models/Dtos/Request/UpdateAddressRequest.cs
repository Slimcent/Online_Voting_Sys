namespace OnlineVoting.Models.Dtos.Request
{
    /// <summary>
    /// Represents the address information required to update a user's address.
    /// </summary>
    public class UpdateAddressRequest
    {
        /// <summary>
        /// The plot or building number.
        /// </summary>
        /// <example>12</example>
        public int PlotNo { get; set; }

        /// <summary>
        /// The street name.
        /// </summary>
        /// <example>Main Street</example>
        public required string StreetName { get; set; }

        /// <summary>
        /// The city in which the address is located.
        /// </summary>
        /// <example>Paderborn</example>
        public required string City { get; set; }

        /// <summary>
        /// The state or region in which the address is located.
        /// </summary>
        /// <example>North Rhine-Westphalia</example>
        public required string State { get; set; }

        /// <summary>
        /// The nationality associated with the address record.
        /// </summary>
        /// <example>Nigerian</example>
        public required string Nationality { get; set; }
    }
}