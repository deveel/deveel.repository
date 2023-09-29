using System;

namespace Deveel.Data {
	public class CreateEntityTests : InMemoryRepositoryTestBase {
		[Fact]
		public async Task Memory_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await InMemoryRepository.AddAsync(person);

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
		public async Task Memory_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await InMemoryRepository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, results[i]);
			}
		}

		[Fact]
		public async Task Repository_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, results[i]);
			}
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPersons() {
			var persons = GeneratePersons(100);

			var results = await FacadeRepository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, results[i]);
			}
		}

	}
}
