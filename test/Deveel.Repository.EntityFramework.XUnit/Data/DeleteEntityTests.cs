using System;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class DeleteEntityTests : EntityFrameworkRepositoryTestBase {
		private readonly IList<PersonEntity> people;

		public DeleteEntityTests(SqlTestConnection testCollection, ITestOutputHelper outputHelper) 
			: base(testCollection, outputHelper) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(EntityRepository<PersonEntity> repository) {
			await repository.CreateAsync(people);
		}

		private PersonEntity GetRandomPerson()
			=> people[Random.Shared.Next(0, people.Count)];

		[Fact]
		public async Task Entity_DeleteExisting() {
			var entity = GetRandomPerson();

			var result = await EntityRepository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = GetRandomPerson();

			var result = await Repository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = GetRandomPerson();

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Entity_DeleteNotExisting() {
			var entity = new PersonEntity { Id = Guid.NewGuid() };

			var result = await EntityRepository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new PersonEntity { Id = Guid.NewGuid() };

			var result = await Repository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new PersonEntity { Id = Guid.NewGuid() };

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Entity_DeleteById_Existing() {
			var id = GetRandomPerson().Id;

			var result = await EntityRepository.DeleteByIdAsync(id.ToString());

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = GetRandomPerson().Id;

			var result = await Repository.DeleteByIdAsync(id.ToString());

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = GetRandomPerson().Id;

			var result = await FacadeRepository.DeleteByIdAsync(id.ToString());

			Assert.True(result);
		}

		[Fact]
		public async Task Mongo_DeleteById_NotExisting() {
			var id = Guid.NewGuid();

			var result = await EntityRepository.DeleteByIdAsync(id.ToString());

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = Guid.NewGuid();

			var result = await Repository.DeleteByIdAsync(id.ToString());

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_NotExisting() {
			var id = Guid.NewGuid();

			var result = await EntityRepository.DeleteByIdAsync(id.ToString());

			Assert.False(result);
		}

	}
}
