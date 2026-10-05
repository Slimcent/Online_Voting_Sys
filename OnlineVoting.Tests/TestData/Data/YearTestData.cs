using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class YearTestData
    {
        public static Year CreateYear(long id = 1, string name = "2026/2027", bool active = true)
        {
            return new Year
            {
                Id = id,
                Name = name,
                Active = active
            };
        }
    }
}
