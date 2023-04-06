using Xunit.Abstractions;

namespace Deveel.Data {
	public class UpdateEntityTests : EntityFrameworkRepositoryTestBase {
		private readonly IList<PersonEntity> people;

		public UpdateEntityTests(SqlTestConnection testCollection, ITestOutputHelper outputHelper) 
			: base(testCollection, outputHelper) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(EntityRepository<PersonEntity> repository) {
			await repository.CreateAsync(people);
		}

		private PersonEntity GetRandomPerson() => people[Random.Shared.Next(0, people.Count)];

		[Fact]
		public async Task Entity_UpdateExisting() {
			var entity = GetRandomPerson();

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await EntityRepository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_UpdateExisting() {
			var entity = GetRandomPerson();

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateExisting() {
			var entity = GetRandomPerson();

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Entity_UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = Guid.NewGuid();
			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await EntityRepository.UpdateAsync(person);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = Guid.NewGuid();
			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(person);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = Guid.NewGuid();
			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(person);

			Assert.False(result);
		}

	}
}
