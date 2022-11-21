using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public class InMemoryRepositoryTestBase : IAsyncLifetime {
		private readonly IServiceProvider serviceProvider;

		public InMemoryRepositoryTestBase() {
			var services = new ServiceCollection();
			AddRepository(services);

			serviceProvider = services.BuildServiceProvider();
		}

		protected InMemoryRepository<Person> InMemoryRepository => serviceProvider.GetRequiredService<InMemoryRepository<Person>>();

		protected IRepository<Person> Repository => serviceProvider.GetRequiredService<IRepository<Person>>();

		protected IRepository<IPerson> FacadeRepository => serviceProvider.GetRequiredService<IRepository<IPerson>>();


		protected virtual void AddRepository(IServiceCollection services) {
			services
				.AddInMemoryRepository<Person>()
				.AddInMemoryFacadeRepository<Person, IPerson>()
				.AddRepositoryController();
		}

		public virtual async Task InitializeAsync() {
			await SeedAsync(InMemoryRepository);
			await SeedAsync(FacadeRepository);
			await SeedAsync(Repository);
		}

		public virtual Task DisposeAsync() {
			return Task.CompletedTask;
		}

		protected virtual Task SeedAsync(IRepository repository) {
			return Task.CompletedTask;
		}

		protected interface IPerson : IEntity {
			string FirstName { get; }

			string LastName { get; }

			DateTime? BirthDate { get; }
		}

		protected class Person : IPerson {
			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateTime? BirthDate { set; get; }

			public string Id { get; set; }
		}
	}
}
