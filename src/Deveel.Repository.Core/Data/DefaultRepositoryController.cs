using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
	/// <summary>
	/// A default implementation of the <see cref="IRepositoryController"/> interface
	/// </summary>
    public class DefaultRepositoryController : IRepositoryController {
		private readonly RepositoryControllerOptions options;
		private readonly IServiceProvider serviceProvider;
		private readonly ILogger logger;

		/// <summary>
		/// Constructs a <see cref="DefaultRepositoryController"/> instance
		/// </summary>
		/// <param name="options">
		/// The options to configure the controller
		/// </param>
		/// <param name="serviceProvider">
		/// The service provider used to resolve the repositories
		/// </param>
		/// <param name="logger">
		/// A logger used to trace the operations
		/// </param>
		public DefaultRepositoryController(IOptions<RepositoryControllerOptions> options, IServiceProvider serviceProvider, ILogger<DefaultRepositoryController>? logger = null)
			: this(options, serviceProvider, (ILogger?) logger) {
		}

		/// <summary>
		/// Constructs a <see cref="DefaultRepositoryController"/> instance
		/// </summary>
		/// <param name="options">
		/// The options to configure the controller
		/// </param>
		/// <param name="serviceProvider">
		/// The service provider used to resolve the repositories
		/// </param>
		/// <param name="logger">
		/// A logger used to trace the operations
		/// </param>
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

		private async Task<IControllableRepository?> GetTenantRepositoryAsync<TEntity>(IRepositoryProvider<TEntity> provider, string tenantId)
			where TEntity : class {
			try {
				LogTrace("Obtaining a new repository instance for tenant '{TenantId}'", tenantId);

				var repository = await provider.GetRepositoryAsync(tenantId);

				LogTrace("A new repository of type {RepositoryType} was obtained for tenant '{TenantId}'",
					repository.GetType().Name, tenantId);

				return repository as IControllableRepository;
			} catch(RepositoryException ex) {
				LogError(ex, "Error while trying to obtain a repository for tenant '{TenantId}'", tenantId);
				throw;
			} catch (Exception ex) {
				LogError(ex, "Error while trying to obtain a repository for tenant '{TenantId}'", tenantId);
				throw new RepositoryException("Could not obtain a repository", ex);
			}
		}

		private IControllableRepository? RequireRepository<TEntity>()
			where TEntity : class {

			LogTrace("Resolving a repository for entity of type {EntityType}", typeof(TEntity).Name);

			var repository = serviceProvider.GetService<IRepository<TEntity>>();

			//if (repository == null) {
			//	LogTrace("No generic repository handling the entity '{EntityType}' was found: trying the base forms", typeof(TEntity).Name);

			//	// less optimal lookup
			//	repository = serviceProvider.GetServices<IRepository>()
			//		.FirstOrDefault(x => x.EntityType == typeof(TEntity));
			//}
			
			if (repository == null)
				throw new NotSupportedException($"Unable to resolve any repository for entities of type {typeof(TEntity)}");

			LogTrace("The repository of type '{RepositoryType}' was resolved handling type '{EntityType}'", 
				repository.GetType().Name, typeof(TEntity).Name);

			return repository as IControllableRepository;
		}

		//private IControllableRepository? RequireControllable(IRepository repository) {
		//	if (!(repository is IControllableRepository controllable)) {
		//		if (!options.IgnoreNotControllable)
		//			throw new NotSupportedException($"The repository of type '{repository.GetType()}' is not controllable");

		//		LogTrace("The repository {RepositoryType} is not controllable and the service is ignoring it", repository.GetType().Name);

		//		return null;
		//	}

		//	return controllable;
		//}

		private IRepositoryProvider<TEntity> RequireRepositoryProvider<TEntity>()
			where TEntity : class {
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
					LogError(ex, "Not Supported Error while creating the repository {RepositoryType}",
						repository.GetType().FullName);
					throw;
				} catch (RepositoryException ex) {
					LogError(ex, "Not Supported Error while creating the repository {RepositoryType}",
						repository.GetType().FullName);
					throw;
				} catch(Exception ex) {
					LogError(ex, "Not Supported Error while creating the repository {RepositoryType}",
						repository.GetType().FullName);

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
					LogError(ex, "Not Supported Error while dropping the repository {RepositoryType}",
						repository.GetType().Name);
					throw;
				} catch (RepositoryException ex) {
					LogError(ex, "Not Supported Error while dropping the repository {RepositoryType}",
						repository.GetType().Name);
					throw;
				} catch (Exception ex) {
					LogError(ex, "Not Supported Error while dropping the repository {RepositoryType}",
						repository.GetType().Name);

					throw new RepositoryException($"Unable to drop the repository '{repository.GetType().Name}'", ex);
				}
			}
		}

		/// <inheritdoc/>
		public virtual async Task CreateRepositoryAsync<TEntity>(CancellationToken cancellationToken = default)
			where TEntity : class {
			LogTrace("Creating the repository for type '{EntityType}'", typeof(TEntity).Name);

			var repository = RequireRepository<TEntity>();

			await CreateRepository(repository, cancellationToken);

			LogTrace("The creation process for repository for type '{EntityType}' finished", typeof(TEntity).Name);
		}

		/// <inheritdoc/>
		public async Task CreateTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class {
			LogTrace("Creating the repository handling the type '{EntityType}' for tenant '{TenantId}'", typeof(TEntity).Name, tenantId);

			var provider = RequireRepositoryProvider<TEntity>();

			var repository = await provider.GetRepositoryAsync(tenantId) as IControllableRepository;

			await CreateRepository(repository, cancellationToken);

			LogTrace("The repository handling the type '{EntityType}' for tenant '{TenantId}' was created", typeof(TEntity).Name, tenantId);
		}

		/// <inheritdoc/>
		public virtual async Task DropRepositoryAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class {
			LogTrace("Dropping the repository handling the type '{EntityType}'", typeof(TEntity).Name);

			var repository = RequireRepository<TEntity>();

			await DropRepository(repository, cancellationToken);

			LogTrace("The repository handling the type '{EntityType}' was dropped", typeof(TEntity).Name);
		}

		/// <inheritdoc/>
		public virtual async Task DropTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class {
			LogTrace("Dropping the repository handling the type '{EntityType}' for tenant '{TenantId}'", typeof(TEntity).Name, tenantId);

			var provider = RequireRepositoryProvider<TEntity>();

			var repository = await GetTenantRepositoryAsync(provider, tenantId);

			await DropRepository(repository, cancellationToken);

			LogTrace("The repository handling the type '{EntityType}' for tenant '{TenantId}' was dropped", typeof(TEntity).Name, tenantId);
		}
	}
}
