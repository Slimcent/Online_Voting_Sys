using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Dtos.Request
{
    public class UpdateAddressRequest
    {
        public int PlotNo { get; set; }

        public required string StreetName { get; set; }

        public required string City { get; set; }

        public required string State { get; set; }

        public required string Nationality { get; set; }
    }
}