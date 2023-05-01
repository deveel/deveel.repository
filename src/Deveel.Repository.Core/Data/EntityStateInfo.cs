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
