using Deveel.Data.Caching;

using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public class PersonManager : EntityManager<Person, string> {
		public PersonManager(
			IRepository<Person, string> repository, 
			IEntityValidator<Person, string>? validator = null, 
			IEntityCache<Person>? cache = null,
			ISystemTime? systemTime = null,
			IOperationErrorFactory<Person>? errorFactory = null,
			IServiceProvider? services = null, 
			ILoggerFactory? loggerFactory = null) : base(repository, validator, cache, systemTime, errorFactory, services, loggerFactory) {
		}

		public async Task<Person?> FindByEmailAsync(string email, CancellationToken? cancellationToken = null) {
			var token = GetCancellationToken(cancellationToken);

			return await GetOrSetAsync($"person:{email}", () => FindFirstAsync(x => x.Email == email, token), token);
		}
	}
}
