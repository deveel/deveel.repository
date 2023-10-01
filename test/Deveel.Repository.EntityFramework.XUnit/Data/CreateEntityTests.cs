using Xunit.Abstractions;

namespace Deveel.Data {
	public class CreateEntityTests : EntityFrameworkRepositoryTestBase {
		public CreateEntityTests(SqlTestConnection sqliteConnection, ITestOutputHelper outputHelper) 
			: base(sqliteConnection, outputHelper) {
		}

		[Fact]
		public async Task Repository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
			Assert.Equal(id, person.Id.ToString());
		}

		[Fact]
		public async Task Repository_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, Guid.Parse(results[i]));
			}
		}
	}
}
