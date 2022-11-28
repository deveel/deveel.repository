using System;

using MongoDB.Bson;

namespace Deveel.Data {
	public class CreateEntityTests : MongoRepositoryTestBase {
		public CreateEntityTests(MongoDbTestFixture mongo) : base(mongo) {
		}

		[Fact]
		public async Task Mongo_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await MongoRepository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task Repository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await FacadeRepository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}


		[Fact]
		public async Task Mongo_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await MongoRepository.CreateAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
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
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
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
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
			}
		}

	}
}
