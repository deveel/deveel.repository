using Xunit.Abstractions;

namespace Deveel.Data {
	public class CreateEntityTests : EntityFrameworkRepositoryTestBase {
		public CreateEntityTests(SqlTestConnection sqliteConnection, ITestOutputHelper outputHelper) 
			: base(sqliteConnection, outputHelper) {
		}

		[Fact]
		public async Task Eentity_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await EntityRepository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
			Assert.Equal(id, person.Id.ToString());
		}

		[Fact]
		public async Task Repository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
			Assert.Equal(id, person.Id.ToString());
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await FacadeRepository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
			Assert.Equal(id, person.Id.ToString());
		}

		[Fact]
		public async Task Entity_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await EntityRepository.CreateAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, Guid.Parse(results[i]));
			}
		}

		[Fact]
		public async Task Repository_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await Repository.CreateAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, Guid.Parse(results[i]));
			}
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await FacadeRepository.CreateAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, Guid.Parse(results[i]));
			}
		}

	}
}
