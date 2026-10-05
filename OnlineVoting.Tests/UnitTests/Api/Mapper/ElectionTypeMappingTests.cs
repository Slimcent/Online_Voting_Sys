using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using System.Reflection;

namespace OnlineVoting.Tests.UnitTests.Api.Mapper
{
    public class ElectionTypeMappingTests
    {
        [Fact]
        public void ElectionType_ToElectionTypeResponse_ShouldMapCalculatedCounts()
        {
            MapperConfiguration configuration = new(config => config.AddMaps(Assembly.Load("OnlineVoting.Api")), NullLoggerFactory.Instance);

            IMapper mapper = configuration.CreateMapper();

            ElectionType electionType = ElectionTypeTestData.CreateElectionTypeWithElectionData();

            ElectionTypeResponse response = mapper.Map<ElectionTypeResponse>(electionType);

            Assert.Equal(2, response.NumberOfElectionPositions);
            Assert.Equal(3, response.NumberOfAspirants);
            Assert.Equal(1, response.NumberOfContestants);
        }

        [Fact]
        public void ElectionType_ToElectionTypeResponse_WithNoElectionData_ShouldReturnZeroCounts()
        {
            MapperConfiguration configuration = new(config => config.AddMaps(Assembly.Load("OnlineVoting.Api")), NullLoggerFactory.Instance);

            IMapper mapper = configuration.CreateMapper();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            ElectionTypeResponse response = mapper.Map<ElectionTypeResponse>(electionType);

            Assert.Equal(0, response.NumberOfElectionPositions);
            Assert.Equal(0, response.NumberOfAspirants);
            Assert.Equal(0, response.NumberOfContestants);
        }
    }
}