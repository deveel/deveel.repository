using System;

namespace Deveel.Data {
	public class RemoveEntityTests : InMemoryRepositoryTestBase {
		private readonly IList<Person> people;

		public RemoveEntityTests() {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(IRepository repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Memory_DeleteExisting() {
			var entity = people.Random();

			Assert.NotNull(entity);

			var result = await InMemoryRepository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = people.Random();

			Assert.NotNull(entity);

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = people.Random();

			Assert.NotNull(entity);

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.True(result);
		}


		[Fact]
		public async Task Memory_DeleteNotExisting() {
			var entity = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var result = await InMemoryRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString()).Generate();

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Memory_DeleteById_Existing() {
			var id = people.Random()?.Id;

			Assert.NotNull(id);

			var result = await InMemoryRepository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = people.Random()?.Id;

			Assert.NotNull(id);

			var result = await Repository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = people.Random()?.Id;

			Assert.NotNull(id);

			var result = await FacadeRepository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task Memory_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await InMemoryRepository.RemoveByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await Repository.RemoveByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await FacadeRepository.RemoveByIdAsync(id);

			Assert.False(result);
		}
	}
}
