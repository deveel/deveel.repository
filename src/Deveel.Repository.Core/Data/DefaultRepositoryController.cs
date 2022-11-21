using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
	public class DefaultRepositoryController : IRepositoryController {
		private readonly RepositoryControllerOptions options;
		private readonly IServiceProvider serviceProvider;
		private readonly ILogger logger;

		public DefaultRepositoryController(IOptions<RepositoryControllerOptions> options, IServiceProvider serviceProvider, ILogger<DefaultRepositoryController>? logger = null)
			: this(options, serviceProvider, (ILogger?) logger) {
		}

		protected DefaultRepositoryController(IOptions<RepositoryControllerOptions> options, IServiceProvider serviceProvider, ILogger? logger = null) {
			this.options = options.Value;
			this.serviceProvider = serviceProvider;
			this.logger = logger ?? NullLogger.Instance;
		}

		private void LogTrace(string message, params object?[] args) {
			if (logger.IsEnabled(LogLevel.Trace))
				logger.LogTrace(message, args);
		}

		private void LogError(Exception ex, string message, params object?[] args) {
			if (logger.IsEnabled(LogLevel.Error))
				logger.LogError(ex, message, args);
		}

		private IControllableRepository? RequireRepository<TEntity>()
			where TEntity : class, IEntity {

			LogTrace("Resolving a repository for entity of type {EntityType}", typeof(TEntity).Name);

			var repository = serviceProvider.GetService<IRepository<TEntity>>() as IRepository;
			if (repository == null) {
				LogTrace("No generic repository handling the entity '{EntityType}' was found: trying the base forms", typeof(TEntity).Name);

				// less optimal lookup
				repository = serviceProvider.GetServices<IRepository>()
					.FirstOrDefault(x => x.EntityType == typeof(TEntity));
			}
			
			if (repository == null)
				throw new NotSupportedException($"Unable to resolve any repository for entities of type {typeof(TEntity)}");

			LogTrace("The repository of type '{RepositoryType}' was resolved handling type '{EntityType}'", 
				repository.GetType().Name, typeof(TEntity).Name);

			return RequireControllable(repository);
		}

		private IControllableRepository? RequireControllable(IRepository repository) {
			if (!(repository is IControllableRepository controllable)) {
				if (!options.IgnoreNotControllable)
					throw new NotSupportedException($"The repository of type '{repository.GetType()}' is not controllable");

				return null;
			}

			return controllable;
		}

		private IRepositoryProvider<TEntity> RequireRepositoryProvider<TEntity>()
			where TEntity : class, IEntity {
			var provider = serviceProvider.GetService<IRepositoryProvider<TEntity>>();

			if (provider == null)
				throw new NotSupportedException($"Unable to resolve any repository provider for entities of type '{typeof(TEntity)}'");

			return provider;
		}

		private async Task CreateRepository(IControllableRepository? repository, CancellationToken cancellationToken) {
			if (repository != null) {
				try {
					if (await repository.ExistsAsync(cancellationToken)) {
						if (options.DeleteIfExists) {
							LogTrace("The repository already exists and the controller is deleting it first");

							await DropRepository(repository, cancellationToken);
						} else if (options.DontCreateExisting) {
							if (logger.IsEnabled(LogLevel.Warning))
								logger.LogWarning("The repository '{RepositoryType}' already exists and the controller is not deleting it",
									repository.GetType().FullName);
							return;
						} else {
							throw new RepositoryException("The repository already exists");
						}
					}

					LogTrace("Creating the repository");

					await repository.CreateAsync(cancellationToken);

					LogTrace("Repository created");
				} catch (NotSupportedException ex) {
					LogError(ex, "Not Supported Error while creating the repository {RepositoryType} handling the type {EntityType}",
						repository.GetType().FullName, repository.EntityType.Name);
					throw;
				} catch (RepositoryException ex) {
					LogError(ex, "Not Supported Error while creating the repository {RepositoryType} handling the type {EntityType}",
						repository.GetType().FullName, repository.EntityType.Name);
					throw;
				} catch(Exception ex) {
					LogError(ex, "Not Supported Error while creating the repository {RepositoryType} handling the type {EntityType}",
						repository.GetType().FullName, repository.EntityType.Name);

					throw new RepositoryException($"Unable to create the repository '{repository.GetType().Name}'", ex);
				}
			}
		}

		private async Task DropRepository(IControllableRepository? repository, CancellationToken cancellationToken) {
			if (repository != null) {
				try {
					if (!await repository.ExistsAsync(cancellationToken)) {
						LogTrace("The repository does not exist: it will be not deleted");
					} else {
						LogTrace("Dropping the repository");

						await repository.DropAsync(cancellationToken);

						LogTrace("The repository was dropped");
					}
				} catch (NotSupportedException ex) {
					LogError(ex, "Not Supported Error while dropping the repository {RepositoryType} handling the type {EntityType}",
						repository.GetType().Name, repository.EntityType.Name);
					throw;
				} catch (RepositoryException ex) {
					LogError(ex, "Not Supported Error while dropping the repository {RepositoryType} handling the type {EntityType}",
						repository.GetType().Name, repository.EntityType.Name);
					throw;
				} catch (Exception ex) {
					LogError(ex, "Not Supported Error while dropping the repository {RepositoryType} handling the type {EntityType}",
						repository.GetType().Name, repository.EntityType.Name);

					throw new RepositoryException($"Unable to drop the repository '{repository.GetType().Name}'", ex);
				}
			}
		}

		public virtual async Task CreateAllRepositoriesAsync(CancellationToken cancellationToken = default) {
			var repositories = serviceProvider.GetServices<IRepository>()
				.Select(RequireControllable);

			foreach (var repository in repositories) {
				await CreateRepository(repository, cancellationToken);
			}
		}

		public virtual async Task CreateTenantRepositoriesAsync(string tenantId, CancellationToken cancellationToken = default) {
			var providers = serviceProvider.GetServices<IRepositoryProvider>();
			foreach (var provider in providers) {
				var repository = RequireControllable(provider.GetRepository(tenantId));

				await CreateRepository(repository, cancellationToken);
			}
		}

		public virtual async Task DropAllRepositoriesAsync(CancellationToken cancellationToken = default) {
			var repositories = serviceProvider.GetServices<IRepository>()
				.Select(RequireControllable);

			foreach (var repository in repositories) {
				await DropRepository(repository, cancellationToken);
			}
		}

		public virtual async Task DropTenantRepositoriesAsync(string tenantId, CancellationToken cancellationToken = default) {
			var providers = serviceProvider.GetServices<IRepositoryProvider>();
			foreach (var provider in providers) {
				var repository = RequireControllable(provider.GetRepository(tenantId));

				await DropRepository(repository, cancellationToken);
			}
		}

		public virtual async Task CreateRepositoryAsync<TEntity>(CancellationToken cancellationToken = default)
			where TEntity : class, IEntity {
			var repository = RequireRepository<TEntity>();

			await CreateRepository(repository, cancellationToken);
		}

		public async Task CreateTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity {
			var provider = serviceProvider.GetService<IRepositoryProvider<TEntity>>();
			if (provider == null)
				throw new NotSupportedException();

			var repository = RequireControllable(provider.GetRepository(tenantId));

			await CreateRepository(repository, cancellationToken);
		}

		public virtual async Task DropRepositoryAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class, IEntity {
			var repository = RequireRepository<TEntity>();

			await DropRepository(repository, cancellationToken);
		}

		public virtual async Task DropTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity {
			var providers = serviceProvider.GetServices<IRepositoryProvider>();
			foreach (var provider in providers) {
				var repository = RequireControllable(provider.GetRepository(tenantId));

				await DropRepository(repository, cancellationToken);
			}
		}
	}
}
