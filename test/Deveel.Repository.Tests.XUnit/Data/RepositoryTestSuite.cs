using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net.Mail;

using Bogus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Deveel.Data {
	public abstract class RepositoryTestSuite<TPerson> : IAsyncLifetime where TPerson : class, IPerson {
		private IServiceProvider? services;
		private AsyncServiceScope scope;

		protected RepositoryTestSuite(ITestOutputHelper? testOutput) {
			TestOutput = testOutput;
		}

		protected ITestOutputHelper? TestOutput { get; }

		protected virtual int EntitySetCount => 100;

		protected IReadOnlyList<TPerson> People { get; private set; }

		protected IServiceProvider Services => scope.ServiceProvider;

		protected virtual IRepository<TPerson> Repository => Services.GetRequiredService<IRepository<TPerson>>();

		protected abstract Faker<TPerson> PersonFaker { get; }

		protected TPerson GeneratePerson() => PersonFaker.Generate();

		protected ISystemTime TestTime { get; } = new TestTime();

		protected IList<TPerson> GeneratePeople(int count) => PersonFaker.Generate(count);

		protected virtual string GeneratePersonId() => Guid.NewGuid().ToString();

		protected virtual void ConfigureServices(IServiceCollection services) {
			if (TestOutput != null)
				services.AddLogging(logging => logging.AddXUnit(TestOutput));
		}

		private void BuildServices() {
			var services = new ServiceCollection();
			services.AddSingleton<ISystemTime>(TestTime);

			ConfigureServices(services);

			this.services = services.BuildServiceProvider();
			scope = this.services.CreateAsyncScope();
		}

		async Task IAsyncLifetime.InitializeAsync() {
			BuildServices();

			People = GeneratePeople(EntitySetCount).ToImmutableList();

			await InitializeAsync();
		}

		protected virtual async Task InitializeAsync() {
			await SeedAsync(Repository);
		}

		async Task IAsyncLifetime.DisposeAsync() {
			await DisposeAsync();

			People = null;

			await scope.DisposeAsync();
			(services as IDisposable)?.Dispose();
		}

		protected virtual Task DisposeAsync() {
			return Task.CompletedTask;
		}

		protected virtual async Task SeedAsync(IRepository<TPerson> repository) {
			await repository.AddRangeAsync(People);
		}

		protected virtual IEnumerable<TPerson> NaturalOrder(IEnumerable<TPerson> source) {
			return source;
		}

		[Fact]
		public async Task AddNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task AddNewPersons() {
			var entities = GeneratePeople(10);

			var results = await Repository.AddRangeAsync(entities);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(entities.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(Repository.GetEntityId(entities[i]), results[i]);
			}
		}

		[Fact]
		public async Task RemoveExisting() {
			var entity = People.Random();

			Assert.NotNull(entity);

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveNotExisting() {
			var entity = GeneratePerson();

			entity.Id = GeneratePersonId();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task RemoveById_Existing() {
			var id = Repository.GetEntityId(People.Random()!);

			Assert.NotNull(id);

			var result = await Repository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveById_NotExisting() {
			var id = GeneratePersonId();

			var result = await Repository.RemoveByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task CountAll() {
			var result = await Repository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(People.Count, result);
		}

		[Fact]
		public void CountAll_Sync() {
			var result = Repository.CountAll();

			Assert.NotEqual(0, result);
			Assert.Equal(People.Count, result);
		}

		[Fact]
		public async Task CountFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task FindById() {
			var id = People.Random()!.Id!;

			var result = await Repository.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FindFirstFiltered() {
			var firstName = People.Random()!.FirstName;

			var result = await Repository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task ExistsFiltered() {
			var firstName = People.Random()!.FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public void ExistsFiltered_Sync() {
			var firstName = People.Random()!.FirstName;

			var result = Repository.Exists(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task FindById_Existing() {
			var person = People.Random()!;

			var result = await Repository.FindByIdAsync(person.Id!);

			Assert.NotNull(result);
			Assert.Equal(person.Id, result.Id);

			Assert.Equal(person.FirstName, result.FirstName);
			Assert.Equal(person.LastName, result.LastName);
		}

		[Fact]
		public async Task FindById_NotFound() {
			var id = GeneratePersonId();

			var result = await Repository.FindByIdAsync(id);

			Assert.Null(result);
		}

		[Fact]
		public void FindById_Sync() {
			var person = People.Random()!;

			var result = Repository.FindById(person.Id!);

			Assert.NotNull(result);
		}

		[Fact]
		public async Task FindFirst() {
			var ordered = NaturalOrder(People).ToList();

			var result = await Repository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(ordered[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task FindAll() {
			var result = await Repository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(People.Count, result.Count);
		}

		[Fact]
		public void FindAll_Sync() {
			var result = Repository.FindAll();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(People.Count, result.Count);
		}

		[Fact]
		public async Task FindAllFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task FindAllFiltered_BadFilter() {
			var firstName = People.Random()!.FirstName;

			var result = await Assert.ThrowsAsync<RepositoryException>(
				() => Repository.FindAllAsync(QueryFilter.Where<MailAddress>(m => m.Address == null)));
		}

		[Fact]
		public async Task GetSimplePage() {
			var request = new PageQuery<TPerson>(1, 10);

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task GetSimplePage_WithParameters() {
			var result = await Repository.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task GetFilteredPage() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new PageQuery<TPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task GetMultiFilteredPage() {
			var person = People.Random(x => x.LastName != null)!;
			var firstName = person.FirstName;
			var lastName = person.LastName;

			var peopleCount = People.Count(x => x.FirstName == firstName && x.LastName == lastName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new PageQuery<TPerson>(1, 10)
				.Where(x => x.FirstName == firstName && x.LastName == lastName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count);
		}


		[Fact]
		public async Task GetDescendingSortedPage() {
			var sorted = People.Where(x => x.LastName != null).OrderByDescending(x => x.LastName).Skip(0).Take(10).ToList();

			var request = new PageQuery<TPerson>(1, 10)
				.OrderByDescending(x => x.LastName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());

			for (int i = 0; i < sorted.Count; i++) {
				Assert.Equal(sorted[i].LastName, result.Items.ElementAt(i).LastName);
			}
		}

		[Fact]
		public async Task GetSortedPage() {
			var totalPages = (int)Math.Ceiling((double)People.Count / 10);

			var request = new PageQuery<TPerson>(1, 10)
				.OrderBy(x => x.FirstName);

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(People.Count, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public void GetPage_Sync() {
			var totalPages = (int)Math.Ceiling((double)People.Count / 10);

			var request = new PageQuery<TPerson>(1, 10);

			var result = Repository.GetPage(request);

			Assert.NotNull(result);	
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(People.Count, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public void GetPersonId() {
			var person = People.Random()!;

			var id = Repository.GetEntityId(person);

			Assert.NotNull(id);
			Assert.Equal(person.Id, id);
		}

		[Fact]
		public async Task UpdateExisting() {
			var person = People.Random()!;

			person.FirstName = "John";

			var result = await Repository.UpdateAsync(person);

			Assert.True(result);

			var updated = await Repository.FindByIdAsync(person.Id);

			Assert.NotNull(updated);
			Assert.Equal(person.FirstName, updated.FirstName);
		}

		[Fact]
		public async Task UpdateNotExisting() {
			var person = GeneratePerson();

			person.Id = GeneratePersonId();

			var result = await Repository.UpdateAsync(person);

			Assert.False(result);
		}
	}
}
