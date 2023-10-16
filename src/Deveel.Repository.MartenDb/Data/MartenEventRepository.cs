using System.Reflection;

using JasperFx.Core;

using Marten;
using Marten.Events;
using Marten.Schema;

namespace Deveel.Data {
	/// <summary>
	/// A repository that aggregates events in a stream
	/// of events from MartenDb.
	/// </summary>
	/// <typeparam name="TAggregate">
	/// The type of entity that aggregates an event stream
	/// to form its state.
	/// </typeparam>
	/// <seealso cref="Aggregate"/>
	public class MartenEventRepository<TAggregate> : IRepository<TAggregate>
		where TAggregate : Aggregate {
		private IDocumentStore store;
		private MemberInfo? idMember;
		private PropertyInfo? versionProperty;

		/// <summary>
		/// Constructs the repository with the given
		/// document store.
		/// </summary>
		/// <param name="store">
		/// The document store that is used to access
		/// the event stream.
		/// </param>
		public MartenEventRepository(IDocumentStore store) {
			ArgumentNullException.ThrowIfNull(store, nameof(store));

			this.store = store;
		}

		/// <inheritdoc/>
		public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default) {
			IDocumentSession? session = null;

			try {
				// TODO: check if this is the best session type for this operation

				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				var key = GetAggregateIdentity(aggregate);

				if (key == null)
					throw new RepositoryException($"The aggregate '{typeof(TAggregate)}' has no key");

				if (key is string streamKey) {
					session.Events.StartStream<TAggregate>(streamKey, aggregate.Events.Uncommitted);
				} else if (key is Guid streamId) {
					session.Events.StartStream<TAggregate>(streamId, aggregate.Events.Uncommitted);
				} else {
					throw new RepositoryException($"The key '{key}' is not a valid string or GUID");
				}

				await session.SaveChangesAsync(cancellationToken);

				aggregate.Commit();
			} catch(RepositoryException) {
				throw;
			} catch (Exception ex) {
				//TODO: log the error
				throw new RepositoryException("Unknown error while adding an aggregate", ex);
			} finally {
				session?.Dispose();
			}
		}

		/// <inheritdoc/>
		public async Task AddRangeAsync(IEnumerable<TAggregate> entities, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(entities, nameof(entities));

			IDocumentSession? session = null;

			try {
				// TODO: check if this is the best session type for this operation
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				foreach (var aggregate in entities) {
					var key = GetAggregateIdentity(aggregate);
					// TODO: verify the version to be 0
					// var version = GetAggregateVersion(aggregate) ?? 0;

					if (key == null)
						throw new RepositoryException($"The aggregate '{typeof(TAggregate)}' has no key");

					if (key is string streamKey) {
						session.Events.StartStream<TAggregate>(streamKey, aggregate.Events.Uncommitted.ToArray());
					} else if (key is Guid streamId) {
						session.Events.StartStream<TAggregate>(streamId, aggregate.Events.Uncommitted.ToArray());
					} else {
						throw new RepositoryException($"The key '{key}' is not a valid string or GUID");
					}
				}

				await session.SaveChangesAsync(cancellationToken);

				foreach (var aggregate in entities) {
					aggregate.Commit();
				}
			} catch(RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while adding a range of aggregates", ex);
			} finally {
				session?.Dispose();
			}
		}
		
		/// <inheritdoc/>
		public async Task<TAggregate?> FindByKeyAsync(object key, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(key, nameof(key));

			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				TAggregate? aggregate = null;

				if (key is AggregateKey aggregateKey) {
					var version = aggregateKey.Version ?? 0L;
					if (aggregateKey.Key is string sKey) {
						aggregate = await session.Events.AggregateStreamAsync<TAggregate>(sKey, version, token: cancellationToken);
					} else if (aggregateKey.Key is Guid streamId) {
						aggregate = await session.Events.AggregateStreamAsync<TAggregate>(streamId, version, token: cancellationToken);
					}
				} else if (key is string sKey) {
					aggregate = await session.Events.AggregateStreamAsync<TAggregate>(sKey, token: cancellationToken);
				} else if (key is Guid streamId) {
					aggregate = await session.Events.AggregateStreamAsync<TAggregate>(streamId, token: cancellationToken);
				} else {
					throw new RepositoryException($"The key '{key}' is not a valid string or GUID");
				}

				if (aggregate != null)
					aggregate.Commit();

				return aggregate;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while finding an aggregate by key", ex);
			} finally {
				session?.Dispose();
			}
		}

		object? IRepository<TAggregate>.GetEntityKey(TAggregate entity)
			=> GetAggregateIdentity(entity);

		/// <summary>
		/// Gets the identity of the given aggregate.
		/// </summary>
		/// <param name="aggregate">
		/// The aggregate to get the identity from.
		/// </param>
		/// <returns>
		/// Returns the identity of the aggregate, or <c>null</c>
		/// if the aggregate has no identity.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when the key of the aggregate is not a valid
		/// identifier.
		/// </exception>
		public virtual object? GetAggregateIdentity(TAggregate aggregate) {
			if (idMember == null) {
				idMember = typeof(TAggregate)
					.GetMembers(BindingFlags.Public | BindingFlags.Instance)
					.Where(x => Attribute.IsDefined(x, typeof(IdentityAttribute)))
					.FirstOrDefault();

				if (idMember == null)
					throw new RepositoryException($"The aggregate '{typeof(TAggregate)}' has no key");
			}

			if (idMember == null)
				return null;

			object? id = null;

			if (idMember is PropertyInfo property) {
				id = property.GetValue(aggregate);
			} else if (idMember is FieldInfo field) {
				id = field.GetValue(aggregate);
			}

			if (id == null)
				return null;

			if (store.Options.Events.StreamIdentity == StreamIdentity.AsGuid) {
				if (id is Guid g)
					return g;
				if (id is string s)
					return Guid.Parse(s);
			} else if (store.Options.Events.StreamIdentity == StreamIdentity.AsString) {
				if (id is string s)
					return s;
				if (id is Guid g)
					return g.ToString();

				return id.ToString();
			}

			throw new RepositoryException($"The key of the aggregate '{typeof(TAggregate)}' is not a valid string or GUID");
		}

		/// <summary>
		/// Gets the version of the given aggregate.
		/// </summary>
		/// <param name="aggregate">
		/// The aggregate to get the version from.
		/// </param>
		/// <returns>
		/// Returns the version of the aggregate, or <c>null</c>
		/// if the aggregate has no version.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when the version of the aggregate is not a valid
		/// version number.
		/// </exception>
		public virtual long? GetAggregateVersion(TAggregate aggregate) {
			if (versionProperty == null) {
				versionProperty = typeof(TAggregate).GetProperties()
					.Where(x => Attribute.IsDefined(x, typeof(VersionAttribute)))
					.FirstOrDefault();
			}

			if (versionProperty == null)
				return null;

			var version = versionProperty.GetValue(aggregate);
			if (version == null)
				return null;

			if (version is string s)
				return Int64.Parse(s);

			if (version is int i)
				return i;
			if (version is long l)
				return l;

			throw new RepositoryException("The version of the aggregate is not a valid integer");
		}

		/// <inheritdoc/>
		public async Task<bool> RemoveAsync(TAggregate entity, CancellationToken cancellationToken = default) {
			IDocumentSession? session = null;

			try {
				// TODO: check if this is the best session type for this operation
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				var key = GetAggregateIdentity(entity);

				if (key == null)
					return false;

				if (key is string streamKey) {
					var state = await session.Events.FetchStreamStateAsync(streamKey, cancellationToken);
					if (state == null || state.IsArchived)
						return false;

					session.Events.ArchiveStream(streamKey);
				} else if (key is Guid streamId) {
					var state = await session.Events.FetchStreamStateAsync(streamId, cancellationToken);

					if (state == null || state.IsArchived)
						return false;

					session.Events.ArchiveStream(streamId);
				}

				await session.SaveChangesAsync(cancellationToken);

				return true;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while removing an aggregate", ex);
			} finally {
				session?.Dispose();
			}
		}

		/// <inheritdoc/>
		public async Task RemoveRangeAsync(IEnumerable<TAggregate> entities, CancellationToken cancellationToken = default) {
			IDocumentSession? session = null;

			try {
				// TODO: check if this is the best session type for this operation
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				foreach (var entity in entities) {
					var key = GetAggregateIdentity(entity);

					if (key == null)
						throw new RepositoryException($"The aggregate '{typeof(TAggregate)}' has no key");

					if (key is string streamKey) {
						var state = await session.Events.FetchStreamStateAsync(streamKey, cancellationToken);
						if (state == null || state.IsArchived)
							throw new RepositoryException($"The aggregate '{streamKey}' is not found");

						session.Events.ArchiveStream(streamKey);
					} else if (key is Guid streamId) {
						var state = await session.Events.FetchStreamStateAsync(streamId, cancellationToken);

						if (state == null || state.IsArchived)
							throw new RepositoryException($"The aggregate '{streamId}' is not found");

						session.Events.ArchiveStream(streamId);
					}
				}

				await session.SaveChangesAsync(cancellationToken);
			} catch(RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while removing a range of aggregates", ex);
			} finally {
				session?.Dispose();
			}
		}

		/// <inheritdoc/>
		public async Task<bool> UpdateAsync(TAggregate entity, CancellationToken cancellationToken = default) {
			IDocumentSession? session = null;

			try {
				session = await store.LightweightSerializableSessionAsync(cancellationToken);
				var key = GetAggregateIdentity(entity);
				var version = GetAggregateVersion(entity) ?? 0;

				if (key == null)
					return false;

				if (key is string streamKey) {
					var state = await session.Events.FetchStreamStateAsync(streamKey, cancellationToken);
					if (state == null || state.IsArchived)
						return false;

					if (version <= state.Version ||
						entity.Events.Uncommitted.Count == 0)
						return false;

					foreach (var @event in entity.Events.Uncommitted) {
						session.Events.Append(streamKey, version, @event);
					}
				} else if (key is Guid streamId) {
					var state = await session.Events.FetchStreamStateAsync(streamId, cancellationToken);

					if (state == null || state.IsArchived)
						return false;

					if (state.Version == version ||
						entity.Events.Uncommitted.Count == 0)
						return false;

					foreach (var @event in entity.Events.Uncommitted) {
						session.Events.Append(streamId, version, @event);
					}
				}

				await session.SaveChangesAsync(cancellationToken);

				entity.Commit();

				return true;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while updating an aggregate", ex);
			} finally {
				session?.Dispose();
			}
		}
	}
}
