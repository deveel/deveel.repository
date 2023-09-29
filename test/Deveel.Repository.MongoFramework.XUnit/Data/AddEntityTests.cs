using Bogus;

using MongoDB.Bson;

namespace Deveel.Data {
	public class AddEntityTests : MongoFrameworkRepositoryTestBase {

		public AddEntityTests(MongoSingleDatabase mongo) : base(mongo) {
		}

		[Fact]
		public async Task Mongo_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await MongoRepository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task Repository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await FacadeRepository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}


		[Fact]
		public async Task Mongo_CreateNewPersons() {
			var persons = Enumerable.Range(0, 100)
				.Select(x => GeneratePerson())
				.ToList();

			var results = await MongoRepository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
			}
		}

		[Fact]
		public async Task Repository_CreateNewPersons() {
			var persons = Enumerable.Range(0, 100)
				.Select(x => GeneratePerson())
				.ToList();

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
			}
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPersons() {
			var persons = Enumerable.Range(0, 100)
				.Select(x => GeneratePerson())
				.ToList();

			var results = await FacadeRepository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
			}
		}

	}
}
