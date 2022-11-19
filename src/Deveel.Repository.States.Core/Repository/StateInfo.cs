using System;

using Deveel.States;

namespace Deveel.Repository {
	/// <summary>
	/// Describes a single state of an entity at a given time.
	/// </summary>
	/// <typeparam name="TStatus">The status code of the state</typeparam>
	public struct StateInfo<TStatus> : IState<TStatus> {
		/// <summary>
		/// Constructs a new state with the given status, at a given time, and a
		/// reference to the actor that created it
		/// </summary>
		/// <param name="status">The status code of the state.</param>
		/// <param name="actor">The referece to the actor (eg. an individual or an application)
		/// that created the status for the entity.</param>
		/// <param name="timeStamp">The exact time-stamp of the state.</param>
		/// <exception cref="ArgumentException">
		/// Thrown if he value of <paramref name="actor"/> is <c>null</c> or an empty string. It is
		/// also thrown if the type of <see cref="TStatus"/> is not an enumeration.
		/// </exception>
		public StateInfo(TStatus status, string actor, DateTimeOffset timeStamp) : this() {
			if (string.IsNullOrWhiteSpace(actor))
				throw new ArgumentException($"'{nameof(actor)}' cannot be null or whitespace.", nameof(actor));
			if (!typeof(TStatus).IsEnum)
				throw new ArgumentException($"The type '{typeof(TStatus)}' is not an enumeration");

			Status = status;
			TimeStamp = DateTimeOffset.UtcNow;
			Actor = actor;
			Data = new Dictionary<string, object>();
			TimeStamp = timeStamp;
		}

		/// <summary>
		/// Constructs a new state with the given status and a
		/// reference to the actor that created it
		/// </summary>
		/// <param name="status">The status code of the state.</param>
		/// <param name="actor">The referece to the actor (eg. an individual or an application)
		/// that created the status for the entity.</param>
		/// <remarks>
		/// <para>
		/// Using this constructor assigns <see cref="DateTimeOffset.UtcNow"/> to <see cref="TimeStamp"/>:
		/// it is possible to eventually modify this value later, although it's not recommended. 
		/// </para>
		/// <para>
		/// If the business logic of the application requires the specific assignment of the
		/// time-stamp of the state, the other constuctor is recommended. 
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown if he value of <paramref name="actor"/> is <c>null</c> or an empty string. It is
		/// also thrown if the type of <see cref="TStatus"/> is not an enumeration.
		/// </exception>
		public StateInfo(TStatus status, string actor)
			: this(status, actor, DateTimeOffset.UtcNow) {
		}

		/// <inheritdoc/>
		public TStatus Status { get; }

		/// <inheritdoc/>
		public DateTimeOffset TimeStamp { get; set; }

		/// <inheritdoc/>
		public string Actor { get; }

		/// <inheritdoc/>
		public IDictionary<string, object> Data { get; set; }
	}
}