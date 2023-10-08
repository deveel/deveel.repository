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

using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	static partial class LoggerExtensions {
		[LoggerMessage(EntityManagerEventIds.UnknownError, LogLevel.Error, "An unknown error occurred during the operation.")]
		public static partial void LogUnknownError(this ILogger logger, Exception exception);

		[LoggerMessage(EntityManagerEventIds.EntityUnknownError, LogLevel.Error, "An unknown error occurred during the operation on the entity {EntityId}")]
		public static partial void LogEntityUnknownError(this ILogger logger, object? entityId, Exception exception);

		[LoggerMessage(EntityManagerEventIds.EntityNotValid, LogLevel.Error, "The entity to be added or updated is not valid.")]
		public static partial void LogEntityNotValid(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.EntityNotModified, LogLevel.Warning, "The entity {EntityId} was not modified during the operation.")]
		public static partial void LogEntityNotModified(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityNotFound, LogLevel.Warning, "The entity {EntityId} was not found in the repository.")]
		public static partial void LogEntityNotFound(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityNotRemoved, LogLevel.Warning, "The entity {EntityId} was not removed from the repository.")]
		public static partial void LogEntityNotRemoved(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.AddingEntity, LogLevel.Debug, "An entity is being added to the repository.")]
		public static partial void LogAddingEntity(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.UpdatingEntity, LogLevel.Debug, "The entity {EntityId} is being updated in the repository.")]
		public static partial void LogUpdatingEntity(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.RemovingEntity, LogLevel.Debug, "The entity {EntityId} is being removed from the repository.")]
		public static partial void LogRemovingEntity(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.AddingEntityRange, LogLevel.Debug, "A range of entities is being added to the repository.")]
		public static partial void LogAddingEntityRange(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.RemovingEntityRange, LogLevel.Debug, "A range of entities is being removed from the repository.")]
		public static partial void LogRemovingEntityRange(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.EntityAdded, LogLevel.Information, "The entity {EntityId} was added to the repository.")]
		public static partial void LogEntityAdded(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityRemoved, LogLevel.Information, "The entity {EntityId} was removed from the repository.")]
		public static partial void LogEntityRemoved(this ILogger logger, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityUpdated, LogLevel.Information, "The entity {EntityId} was updated in the repository.")]
		public static partial void LogEntityUpdated(this ILogger logger, object? entityId);
	}
}
