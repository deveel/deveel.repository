using Bogus;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public class InMemoryRepositoryTestBase : IAsyncLifetime {
		private readonly IServiceProvider serviceProvider;

		public InMemoryRepositoryTestBase() {
			var services = new ServiceCollection();
			AddRepository(services);

			serviceProvider = services.BuildServiceProvider();

			PersonFaker = new PersonFaker();
		}

		protected InMemoryRepository<Person> InMemoryRepository => serviceProvider.GetRequiredService<InMemoryRepository<Person>>();

		protected IRepository<Person> Repository => serviceProvider.GetRequiredService<IRepository<Person>>();

		protected IPageableRepository<Person>? PageableRepository => Repository as IPageableRepository<Person>;

		protected IFilterableRepository<Person>? FilterableRepository => Repository as IFilterableRepository<Person>;

        protected Faker<Person> PersonFaker { get; }

		protected Person GeneratePerson() => PersonFaker.Generate();

		protected IList<Person> GeneratePersons(int count)
			=> PersonFaker.Generate(count);

		protected virtual void AddRepository(IServiceCollection services) {
			services.AddInMemoryRepository<Person>();
			services.AddRepositoryController();
		}

		public virtual async Task InitializeAsync() {
			await SeedAsync(InMemoryRepository);
			await SeedAsync(Repository);
		}

		public virtual Task DisposeAsync() {
			return Task.CompletedTask;
		}

		protected virtual Task SeedAsync(IRepository<Person> repository) {
			return Task.CompletedTask;
		}
	}
}
