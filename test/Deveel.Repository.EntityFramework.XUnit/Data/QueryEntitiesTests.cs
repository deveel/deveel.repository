using System;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(SqlConnectionCollection))]
	public class QueryEntitiesTests : EntityFrameworkRepositoryTestBase {
		private readonly IList<PersonEntity> people;

		public QueryEntitiesTests(SqlTestConnection testCollection, ITestOutputHelper outputHelper) 
			: base(testCollection, outputHelper) {
			people = GeneratePersons(100);
		}

		private PersonEntity GetRandomPerson()
			=> people[Random.Shared.Next(0, people.Count)];

		protected override async Task SeedAsync(EntityRepository<PersonEntity> repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Entity_CountAll() {
			var result = await EntityRepository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(people.Count, result);
		}

		[Fact]
		public async Task Repository_CountAll() {
			var result = await FilterableRepository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(people.Count, result);
		}

		[Fact]
		public async Task Entity_CountFiltered() {
			var firstName = GetRandomPerson().FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await EntityRepository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task Repository_CountFiltered() {
			var firstName = GetRandomPerson().FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await FilterableRepository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task Entity_FindById() {
			var id = GetRandomPerson().Id;

			var result = await EntityRepository.FindByIdAsync(id.ToString());

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}


		[Fact]
		public async Task Repository_FindById() {
			var id = GetRandomPerson().Id;

			var result = await Repository.FindByIdAsync(id.ToString());

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FacadeRepository_FindById() {
			var id = GetRandomPerson().Id;

			var result = await FacadeRepository.FindByIdAsync(id.ToString());

			Assert.NotNull(result);
			Assert.Equal(id.ToString(), result.Id);
		}



		[Fact]
		public async Task Entity_FindFirstFiltered() {
			var firstName = GetRandomPerson().FirstName;

			var result = await EntityRepository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_FindFirstFiltered() {
			var firstName = GetRandomPerson().FirstName;

			var result = await Repository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task Entity_ExistsFiltered() {
			var firstName = GetRandomPerson().FirstName;

			var result = await EntityRepository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_ExistsFiltered() {
			var firstName = GetRandomPerson().FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task Entity_FindFirst() {
			var expected = people.OrderBy(x => x.Id).First();

			var result = await EntityRepository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(expected.FirstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_FindFirst() {
			var expected = people.OrderBy(x => x.Id).First();

			var result = await Repository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(expected.FirstName, result.FirstName);
		}

		[Fact]
		public async Task Entity_FindAll() {
			var result = await EntityRepository.FindAllAsync();

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
		public async Task Entity_FindAllFiltered() {
			var firstName = GetRandomPerson().FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await EntityRepository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task Repository_FindAllFiltered() {
			var firstName = GetRandomPerson().FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task Entity_GetPage() {
			var request = new RepositoryPageRequest<PersonEntity>(1, 10);

			var result = await EntityRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetPage() {
			var request = new RepositoryPageRequest<PersonEntity>(1, 10);

			var result = await PageableRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task Entity_GetFilteredPage() {
			var firstName = people[people.Count - 1].FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<PersonEntity>(1, 10).Where(x => x.FirstName == firstName);

			var result = await EntityRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetFilteredPage() {
			var firstName = GetRandomPerson().FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<PersonEntity>(1, 10).Where(x => x.FirstName == firstName);
			
			var result = await PageableRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task Entity_GetSortedPage() {
			var request = new RepositoryPageRequest<PersonEntity>(1, 10)
				.OrderByDescending(x => x.FirstName);

			var result = await EntityRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetSortedPage() {
			var request = new RepositoryPageRequest<PersonEntity>(1, 10)
				.OrderByDescending(x => x.FirstName);

			var result = await PageableRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}
	}
}
