using System.Collections;

namespace Deveel.Data {
	/// <summary>
	/// A collection of events that are applied to an aggregate.
	/// </summary>
	public sealed class EventCollection : IReadOnlyCollection<object> {
		private readonly List<EventState> events;

		/// <summary>
		/// Constructs the collection from the initial
		/// set of committed events.
		/// </summary>
		/// <param name="aggregate">
		/// The aggregate that the events are applied to.
		/// </param>
		/// <param name="committedEvents">
		/// The initial set of events that are committed to
		/// form the aggregate.
		/// </param>
		internal EventCollection(Aggregate aggregate, IEnumerable<object>? committedEvents = null) {
			ArgumentNullException.ThrowIfNull(aggregate, nameof(aggregate));

			Aggregate = aggregate;

			events = new List<EventState>();

			if (committedEvents != null) {
				foreach (var @event in committedEvents) {
					events.Add(new EventState(@event, true));
				}
			}
		}

		/// <summary>
		/// Gets the aggregate that the events are applied to.
		/// </summary>
		public Aggregate Aggregate { get; }

		/// <summary>
		/// Gets the total number of events in the collection.
		/// </summary>
		public int Count {
			get {
				lock (events) {
					return events.Count;
				}
			}
		}

		/// <inheritdoc/>
		public IReadOnlyList<object> Committed {
			get {
				lock (events) {
					return events.Where(x => x.IsCommitted).Select(x => x.Event).ToArray();
				}
			}
		}

		/// <inheritdoc/>
		public IReadOnlyList<object> Uncommitted {
			get {
				lock (events) {
					return events.Where(x => !x.IsCommitted).Select(x => x.Event).ToArray();
				}
			}
		}

		/// <summary>
		/// Adds a new event to the collection as uncommitted.
		/// </summary>
		/// <param name="event">
		/// The event to add to the collection.
		/// </param>
		/// <remarks>
		/// The event is added to the collection as uncommitted,
		/// that means that it is not yet part of the aggregate
		/// and that can be removed or cleared.
		/// </remarks>
		internal void Add(object @event) {
			lock (events) {
				events.Add(new EventState(@event, false));
			}
		}

		/// <summary>
		/// Clears all the uncommitted events from the collection.
		/// </summary>
		internal void Clear() {
			lock (events) {
				for (var i = events.Count - 1; i >= 0; i--) {
					if (!events[i].IsCommitted)
						events.RemoveAt(i);
				}
			}
		}

		internal void Commit() {
			lock (events) {
				for (var i = events.Count - 1; i >= 0; i--) {
					if (!events[i].IsCommitted)
						events[i] = new EventState(events[i].Event, true);
				}
			}
		}

		/// <summary>
		/// Checks if the given event is contained in the collection.
		/// </summary>
		/// <param name="event">
		/// The event to check.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the event is contained in the
		/// collection, otherwise <c>false</c>.
		/// </returns>
		public bool Contains(object @event) {
			lock (events) {
				return events.Any(x => x.Event.Equals(@event));
			}
		}

		/// <inheritdoc/>
		public IEnumerator<object> GetEnumerator() {
			lock (events) {
				return events.Select(x => x.Event).GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private readonly struct EventState {
			internal EventState(object @event, bool isCommitted) {
				Event = @event;
				IsCommitted = isCommitted;
			}

			public object Event { get; }

			public bool IsCommitted { get; }
		}
	}
}
