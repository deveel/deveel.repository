using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Mongo2Go;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using Xunit;

namespace Deveel.Repository {
	public class MongoDbFieldMultiTenancyTests : IDisposable {
		private readonly MongoDbRunner _server;
		private readonly IMongoClient _client;
		private readonly IMongoCollection<TestDocument> _collection;

		private MongoDbOptions options;
		private MongoDbStoreOptions<TestDocument> storeOptions;
		private MongoRepository<TestDocument> _store;
		private MongoRepositoryProvider<TestDocument> _provider;

		public MongoDbFieldMultiTenancyTests() {
			_server = MongoDbRunner.Start(logger: NullLogger.Instance);

			options = new MongoDbOptions {
				ConnectionString = _server.ConnectionString,
				Collections = new Dictionary<string, MongoDbCollectionOptions> {
					{ nameof(TestDocument), new MongoDbCollectionOptions{ Name = "tests" } }
				},
				DatabaseName = "testdb",
				MultiTenancy = new MongoDbMultiTenancyOptions {
					Handling = MultiTenancyHandling.TenantField
				}
			};

			storeOptions = new MongoDbStoreOptions<TestDocument> {
				ConnectionString= _server.ConnectionString,
				CollectionName = "tests",
				DatabaseName = "testdb"
			};

			_client = new MongoClient(_server.ConnectionString);
			_collection = _client.GetDatabase(options.DatabaseName).GetCollection<TestDocument>(options.Collections[nameof(TestDocument)].Name);

			_store = new MongoRepository<TestDocument>(Options.Create(storeOptions));
			_provider = new MongoRepositoryProvider<TestDocument>(Options.Create(options));
		}

		[Fact]
		public async Task CreateEntityInStore() {
			var tenantId = Guid.NewGuid().ToString("N");

			var entity = new TestDocument {
				TenantId = tenantId,
				CreatedAt = DateTimeOffset.UtcNow,
			};

			await _store.CreateAsync(entity);

			Assert.NotNull(entity.TenantId);
			Assert.Equal(tenantId, entity.TenantId);
		}

		[Fact]
		public async Task CreateEntityInProvider() {
			var tenantId = Guid.NewGuid().ToString("N");

			var entity = new TestDocument {
				CreatedAt = DateTimeOffset.UtcNow,
			};

			await _provider.CreateAsync(tenantId, entity);

			Assert.NotNull(entity.TenantId);
			Assert.Equal(tenantId, entity.TenantId);
		}

		[Fact]
		public async void CreateAndGetPageFromStore() {

			var tenantId = Guid.NewGuid().ToString();

			var docs = new List<TestDocument>();
			for (int i = 0; i < 100; i++) {			
				docs.Add(new TestDocument { Value = $"test_{i}", TenantId = tenantId });
			}

			await _collection.InsertManyAsync(docs);

			var page = await _store.GetPageAsync(new PageRequest<TestDocument>(1, 10), default);

			Assert.NotNull(page);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(100, page.TotalItems);
			Assert.Equal(10, page.TotalPages);
			Assert.NotEmpty(page.Items);
			Assert.Equal(10, page.Items.Count());

			foreach (var item in page.Items) {
				Assert.Equal(tenantId,  item.TenantId);
			}
		}

		[Fact]
		public async Task GetForTenantMixed() {
			const string tenantA = "tenantA";
			const string tenantB = "tenantB";

			var docs = new List<TestDocument> {
				new TestDocument { Value = "testValue", TenantId = tenantA },
				new TestDocument { Value = "testValue", TenantId = tenantB }
			};

			await _collection.InsertManyAsync(docs);

			var tenantDocs = await _provider.FindAllAsync(tenantA);

			Assert.Single(tenantDocs);
			Assert.Equal(tenantA, tenantDocs[0].TenantId);
		}


		public void Dispose() {
			_server?.Dispose();
		}

		#region TestDocument

		public class TestDocument : IMongoDocument, IEntity {
			/// <inheritdoc />
			string IMongoDocument.Id => Id.ToEntityId();

			string IEntity.Id => Id.ToEntityId();

			/// <inheritdoc />
			public DateTimeOffset CreatedAt { get; set; }

			/// <inheritdoc />
			[BsonId]
			public ObjectId Id { get; set; }

			public string Value { get; set; }

			public string TenantId { get; set; }
		}

		#endregion

	}
}
