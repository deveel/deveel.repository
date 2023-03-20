using System;

using Microsoft.Extensions.Logging;

namespace Deveel.Data {
    static partial class LoggerExtensions {
        [LoggerMessage(Level = LogLevel.Error, 
            Message = "An unknwon error has occurred while operating on the entity '{EntityType}'")]
        public static partial void LogUnknownError(this ILogger logger, Exception error, Type entityType);
    }
}
