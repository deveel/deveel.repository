using System;

using MongoDB.Bson;

namespace Deveel.Data {
	public class CreateEntityTests : MongoRepositoryTestBase {
		public CreateEntityTests(MongoDbTestFixture mongo) : base(mongo) {
		}

		[Fact]
		public async Task Mongo_CreateNewPerson() {
			var randomNameGen = new RandomNameGenerator();

			var name = randomNameGen.NewName();

			var person = new MongoPerson {
				FirstName = name.Item1,
				LastName = name.Item2
			};

			var id = await MongoRepository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task Repository_CreateNewPerson() {
			var randomNameGen = new RandomNameGenerator();

			var name = randomNameGen.NewName();

			var person = new MongoPerson {
				FirstName = name.Item1,
				LastName = name.Item2
			};

			var id = await Repository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPerson() {
			var randomNameGen = new RandomNameGenerator();

			var name = randomNameGen.NewName();

			var person = new MongoPerson {
				FirstName = name.Item1,
				LastName = name.Item2
			};

			var id = await FacadeRepository.CreateAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}


		[Fact]
		public async Task Mongo_CreateNewPersons() {
			var randomNameGen = new RandomNameGenerator();
			var persons = Enumerable.Range(0, 100)
				.Select(_ => randomNameGen.NewName())
				.Select(x => new MongoPerson { FirstName = x.Item1, LastName = x.Item2 })
				.ToList();

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
			var randomNameGen = new RandomNameGenerator();
			var persons = Enumerable.Range(0, 100)
				.Select(_ => randomNameGen.NewName())
				.Select(x => new MongoPerson { FirstName = x.Item1, LastName = x.Item2 })
				.ToList();

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
			var randomNameGen = new RandomNameGenerator();
			var persons = Enumerable.Range(0, 100)
				.Select(_ => randomNameGen.NewName())
				.Select(x => new MongoPerson { FirstName = x.Item1, LastName = x.Item2 })
				.ToList();

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
