using System;

namespace Deveel.Data {
	/// <summary>
	/// A structure that describes the status of an entity
	/// </summary>
	/// <typeparam name="TStatus">The status type of the state</typeparam>
    public readonly struct EntityStateInfo<TStatus> {
		/// <summary>
		/// Constructs a new entity state structure
		/// </summary>
		/// <param name="status">The status of the entity</param>
		/// <param name="actorId">The actor (user or system) that set 
		/// the status to the entity</param>
		/// <param name="timeStamp">The exact time-stamp of the state</param>
		/// <param name="data">An optional set of metadata that describes the state</param>
        public EntityStateInfo(TStatus status, string actorId, DateTimeOffset? timeStamp = null, IDictionary<string, object>? data = null) : this() {
            Status = status;
            ActorId = actorId;
            TimeStamp = timeStamp ?? DateTimeOffset.UtcNow;
            Data = data;
        }

		/// <summary>
		/// Gets the identifier of the actor (user or system)
		/// that operated the state
		/// </summary>
        public string ActorId { get; }

		/// <summary>
		/// Gets an optional set of metadata describing the state
		/// </summary>
        public IDictionary<string, object>? Data { get; }

		/// <summary>
		/// Gets the exact time-stamp of the state
		/// </summary>
        public DateTimeOffset TimeStamp { get; }

		/// <summary>
		/// Gets the status of the entity
		/// </summary>
        public TStatus Status { get; }
    }
}
