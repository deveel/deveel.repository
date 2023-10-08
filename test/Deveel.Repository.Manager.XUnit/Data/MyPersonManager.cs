using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public class MyPersonManager : EntityManager<Person> {
		public MyPersonManager(IRepository<Person> repository, 
			IEntityValidator<Person>? validator = null, 
			IServiceProvider? services = null, 
			ILoggerFactory? loggerFactory = null) 
			: base(repository, validator, services, loggerFactory) {
		}
	}
}
