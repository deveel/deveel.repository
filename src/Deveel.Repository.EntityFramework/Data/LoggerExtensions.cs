using System;

using Microsoft.Extensions.Logging;

namespace Deveel.Data {
    static partial class LoggerExtensions {
        [LoggerMessage(EventId = LogEventIds.UnknownError, Level = LogLevel.Error, 
            Message = "An unknwon error has occurred while operating on the entity '{EntityType}'")]
        public static partial void LogUnknownError(this ILogger logger, Exception error, Type entityType);

        [LoggerMessage(EventId = LogEventIds.CreatingEntity, Level = LogLevel.Trace, 
                       Message = "Creating a new entity of type '{EntityType}' for tenant '{TenantId}'")]
        public static partial void TraceCreatingEntity(this ILogger logger, Type entityType, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.UpdatingEntity, Level = LogLevel.Trace, 
                                  Message = "Updating an entity of type '{EntityType}' (ID={EntityId}) owned by tenant '{TenantId}'")]
        public static partial void TraceUpdatingEntity(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.DeletingEntity, Level = LogLevel.Trace, 
                                             Message = "Deleting an entity of type '{EntityType}' (ID={EntityId}) owned by tenant '{TenantId}'")]
        public static partial void TraceDeletingEntity(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.FindingById, Level = LogLevel.Trace, 
                       Message = "Finding an entity of type '{EntityType}' with ID '{EntityId}' owned by tenant '{TenantId}'")]
        public static partial void TraceFindingById(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityCreated, Level = LogLevel.Information, 
                                  Message = "Entity of type '{EntityType}' with ID '{EntityId}' was created for tenant '{TenantId}'")]
        public static partial void LogEntityCreated(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityUpdated, Level = LogLevel.Information, 
                                             Message = "Entity of type '{EntityType}' (ID={EntityId}) updated for tenant '{TenantId}'")]
        public static partial void LogEntityUpdated(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityDeleted, Level = LogLevel.Information, 
                                                        Message = "Entity of type '{EntityType}' (ID={EntityId}) deleted for tenant '{TenantId}'")]
        public static partial void LogEntityDeleted(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityFoundById, Level = LogLevel.Trace, 
                                  Message = "Entity of type '{EntityType}' found with ID '{EntityId}' for tenant '{TenantId}'")]
        public static partial void TraceEntityFoundById(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityNotFoundById, Level = LogLevel.Trace, 
                                                        Message = "Entity of type '{EntityType}' not found with ID '{EntityId}' for tenant '{TenantId}'")]
        public static partial void TraceEntityNotFoundById(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityNotFound, Level = LogLevel.Warning, 
            Message = "Entity of type '{EntityType}' with ID '{EntityId}' not found for tenant '{TenantId}'")]
        public static partial void WarnEntityNotFound(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityNotDeleted, Level = LogLevel.Warning, 
                       Message = "Entity of type '{EntityType}' with ID '{EntityId}' not deleted for tenant '{TenantId}'")]
        public static partial void WarnEntityNotDeleted(this ILogger logger, Type entityType, string entityId, string? tenantId);

        [LoggerMessage(EventId = LogEventIds.EntityNotUpdated, Level = LogLevel.Warning, 
                                             Message = "Entity of type '{EntityType}' with ID '{EntityId}' not updated for tenant '{TenantId}'")]
        public static partial void WarnEntityNotUpdated(this ILogger logger, Type entityType, string entityId, string? tenantId);
    }
}
