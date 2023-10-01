using System.Net.Mail;

using Bogus;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public class InMemoryRepositoryTests : IAsyncLifetime {
		private readonly AsyncServiceScope scope;

		public InMemoryRepositoryTests() {
			People = GeneratePersons(100);

			var services = new ServiceCollection();
			AddRepository(services);

			scope = services.BuildServiceProvider().CreateAsyncScope();
		}

		protected IList<Person> People { get; }

		protected IRepository<Person> Repository => scope.ServiceProvider.GetRequiredService<IRepository<Person>>();

		protected Faker<Person> PersonFaker { get; } = new PersonFaker();

		protected Person GeneratePerson() => PersonFaker.Generate();

		protected IList<Person> GeneratePersons(int count)
			=> PersonFaker.Generate(count);

		protected virtual void AddRepository(IServiceCollection services) {
			services.AddInMemoryRepository<Person>();
			services.AddRepositoryController();
		}

		public virtual async Task InitializeAsync() {
			await SeedAsync(Repository);
		}

		public virtual async Task DisposeAsync() {
			await scope.DisposeAsync();
		}

		protected virtual async Task SeedAsync(IRepository<Person> repository) {
			await repository.AddRangeAsync(People);
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
			var persons = GeneratePersons(10);

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, results[i]);
			}
		}

		[Fact]
		public async Task DeleteExisting() {
			var entity = People.Random();

			Assert.NotNull(entity);

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task DeleteNotExisting() {
			var entity = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task DeleteById_Existing() {
			var id = People.Random()?.Id;

			Assert.NotNull(id);

			var result = await Repository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

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
		public async Task CountFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task FindById() {
			var id = People.Random()!.Id;

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
		public async Task FindById_Existing() {
			var person = People.Random()!;

			var result = await Repository.FindByIdAsync(person.Id);

			Assert.NotNull(result);
			Assert.Equal(person.Id, result.Id);

			Assert.Equal(person.FirstName, result.FirstName);
			Assert.Equal(person.LastName, result.LastName);
		}

		[Fact]
		public async Task FindById_NotFound() {
			var id = Guid.NewGuid().ToString();

			var result = await Repository.FindByIdAsync(id);

			Assert.Null(result);
		}

		[Fact]
		public async Task FindFirst() {
			var result = await Repository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(People[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task FindAll() {
			var result = await Repository.FindAllAsync();

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
			var request = new RepositoryPageRequest<Person>(1, 10);

			var result = await Repository.GetPageAsync(request);

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

			var request = new RepositoryPageRequest<Person>(1, 10)
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
		public async Task GetSortedPage() {
			var request = new RepositoryPageRequest<Person>(1, 10)
				.OrderBy(x => x.FirstName);

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
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
			var person = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var result = await Repository.UpdateAsync(person);

			Assert.False(result);
		}
	}
}
