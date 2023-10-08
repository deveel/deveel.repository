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

namespace Deveel.Data {
	/// <summary>
	/// Lists the event identifiers that can be used to identify
	/// logged events from the <see cref="EntityManager{TEntity}"/>.
	/// </summary>
	public static class EntityManagerEventIds {
		// Errors

		/// <summary>
		/// An unknown error occurred during the operation.
		/// </summary>
		public const int UnknownError = -1000;

		/// <summary>
		/// An unknown error occurred during the operation
		/// on an entity.
		/// </summary>
		public const int EntityUnknownError = -1001;

		/// <summary>
		/// The entity to be added or updated is not valid.
		/// </summary>
		public const int EntityNotValid = -10010;

		// Warnings

		/// <summary>
		/// The entity was not modified during the operation.
		/// </summary>
		public const int EntityNotModified = 1003;

		/// <summary>
		/// The entity was not found in the repository.
		/// </summary>
		public const int EntityNotFound = 1004;

		/// <summary>
		/// The entity was not removed from the repository.
		/// </summary>
		public const int EntityNotRemoved = 1005;


		// Debugs

		/// <summary>
		/// An entity is being added to the repository.
		/// </summary>
		public const int AddingEntity = 100101;

		/// <summary>
		/// A range of entities is being added to the repository.
		/// </summary>
		public const int AddingEntityRange = 100102;

		/// <summary>
		/// An entity is being updated in the repository.
		/// </summary>
		public const int UpdatingEntity = 100104;

		/// <summary>
		/// An entity is being removed from the repository.
		/// </summary>
		public const int RemovingEntity = 100105;

		/// <summary>
		/// A range of entities is being removed from the repository.
		/// </summary>
		public const int RemovingEntityRange = 100106;

		// Information

		/// <summary>
		/// The entity was added to the repository.
		/// </summary>
		public const int EntityAdded = 2001;

		/// <summary>
		/// The entity was updated in the repository.
		/// </summary>
		public const int EntityUpdated = 2002;

		/// <summary>
		/// The entity was removed from the repository.
		/// </summary>
		public const int EntityRemoved = 2003;
	}
}
