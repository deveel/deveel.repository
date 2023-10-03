using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public static class DependencyInjectionTests {
		[Fact]
		public static void AddDefaultInMemoryRepository() {
			var services = new ServiceCollection();
			services.AddRepository<InMemoryRepository<Person>>();

			var serviceProvider = services.BuildServiceProvider();

			Assert.NotNull(serviceProvider.GetService<InMemoryRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IPageableRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IFilterableRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IQueryableRepository<Person>>());
		}

		[Fact]
		public static void AddInMemoryRepository() {
			var services = new ServiceCollection();
			services.AddRepository<PersonRepository>();

			var serviceProvider = services.BuildServiceProvider();

			Assert.NotNull(serviceProvider.GetService<PersonRepository>());
			Assert.NotNull(serviceProvider.GetService<IRepository<Person>>());
			Assert.IsType<PersonRepository>(serviceProvider.GetService<IRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IPageableRepository<Person>>());
			Assert.IsType<PersonRepository>(serviceProvider.GetService<IPageableRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IFilterableRepository<Person>>());
			Assert.IsType<PersonRepository>(serviceProvider.GetService<IFilterableRepository<Person>>());
			Assert.NotNull(serviceProvider.GetService<IQueryableRepository<Person>>());
			Assert.IsType<PersonRepository>(serviceProvider.GetService<IQueryableRepository<Person>>());
		}

		[Fact]
		public static async Task AddInMemoryRepositoryProvider() {
			var services = new ServiceCollection();
			services.AddRepositoryProvider<PersonRepositoryProvider>();

			var serviceProvider = services.BuildServiceProvider();

			Assert.NotNull(serviceProvider.GetService<PersonRepositoryProvider>());
			Assert.NotNull(serviceProvider.GetService<IRepositoryProvider<Person>>());
			Assert.IsType<PersonRepositoryProvider>(serviceProvider.GetService<IRepositoryProvider<Person>>());

			var repository = await serviceProvider.GetRequiredService<IRepositoryProvider<Person>>().GetRepositoryAsync(Guid.NewGuid().ToString());

			Assert.NotNull(repository);
			Assert.IsType<PersonRepository>(repository);
		}
	}
}
