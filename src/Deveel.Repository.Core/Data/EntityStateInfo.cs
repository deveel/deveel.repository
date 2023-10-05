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

namespace Deveel.Data {
	/// <summary>
	/// A structure that holds information about the state of an entity
	/// </summary>
	/// <typeparam name="TStatus">
	/// The status of the entity, which can be an enumeration or a string
	/// </typeparam>
    public readonly struct EntityStateInfo<TStatus> {
		/// <summary>
		/// Constructs a new instance of the <see cref="EntityStateInfo{TStatus}"/> structure
		/// with the given information.
		/// </summary>
		/// <param name="status">
		/// The status of the entity, which can be an enumeration or a string
		/// </param>
		/// <param name="actorId">
		/// The identifier of the actor that caused the change of the state
		/// </param>
		/// <param name="timeStamp">
		/// The time stamp of the change of the state
		/// </param>
		/// <param name="data">
		/// A set of data associated with the state
		/// </param>
        public EntityStateInfo(TStatus status, string actorId, DateTimeOffset? timeStamp = null, IDictionary<string, object>? data = null) : this() {
            Status = status;
            ActorId = actorId;
            TimeStamp = timeStamp ?? DateTimeOffset.UtcNow;
            Data = data;
        }

		/// <summary>
		/// Gets the identifier of the actor that caused the change of the state
		/// </summary>
        public string ActorId { get; }

		/// <summary>
		/// Gets a set of data associated with the state
		/// </summary>
        public IDictionary<string, object>? Data { get; }

		/// <summary>
		/// Gets the time stamp of the change of the state
		/// </summary>
        public DateTimeOffset TimeStamp { get; }

		/// <summary>
		/// Gets the status of the entity, which can be an enumeration or a string
		/// </summary>
        public TStatus Status { get; }
    }
}
