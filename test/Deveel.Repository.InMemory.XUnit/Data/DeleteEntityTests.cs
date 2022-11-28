using System;

namespace Deveel.Data {
	public class DeleteEntityTests : InMemoryRepositoryTestBase {
		private readonly IList<Person> people;

		public DeleteEntityTests() {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(IRepository repository) {
			await repository.CreateAsync(people);
		}

		[Fact]
		public async Task Memory_DeleteExisting() {
			var entity = people[^1];

			var result = await InMemoryRepository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = people[^1];

			var result = await Repository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = people[^1];

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.True(result);
		}


		[Fact]
		public async Task Memory_DeleteNotExisting() {
			var entity = new Person { Id = Guid.NewGuid().ToString() };

			var result = await InMemoryRepository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new Person { Id = Guid.NewGuid().ToString() };

			var result = await Repository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new Person { Id = Guid.NewGuid().ToString() };

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Memory_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await InMemoryRepository.DeleteByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await Repository.DeleteByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await FacadeRepository.DeleteByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task Memory_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await InMemoryRepository.DeleteByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await Repository.DeleteByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await FacadeRepository.DeleteByIdAsync(id);

			Assert.False(result);
		}
	}
}
