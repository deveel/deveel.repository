using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
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
				services.AddLogging(logging => { logging.ClearProviders(); logging.AddXUnit(TestOutput); });
		}

		private void BuildServices() {
			var services = new ServiceCollection();
			services.AddSystemTime(TestTime);

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

		protected virtual Task<TPerson?> FindPersonAsync(object id) {
			var entity = People.FirstOrDefault(x => Repository.GetEntityKey(x) == id);
			return Task.FromResult(entity);
		}

		protected virtual Task<TPerson> RandomPersonAsync(Expression<Func<TPerson, bool>>? predicate = null) {
			var result = People.Random(predicate?.Compile());

			if (result == null)
				throw new InvalidOperationException("No person found");

			return Task.FromResult(result);
		}

		[Fact]
		public async Task AddNewPerson() {
			var person = GeneratePerson();

			await Repository.AddAsync(person);

			var id = Repository.GetEntityKey(person);

			Assert.NotNull(id);

			var found = await Repository.FindByKeyAsync(id);
			Assert.NotNull(found);
			Assert.Equal(person.FirstName, found.FirstName);
			Assert.Equal(person.LastName, found.LastName);
			Assert.Equal(person.Email, found.Email);
		}

		[Fact]
		public async Task AddNewPerson_Sync() {
			var person = GeneratePerson();

			Repository.Add(person);

			var id = Repository.GetEntityKey(person);

			Assert.NotNull(id);

			var found = await Repository.FindByKeyAsync(id);
			
			Assert.NotNull(found);
			Assert.Equal(person.FirstName, found.FirstName);
			Assert.Equal(person.LastName, found.LastName);
			Assert.Equal(person.Email, found.Email);
		}

		[Fact]
		public async Task AddNewPersons() {
			var entities = GeneratePeople(10);

			await Repository.AddRangeAsync(entities);

			foreach (var item in entities) {
				var key = Repository.GetEntityKey(item);
				Assert.NotNull(key);

				var found = await Repository.FindByKeyAsync(key);
				Assert.NotNull(found);
			}
		}

		[Fact]
		public async Task RemoveExisting() {
			var person = await RandomPersonAsync();

			Assert.NotNull(person);

			var result = await Repository.RemoveAsync(person);

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
		public async Task RemoveByKey_Existing() {
			var key = Repository.GetEntityKey(People.Random()!);

			Assert.NotNull(key);

			var result = await Repository.RemoveByKeyAsync(key);

			Assert.True(result);
		}

		[Fact]
		public void RemoveByKeySync_Existing() {
			var key = Repository.GetEntityKey(People.Random()!);

			Assert.NotNull(key);

			var result = Repository.RemoveByKey(key);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveByKey_NotExisting() {
			var id = GeneratePersonId();

			var result = await Repository.RemoveByKeyAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task RemoveRangeOfExisting() {
			var peopleCount = People.Count;
			var people = People.Take(10).ToList();

			await Repository.RemoveRangeAsync(people);

			var result = await Repository.FindAllAsync();
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount - 10, result.Count);
		}

		[Fact]
		public async Task RemoveRangeWithOneNotExisting() {
			var peopleCount = People.Count;
			var people = People.Take(9).ToList();

			var entity = GeneratePerson();
			entity.Id = GeneratePersonId();

			people.Add(entity);

			await Assert.ThrowsAsync<RepositoryException>(() => Repository.RemoveRangeAsync(people));

			var result = await Repository.FindAllAsync();
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
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
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task CountFiltered_Sync() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var count = Repository.Count(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task FindByKey() {
			var person = await RandomPersonAsync();
			var id = person.Id!;

			var result = await Repository.FindByKeyAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FindFirstFiltered() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = await Repository.FindFirstAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public void FindFirstSync() {
			var result = Repository.FindFirst();

			Assert.NotNull(result);
			Assert.NotNull(result.Id);
		}

		[Fact]
		public async Task ExistsFiltered() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task ExistsFiltered_Sync() {
			var person = await RandomPersonAsync();
			var firstName =person.FirstName;

			var result = Repository.Exists(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task FindById_Existing() {
			var person = await RandomPersonAsync();

			var result = await Repository.FindByKeyAsync(person.Id!);

			Assert.NotNull(result);
			Assert.Equal(person.Id, result.Id);

			Assert.Equal(person.FirstName, result.FirstName);
			Assert.Equal(person.LastName, result.LastName);
		}

		[Fact]
		public async Task FindById_NotFound() {
			var id = GeneratePersonId();

			var result = await Repository.FindByKeyAsync(id);

			Assert.Null(result);
		}

		[Fact]
		public async Task FindById_Sync() {
			var person = await RandomPersonAsync();

			var result = Repository.FindByKey(person.Id!);

			Assert.NotNull(result);
		}

		[Fact]
		public async Task FindFirst() {
			var ordered = NaturalOrder(People).ToList();

			var result = await Repository.FindFirstAsync();

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
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task FindAllFiltered_BadFilter() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = await Assert.ThrowsAsync<RepositoryException>(
				() => Repository.FindAllAsync(QueryFilter.Where<MailAddress>(m => m.Address == null)));
		}

		[Fact]
		public async Task GetSimplePage() {
			var totalItems = People.Count;
			var totalPages = (int)Math.Ceiling((double)totalItems / 10);

			var result = await Repository.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(totalItems, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public async Task GetSimplePage_WithParameters() {
			var totalItems = People.Count;
			var totalPages = (int)Math.Ceiling((double)totalItems / 10);

			var result = await Repository.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(totalItems, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public async Task GetFilteredPage() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
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
		public async Task GetPage_MultipleFilters() {
			var person = await RandomPersonAsync(x => x.LastName != null);
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
		public async Task GetPage_ChainedFilters() {
			var person = await RandomPersonAsync(x => x.DateOfBirth != null);
			var firstName = person.FirstName;
			var birthDate = person.DateOfBirth!.Value;

			var peopleCount = People
				.Where(x => x.FirstName == firstName)
				.Where(x => x.DateOfBirth >= birthDate)
				.Count();

			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new PageQuery<TPerson>(1, 10)
				.Where(x => x.FirstName == firstName)
				.Where(x => x.DateOfBirth >= birthDate);

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
		public async Task GetPersonId() {
			var person = await RandomPersonAsync();

			var id = Repository.GetEntityKey(person);

			Assert.NotNull(id);
			Assert.Equal(person.Id, id.ToString());
		}

		[Fact]
		public async Task UpdateExisting() {
			var person = await RandomPersonAsync(x => x.FirstName != "John");

			person.FirstName = "John";

			var result = await Repository.UpdateAsync(person);

			Assert.True(result);

			var updated = await Repository.FindByKeyAsync(person.Id!);

			Assert.NotNull(updated);
			Assert.Equal(person.FirstName, updated.FirstName);
			Assert.Equal(person.LastName, updated.LastName);
			Assert.Equal(person.Email, updated.Email);
			Assert.Equal(person.DateOfBirth, updated.DateOfBirth);
		}

		[Fact]
		public async Task UpdateExisting_Sync() {
			var person = await RandomPersonAsync(x => x.FirstName != "John");

			person.FirstName = "John";

			var result = Repository.Update(person);

			Assert.True(result);

			var updated = await Repository.FindByKeyAsync(person.Id!);

			Assert.NotNull(updated);
			Assert.Equal(person.FirstName, updated.FirstName);
			Assert.Equal(person.LastName, updated.LastName);
			Assert.Equal(person.Email, updated.Email);
			Assert.Equal(person.DateOfBirth, updated.DateOfBirth);
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
