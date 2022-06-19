using OfficeOpenXml.Attributes;

namespace OnlineVoting.Models.Dtos.Request
{
    public class StudentListDownload
    {
        [EpplusTableColumn(Order = 1)]
        public int SN { get; set; }

        [EpplusTableColumn(Order = 2)]
        public string? RegNo { get; set; }

        [EpplusTableColumn(Order = 3)]
        public string? FirstName { get; set; }

        [EpplusTableColumn(Order = 4)]
        public string? LastName { get; set; }

        [EpplusTableColumn(Order = 5)]
        public string? Email { get; set; }

        [EpplusTableColumn(Order = 6)]
        public string? PhoneNumber{ get; set; }

        [EpplusTableColumn(Order = 7)]
        public string? Gender{ get; set; }
    }
}
