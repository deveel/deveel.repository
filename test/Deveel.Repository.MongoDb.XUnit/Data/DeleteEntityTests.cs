﻿using System;

using MongoDB.Bson;

namespace Deveel.Data {
	public sealed class DeleteEntityTests : MongoRepositoryTestBase {
		private readonly IList<MongoPerson> people;

		public DeleteEntityTests(MongoDbTestFixture mongo) : base(mongo) {
			var nameGen = new RandomNameGenerator();

			people = Enumerable.Range(1, 100)
				.Select(_ => nameGen.NewName())
				.Select(x => new MongoPerson { FirstName = x.Item1, LastName = x.Item2 })
				.ToList();
		}

		protected override async Task SeedAsync(MongoRepository<MongoPerson> repository) {
			await repository.CreateAsync(people);
		}

		[Fact]
		public async Task Mongo_DeleteExisting() {
			var entity = people[^1];

			var result = await MongoRepository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = people[^1];

			var result = await Repository.DeleteAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = people[^1];

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.True(result);
		}


		[Fact]
		public async Task Mongo_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId() };

			var result = await MongoRepository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId() };

			var result = await Repository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId() };

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Mongo_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await MongoRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await Repository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await FacadeRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task Mongo_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await MongoRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await Repository.DeleteByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await FacadeRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.False(result);
		}
	}
}