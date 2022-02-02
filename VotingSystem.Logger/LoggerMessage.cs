﻿using NLog;

namespace VotingSystem.Logger
{
    internal class LoggerMessage : ILoggerMessage
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public LoggerMessage()
        {
        }

        public void LogDebug(string message)
        {
            logger.Debug(message);
        }
        public void LogError(string message)
        {
            logger.Error(message);
        }
        public void LogInfo(string message)
        {
            logger.Info(message);
        }
        public void LogWarn(string message)
        {
            logger.Warn(message);
        }
    }
}
