using System;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class DeleteEntityTests : EntityFrameworkRepositoryTestBase {
		private readonly IList<PersonEntity> people;

		public DeleteEntityTests(SqlTestConnection testCollection, ITestOutputHelper outputHelper) 
			: base(testCollection, outputHelper) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(IRepository<PersonEntity> repository) {
			await repository.AddRangeAsync(people);
		}

		private PersonEntity GetRandomPerson()
			=> people[Random.Shared.Next(0, people.Count)];

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = GetRandomPerson();

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new PersonEntity { Id = Guid.NewGuid() };

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = GetRandomPerson().Id;

			var result = await Repository.RemoveByIdAsync(id.ToString());

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = Guid.NewGuid();

			var result = await Repository.RemoveByIdAsync(id.ToString());

			Assert.False(result);
		}
	}
}
