using System;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class QueryEntitiesTests : MongoFrameworkRepositoryTestBase {
		private readonly IList<MongoPerson> people;

		public QueryEntitiesTests(MongoSingleDatabase mongo) : base(mongo) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(MongoRepository<MongoDbContext, MongoPerson> repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Mongo_CountAll() {
			var result = await MongoRepository.CountAllAsync();

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
		public async Task FacadeRepository_CountAll() {
			var result = await FilterableFacadeRepository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(people.Count, result);
		}


		[Fact]
		public async Task Mongo_CountFiltered() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await MongoRepository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task Repository_CountFiltered() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await FilterableRepository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task FacadeRepository_CountFiltered() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var count = await FilterableFacadeRepository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);

		}

		[Fact]
		public async Task Mongo_FindById() {
			var id = people.Random()!.Id;

			var result = await MongoRepository.FindByIdAsync(id.ToEntityId());

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}


		[Fact]
		public async Task Repository_FindById() {
			var id = people.Random()!.Id;

			var result = await Repository.FindByIdAsync(id.ToEntityId());

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FacadeRepository_FindById() {
			var id = people.Random()!.Id;

			var result = await FacadeRepository.FindByIdAsync(id.ToEntityId());

			Assert.NotNull(result);
			Assert.Equal(id.ToEntityId(), result.Id);
		}



		[Fact]
		public async Task Mongo_FindFirstFiltered() {
			var firstName = people.Random()!.FirstName;

			var result = await MongoRepository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_FindFirstFiltered() {
			var firstName = people.Random()!.FirstName;

			var result = await Repository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task FacadeRepository_FindFirstFiltered() {
			var firstName = people.Random()!.FirstName;

			var result = await FacadeRepository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}


		[Fact]
		public async Task Mongo_ExistsFiltered() {
			var firstName = people.Random()!.FirstName;

			var result = await MongoRepository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_ExistsFiltered() {
			var firstName = people.Random()!.FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_ExistsFiltered() {
			var firstName = people.Random()!.FirstName;

			var result = await FacadeRepository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}


		[Fact]
		public async Task Mongo_FindFirst() {
			var result = await MongoRepository.FindAsync();

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
		public async Task Mongo_FindAll() {
			var result = await MongoRepository.FindAllAsync();

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
		public async Task Mongo_FindAllFiltered() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await MongoRepository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

        [Fact]
        public async Task Mongo_FindAllMongoQueryFiltered() {
            var firstName = people.Random()!.FirstName;
            var peopleCount = people.Count(x => x.FirstName == firstName);

            var result = await MongoRepository.FindAllAsync(new MongoQueryFilter<MongoPerson>(Builders<MongoPerson>.Filter.Where(x => x.FirstName == firstName)));

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(peopleCount, result.Count);
        }

        [Fact]
		public async Task Repository_FindAllFiltered() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task FacadeRepository_FindAllFiltered() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);

			var result = await FacadeRepository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}


		[Fact]
		public async Task Mongo_GetPage() {
			var request = new RepositoryPageRequest<MongoPerson>(1, 10);

			var result = await MongoRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetPage() {
			var request = new RepositoryPageRequest<MongoPerson>(1, 10);

			var result = await PageableRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task FacadeRepository_GetPage() {
			var request = new RepositoryPageRequest<MongoPerson>(1, 10);

			var result = await FacadePageableRepository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}



		[Fact]
		public async Task Mongo_GetFilteredPage() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await MongoRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task Mongo_GetMultiFilteredPage() {
			var person = people.Random(x => x.LastName != null)!;
			var firstName = person.FirstName;
			var lastName = person.LastName;

			var peopleCount = people.Count(x => x.FirstName == firstName && x.LastName == lastName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.Where(x => x.FirstName == firstName)
				.Where(x => x.LastName == lastName);

			var result = await MongoRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetFilteredPage() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await PageableRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetMultiFilteredPage() {
			var person = people.Random(x => x.LastName != null)!;
			var firstName = person.FirstName;
			var lastName = person.LastName;

			var peopleCount = people.Count(x => x.FirstName == firstName && x.LastName == lastName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.Where(x => x.FirstName == firstName)
				.Where(x => x.LastName == lastName);

			var result = await PageableRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task FacadeRepository_GetFilteredPage() {
			var firstName = people.Random()!.FirstName;
			var peopleCount = people.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<IPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await FacadePageableRepository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}


		[Fact]
		public async Task Mongo_GetDescendingSortedPage() {
			var sorted = people.Skip(0).Take(10).OrderByDescending(x => x.LastName).ToList();
			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.OrderByDescending(x => x.LastName);

			var result = await MongoRepository.GetPageAsync(request);
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
		public async Task Mongo_GetSortedPage() {
			var sorted = people.Skip(0).Take(10).OrderBy(x => x.LastName).ToList();
			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.OrderBy(x => x.LastName);

			var result = await MongoRepository.GetPageAsync(request);
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
		public async Task Repository_GetDescendingSortedPage() {
			var sorted = people.Skip(0).Take(10).OrderByDescending(x => x.LastName).ToList();
			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.OrderByDescending(x => x.LastName);

			var result = await PageableRepository.GetPageAsync(request);
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
	}
}
