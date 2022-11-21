using System;

namespace Deveel.Data {
	public class QueryEntitiesTests : InMemoryRepositoryTestBase {
		private readonly IList<Person> people;

		public QueryEntitiesTests() {
			var nameGen = new RandomNameGenerator();

			people = Enumerable.Range(1, 100)
				.Select(_ => nameGen.NewName())
				.Select(x => new Person { FirstName = x.Item1, LastName = x.Item2 })
				.ToList();
		}

		protected override async Task SeedAsync(IRepository repository) {
			await repository.CreateAsync(people);
		}

		[Fact]
		public async Task Memory_CountAll() {
			var result = await InMemoryRepository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(people.Count, result);
		}

		[Fact]
		public async Task Repository_CountAll() {
			var result = await Repository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(people.Count, result);
		}

		[Fact]
		public async Task FacadeRepository_CountAll() {
			var result = await FacadeRepository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(people.Count, result);
		}


		[Fact]
		public async Task Memory_CountFiltered() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await InMemoryRepository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task Repository_CountFiltered() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task Memory_FindById() {
			var id = people[people.Count - 1].Id;

			var result = await InMemoryRepository.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}


		[Fact]
		public async Task Repository_FindById() {
			var id = people[people.Count - 1].Id;

			var result = await Repository.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FacadeRepository_FindById() {
			var id = people[people.Count - 1].Id;

			var result = await FacadeRepository.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}



		[Fact]
		public async Task Memory_FindFirstFiltered() {
			var firstName = people[people.Count - 1].FirstName;

			var result = await InMemoryRepository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_FindFirstFiltered() {
			var firstName = people[people.Count - 1].FirstName;

			var result = await Repository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task Memory_ExistsFiltered() {
			var firstName = people[people.Count - 1].FirstName;

			var result = await InMemoryRepository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_ExistsFiltered() {
			var firstName = people[people.Count - 1].FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}


		[Fact]
		public async Task Memory_FindFirst() {
			var result = await InMemoryRepository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(people[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_FindFirst() {
			var result = await Repository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(people[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task FacadeRepository_FindFirst() {
			var result = await FacadeRepository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(people[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task Memory_FindAll() {
			var result = await InMemoryRepository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(people.Count, result.Count);
		}

		[Fact]
		public async Task Repository_FindAll() {
			var result = await Repository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(people.Count, result.Count);
		}

		[Fact]
		public async Task FacadeRepository_FindAll() {
			var result = await FacadeRepository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(people.Count, result.Count);
		}



		[Fact]
		public async Task Memory_FindAllFiltered() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await InMemoryRepository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task Repository_FindAllFiltered() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}


		[Fact]
		public async Task Memory_GetPage() {
			var request = new RepositoryPageRequest<Person>(1, 10);

			var result = await InMemoryRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetPage() {
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
		public async Task FacadeRepository_GetPage() {
			var request = new RepositoryPageRequest<Person>(1, 10);

			var result = await FacadeRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}



		[Fact]
		public async Task Memory_GetFilteredPage() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<Person>(1, 10) {
				Filter = x => x.FirstName == firstName
			};

			var result = await InMemoryRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetFilteredPage() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<Person>(1, 10) {
				Filter = x => x.FirstName == firstName
			};

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}
	}
}
