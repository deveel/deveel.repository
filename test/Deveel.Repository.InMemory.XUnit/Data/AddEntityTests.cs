using System;

namespace Deveel.Data {
	public class AddEntityTests : InMemoryRepositoryTestBase {
		[Fact]
		public async Task Memory_AddNewPerson() {
			var person = GeneratePerson();

			var id = await InMemoryRepository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task Repository_AddNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task Memory_AddNewPersons() {
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
		public async Task Repository_AddNewPersons() {
			var persons = GeneratePersons(100);

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, results[i]);
			}
		}
	}
}
