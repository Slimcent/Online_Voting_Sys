using Moq;
using OnlineVoting.Services.Implementation;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class ServiceFactoryTests
    {
        [Fact]
        public void GetService_WithRegisteredService_ShouldReturnService()
        {
            Mock<IServiceProvider> serviceProvider = new();
            TestService expectedService = new();

            serviceProvider.Setup(provider => provider.GetService(typeof(TestService))).Returns(expectedService);

            ServiceFactory serviceFactory = new(serviceProvider.Object);

            TestService result = serviceFactory.GetService<TestService>();

            Assert.Same(expectedService, result);

            serviceProvider.Verify(provider => provider.GetService(typeof(TestService)), Times.Once);
        }

        [Fact]
        public void GetService_WithUnregisteredService_ShouldThrowInvalidOperationException()
        {
            Mock<IServiceProvider> serviceProvider = new();

            serviceProvider.Setup(provider => provider.GetService(typeof(TestService))).Returns(null);

            ServiceFactory serviceFactory = new(serviceProvider.Object);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => serviceFactory.GetService<TestService>());

            Assert.Equal("Type Not Supported", exception.Message);

            serviceProvider.Verify(provider => provider.GetService(typeof(TestService)), Times.Once);
        }

        private class TestService
        {
        }
    }
}