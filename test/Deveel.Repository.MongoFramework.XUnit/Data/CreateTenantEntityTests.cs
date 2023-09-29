﻿using System;

using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

namespace Deveel.Data {
	public class CreateTenantEntityTests : MongoRepositoryProviderTestBase {
		private ISystemTime testTime = new TestTime();

		public CreateTenantEntityTests(MongoFrameworkTestFixture mongo) 
			: base(mongo) {
		}

		protected override void AddRepositoryProvider(IServiceCollection services) {
			services.AddSystemTime(testTime);

			base.AddRepositoryProvider(services);
		}

		[Fact]
		public async Task Mongo_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await MongoRepository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);

			var created = await FindPerson(ObjectId.Parse(id));

			Assert.NotNull(created);
			Assert.Equal(person.FirstName, created.FirstName);
			Assert.Equal(person.LastName, created.LastName);
			Assert.NotNull(person.BirthDate);
			Assert.NotNull(created.BirthDate);
			Assert.Equal(person.BirthDate.Value.Date, created.BirthDate.Value.Date);
			Assert.NotNull(created.CreatedAtUtc);
			Assert.Equal(testTime.UtcNow, created.CreatedAtUtc);
		}

		[Fact]
		public async Task Repository_CreateNewPerson() {
			var person = GeneratePerson();
			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);

			var created = await FindPerson(ObjectId.Parse(id));

			Assert.NotNull(created);
			Assert.Equal(person.FirstName, created.FirstName);
			Assert.Equal(person.LastName, created.LastName);
			Assert.NotNull(person.BirthDate);
			Assert.NotNull(created.BirthDate);
			Assert.Equal(person.BirthDate.Value.Date, created.BirthDate.Value.Date);
			Assert.NotNull(created.CreatedAtUtc);
			Assert.Equal(testTime.UtcNow, created.CreatedAtUtc);
		}

		[Fact]
		public async Task FacadeRepository_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await FacadeRepository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);

			var created = await FindPerson(ObjectId.Parse(id));

			Assert.NotNull(created);
			Assert.Equal(person.FirstName, created.FirstName);
			Assert.Equal(person.LastName, created.LastName);
			Assert.NotNull(person.BirthDate);
			Assert.NotNull(created.BirthDate);
			Assert.Equal(person.BirthDate.Value.Date, created.BirthDate.Value.Date);
			Assert.NotNull(created.CreatedAtUtc);
			Assert.Equal(testTime.UtcNow, created.CreatedAtUtc);
		}


		[Fact]
		public async Task MongoProvider_CreateNewPerson() {
			var person = GeneratePerson();
			var id = await MongoRepositoryProvider.CreateAsync(TenantId, person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);

			// TODO: validate it was created in the correct tenant
		}

		[Fact]
		public async Task RepositoryProvider_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await RepositoryProvider.CreateAsync(TenantId, person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task FacadeRepositoryProvider_CreateNewPerson() {
			var person = GeneratePerson();

			var id = await FacadeRepositoryProvider.CreateAsync(TenantId, person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}



		[Fact]
		public async Task Mongo_CreateNewPersons() {
			var persons = GeneratePersons(100);

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
			var persons = GeneratePersons(100);

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
			var persons = GeneratePersons(100);

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
