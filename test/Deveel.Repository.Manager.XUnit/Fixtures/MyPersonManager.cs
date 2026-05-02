using Deveel.Data.Caching;

using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public class MyPersonManager : EntityManager<Person> {
		public MyPersonManager(IRepository<Person> repository, 
			IEntityValidator<Person>? validator = null, 
			IEntityCache<Person>? cache = null,
			ISystemTime? systemTime = null,
			IOperationErrorFactory<Person>? errorFactory = null,
			IServiceProvider? services = null, 
			ILoggerFactory? loggerFactory = null) 
			: base(repository, validator, cache, systemTime, errorFactory, services, loggerFactory) {
		}

		public async Task<Person?> FindByEmailAsync(string email, CancellationToken? cancellationToken = null) {
			return await FindFirstAsync(x => x.Email == email, cancellationToken);
		}
	}
}
