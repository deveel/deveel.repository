using System;

using Microsoft.Extensions.Logging;

namespace Deveel.Repository {
    public static partial class LoggerExtensions {
        [LoggerMessage(Level = LogLevel.Error, 
            Message = "Unknwon error while operating on the repository at '{DatabaseName}'.'{CollectionName}' - {ErrorMessage}")]
        public static partial void LogUnknownError(this ILogger logger, Exception error, string databaseName, string collectionName, string errorMessage);
    }
}
