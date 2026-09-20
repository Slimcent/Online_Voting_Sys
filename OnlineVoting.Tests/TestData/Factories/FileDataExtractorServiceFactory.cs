using DinkToPdf.Contracts;
using Moq;
using OnlineVoting.Services.Implementation;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class FileDataExtractorServiceFactory
    {
        public Mock<IConverter> Converter { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        
        public FileDataExtractorServiceFactory()
        {
            Converter = new Mock<IConverter>();
            LoggerMessage = new Mock<ILoggerMessage>();
        }
    }
}