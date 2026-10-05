using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using System.Reflection;

namespace OnlineVoting.Tests.UnitTests.Mapper
{
    public class ElectionStatusMappingTests
    {
        private readonly IMapper _mapper;

        public ElectionStatusMappingTests()
        {
            MapperConfiguration configuration = new(config =>
                config.AddMaps(Assembly.Load("OnlineVoting.Api")), NullLoggerFactory.Instance);

            _mapper = configuration.CreateMapper();
        }

        [Fact]
        public void ElectionStatus_ToElectionStatusResponse_ShouldMapNumberOfElections()
        {
            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatusWithElectionData();

            ElectionStatusResponse response = _mapper.Map<ElectionStatusResponse>(electionStatus);

            Assert.Equal(electionStatus.Id, response.Id);
            Assert.Equal(electionStatus.Name, response.Name);
            Assert.Equal(electionStatus.Description, response.Description);
            Assert.Equal(electionStatus.Active, response.Active);
            Assert.Equal(2, response.NumberOfElections);
        }

        [Fact]
        public void UpdateElectionStatusRequest_ToElectionStatus_ShouldUpdateFieldsWithoutChangingId()
        {
            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(1);
            UpdateElectionStatusRequest request = ElectionStatusTestData.CreateUpdateElectionStatusRequest(
                id: 1,
                name: " Updated Draft ",
                description: " Updated election status description. ");

            _mapper.Map(request, electionStatus);

            Assert.Equal(1, electionStatus.Id);
            Assert.Equal("Updated Draft", electionStatus.Name);
            Assert.Equal("Updated election status description.", electionStatus.Description);
        }
    }
}