namespace Deveel.Data {
	/// <summary>
	/// Provides a base implementation of an object
	/// that aggregates a stream of events to form
	/// its state.
	/// </summary>
	public abstract class Aggregate {
		/// <summary>
		/// Constructs the aggregate from the initial set of
		/// committed events.
		/// </summary>
		/// <param name="committedEvents"></param>
		protected Aggregate(IEnumerable<object>? committedEvents = null) {
			Events = new EventCollection(this, committedEvents);

			foreach (var @event in Events.Committed) {
				ApplyEvent(@event);
			}
		}

		/// <summary>
		/// Gets the stream of events that are applied to
		/// the aggregate.
		/// </summary>
		public EventCollection Events { get; }

		/// <summary>
		/// Gets the version of the aggregate.
		/// </summary>
		/// <remarks>
		/// The default implementation returns the total number
		/// of events in the stream.
		/// </remarks>
		[Version]
		public virtual int Version => Events.Count;

		/// <summary>
		/// Applies the given event to the aggregate, changing
		/// its state and incrementing the version.
		/// </summary>
		/// <param name="event">
		/// The event to apply to the aggregate.
		/// </param>
		public void Apply(object @event) {
			ArgumentNullException.ThrowIfNull(@event, nameof(@event));

			ApplyEvent(@event);

			Events.Add(@event);
		}

		/// <summary>
		/// When overridden in a derived class, applies the given
		/// event to the aggregate, changing its state.
		/// </summary>
		/// <param name="event">
		/// The event to apply to the aggregate.
		/// </param>
		protected virtual void ApplyEvent(object @event) {
		}

		/// <summary>
		/// Commits the current stream of events to the aggregate.
		/// </summary>
		public void Commit() => Events.Commit();

		/// <summary>
		/// Rolls back the current stream of uncommitted events 
		/// to the aggregate.
		/// </summary>
		public void Rollback() => Events.Clear();
	}
}
