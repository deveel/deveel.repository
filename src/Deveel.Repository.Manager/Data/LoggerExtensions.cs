﻿// Copyright 2023 Deveel AS
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
        // Errors
		[LoggerMessage(EntityManagerEventIds.UnknownError, LogLevel.Error, "An unknown error occurred during a management operation of entities of type {EntityType}.")]
		public static partial void LogUnknownError(this ILogger logger, Type entityType, Exception exception);

		[LoggerMessage(EntityManagerEventIds.EntityUnknownError, LogLevel.Error, "An unknown error occurred during the operation on the entity of type {EntityType} identified by {EntityId}")]
		public static partial void LogEntityUnknownError(this ILogger logger, Type entityType, object? entityId, Exception exception);

		[LoggerMessage(EntityManagerEventIds.EntityNotValid, LogLevel.Error, "The entity of type {EntityType} to be added or updated is not valid.")]
		public static partial void LogEntityNotValid(this ILogger logger, Type entityType);

		[LoggerMessage(EntityManagerEventIds.EntityNotCached, LogLevel.Error, "The entity of type {EntityType} identified by {EntityId} was not cached.")]
		public static partial void LogEntityNotCached(this ILogger logger, Exception error, Type entityType, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityNotEvicted, LogLevel.Error, "The entity of type {EntityType} identified by {EntityId} was not removed from the cache.")]
		public static partial void LogEntityNotEvicted(this ILogger logger, Exception error, Type entityType, object? entityId);

        // Warnings
		[LoggerMessage(EntityManagerEventIds.EntityNotModified, LogLevel.Warning, "The entity of type {EntityType} identified by {EntityId} was not modified during the operation.")]
		public static partial void LogEntityNotModified(this ILogger logger, Type entityType, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityNotFound, LogLevel.Warning, "The entity of type {EntityType} identified by {EntityId} was not found in the repository.")]
		public static partial void LogEntityNotFound(this ILogger logger, Type entityType, object? entityId);

		[LoggerMessage(EntityManagerEventIds.EntityNotRemoved, LogLevel.Warning, "The entity of type {EntityType} identified by {EntityId} was not removed from the repository.")]
		public static partial void LogEntityNotRemoved(this ILogger logger, Type entityType, object? entityId);

        // Debugs
		[LoggerMessage(EntityManagerEventIds.AddingEntity, LogLevel.Debug, "An entity of type {EntityType} is being added to the repository.")]
		public static partial void LogAddingEntity(this ILogger logger, Type entityType);

		[LoggerMessage(EntityManagerEventIds.UpdatingEntity, LogLevel.Debug, "The entity of type {EntityType} identified by {EntityId} is being updated in the repository.")]
		public static partial void LogUpdatingEntity(this ILogger logger, Type entityType, object? entityId);

		[LoggerMessage(EntityManagerEventIds.RemovingEntity, LogLevel.Debug, "The entity of tpye {EntityType} identified by {EntityId} is being removed from the repository.")]
		public static partial void LogRemovingEntity(this ILogger logger, Type entityType, object? entityId);

		[LoggerMessage(EntityManagerEventIds.AddingEntityRange, LogLevel.Debug, "A range of entities of type {EntityType} is being added to the repository.")]
		public static partial void LogAddingEntityRange(this ILogger logger, Type entityType);

		[LoggerMessage(EntityManagerEventIds.RemovingEntityRange, LogLevel.Debug, "A range of entities is being removed from the repository.")]
		public static partial void LogRemovingEntityRange(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.FindingEntityByKey, LogLevel.Debug, "Attempting to find an entity of type {EntityType} using the key {EntityKey}")]
		public static partial void LogFindingEntityByKey(this ILogger logger, Type entityType, object? entityKey);

		[LoggerMessage(EntityManagerEventIds.FindingFirstEntityByQuery, LogLevel.Debug, "Attempting to find an entity of type {EntityType} using a query filter")]
		public static partial void LogFindingFirstEntityByQuery(this ILogger logger, Type entityType);

		[LoggerMessage(EntityManagerEventIds.FindingAllEntitiesByQuery, LogLevel.Debug, "Attempting to find a range of entities of type {EntityType} using a query filter")]
		public static partial void LogFindingAllEntitiesByQuery(this ILogger logger, Type entityType);

		[LoggerMessage(EntityManagerEventIds.GettingEntityPage, LogLevel.Debug, "Page {PageNumber} of {PageSize} entities of type {EntityType} is being requested from the repository")]
		public static partial void LogGettingEntityPage(this ILogger logger, Type entityType, int pageNumber, int pageSize);

		[LoggerMessage(EntityManagerEventIds.CountingEntities, LogLevel.Debug, "The count of entities of type {EntityType} is being requested from the repository")]
		public static partial void LogCountingEntities(this ILogger logger, Type entityType);

		[LoggerMessage(EntityManagerEventIds.EntityFoundByKey, LogLevel.Debug, "An entity of type {EntityType} identified by {EntityKey} has been found repository")]
		public static partial void LogEntityFoundByKey(this ILogger logger, Type entityType, object? entityKey);

        // Information

		[LoggerMessage(EntityManagerEventIds.EntityAdded, LogLevel.Information, "The entity {EntityId} was added to the repository.")]
		public static partial void LogEntityAdded(this ILogger logger, object? entityId);

        [LoggerMessage(EntityManagerEventIds.EntityRangeAdded, LogLevel.Information, "A range of entities was added to the repository.")]
        public static partial void LogEntityRangeAdded(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.EntityRemoved, LogLevel.Information, "The entity {EntityId} was removed from the repository.")]
		public static partial void LogEntityRemoved(this ILogger logger, object? entityId);

        [LoggerMessage(EntityManagerEventIds.EntityRangeRemoved, LogLevel.Information, "A range of entities was removed from the repository.")]
        public static partial void LogEntityRangeRemoved(this ILogger logger);

		[LoggerMessage(EntityManagerEventIds.EntityUpdated, LogLevel.Information, "The entity {EntityId} was updated in the repository.")]
		public static partial void LogEntityUpdated(this ILogger logger, object? entityId);
	}
}
