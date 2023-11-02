// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	static partial class LoggerExtensions {
		[LoggerMessage(EventId = -100202, Level = LogLevel.Error,
			Message = "Unknown error while operating on the repository")]
		public static partial void LogUnknownError(this ILogger logger, Exception ex);

		[LoggerMessage(EventId = -100201, Level = LogLevel.Error,
						Message = "Unknown error while operating on the repository for entity {EntityId}")]
		public static partial void LogUnknownEntityError(this ILogger logger, Exception ex, object? entityId);

		[LoggerMessage(EventId = 1000231, Level = LogLevel.Debug, 
			Message = "Trying to find entity with ID {EntityId}")]
		public static partial void TraceFindingById(this ILogger logger, object? entityId);

		[LoggerMessage(EventId = 1000232, Level = LogLevel.Debug, 
			Message = "Trying to find entity with ID {EntityId} for tenant {TenantId}")]
		public static partial void TraceFindingByIdForTenant(this ILogger logger, string tenantId, object? entityId);

		[LoggerMessage(EventId = 1000233, Level = LogLevel.Debug, 
			Message = "An entity with ID {EntityId} found")]
		public static partial void TraceFoundById(this ILogger logger, object? entityId);

		[LoggerMessage(EventId = 1000234, Level = LogLevel.Debug, 
			Message = "An entity with ID {EntityId} found for tenant {TenantId}")]
		public static partial void TraceFoundByIdForTenant(this ILogger logger, string tenantId, object? entityId);

		[LoggerMessage(EventId = 100024, Level = LogLevel.Debug, Message = "Deleting entity {EntityId}")]
		public static partial void TraceDeleting(this ILogger logger, object entityId);

		[LoggerMessage(EventId = 1000241, Level = LogLevel.Debug, Message = "Deleting entity {EntityId} for tenant {TenantId}")]
		public static partial void TraceDeletingForTenant(this ILogger logger, string tenantId, object entityId);

		[LoggerMessage(EventId = 100025, Level = LogLevel.Debug, Message = "Entity {EntityId} deleted")]
		public static partial void TraceDeleted(this ILogger logger, object entityId);

		[LoggerMessage(EventId = 1000251, Level = LogLevel.Debug, Message = "Entity {EntityId} deleted for tenant {TenantId}")]
		public static partial void TraceDeletedForTenant(this ILogger logger, string tenantId, object entityId);


		[LoggerMessage(EventId = 100023, Level = LogLevel.Debug, Message = "Creating new entity")]
		public static partial void TraceCreating(this ILogger logger);

		[LoggerMessage(EventId = 1000236, Level = LogLevel.Debug, Message = "Creating new entity for tenant {TenantId}")]
		public static partial void TraceCreatingForTenant(this ILogger logger, string tenantId);

		[LoggerMessage(EventId = 100022, Level = LogLevel.Debug, Message = "Entity {EntityId} created")]
		public static partial void TraceCreated(this ILogger logger, object entityId);

		[LoggerMessage(EventId = 1000221, Level = LogLevel.Debug, Message = "Entity {EntityId} created for tenant {TenantId}")]
		public static partial void TraceCreatedForTenant(this ILogger logger, string tenantId, object entityId);

		[LoggerMessage(EventId = 100021, Level = LogLevel.Debug, Message = "Updating entity {EntityId}")]
		public static partial void TraceUpdating(this ILogger logger, string entityId);

		[LoggerMessage(EventId = 1000211, Level = LogLevel.Debug, Message = "Updating entity {EntityId} for tenant {TenantId}")]
		public static partial void TraceUpdatingForTenant(this ILogger logger, string tenantId, object entityId);

		[LoggerMessage(EventId = 100020, Level = LogLevel.Debug, Message = "Entity {EntityId} updated")]
		public static partial void TraceUpdated(this ILogger logger, object entityId);

		[LoggerMessage(EventId = 1000201, Level = LogLevel.Debug, Message = "Entity {EntityId} updated for tenant {TenantId}")]
		public static partial void TraceUpdatedForTenant(this ILogger logger, string tenantId, object entityId);
	}
}
