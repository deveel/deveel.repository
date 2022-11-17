using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Mongo2Go;
using MongoDB.Bson;

using Xunit;

namespace Deveel.Repository {
	public class MongoDbImplementingStoreTests : IDisposable {
		private MongoDbRunner dbRunner;
		private IRepository<ITest> store;
		private MongoRepository<TestDocument, ITest> baseStore;

		private IRepositoryProvider<ITest> storeProvider;

		public MongoDbImplementingStoreTests() {
			dbRunner = MongoDbRunner.Start(logger: NullLogger.Instance);

			var services = new ServiceCollection();
			services.AddMongoOptions(options => {
				options.DatabaseName = "test_db";
				options.Collections = new Dictionary<string, MongoDbCollectionOptions> {
					{ nameof(TestDocument), new MongoDbCollectionOptions{ Name = "tests" } }
				};
				options.ConnectionString = dbRunner.ConnectionString;
				options.MultiTenancy.Handling = MultiTenancyHandling.TenantDatabase;
				options.MultiTenancy.DatabaseNameFormat = "{database}_{tenant}";
			})
				.AddMongoStoreOptions<TestDocument>(options => {
					options.DatabaseName = "test_db";
					options.ConnectionString = dbRunner.ConnectionString;
					options.CollectionName = "tests";
				});

			services.AddMongoDbStore<TestDocument>()
				.AddMongoDbStoreProvider<TestDocument>()
				.AddMongoDbFacadeStore<TestDocument, ITest>()
				.AddMongoDbFacadeStoreProvider<TestDocument, ITest>();

			var provider = services.BuildServiceProvider();

			baseStore = provider.GetRequiredService<MongoRepository<TestDocument, ITest>>();
			store = provider.GetRequiredService<IRepository<ITest>>();

			storeProvider = provider.GetRequiredService<IRepositoryProvider<ITest>>();
		}

		[Fact]
		public async Task CreateNewInstance() {
			var result = await store.CreateAsync(new TestDocument { Data = "foo-bar" });

			Assert.NotNull(result);
			Assert.NotEmpty(result);
		}

		[Fact]
		public async Task CreateNewInstanceOnTenant() {
			var tenantId = Guid.NewGuid().ToString("N");

			var result = await storeProvider.CreateAsync(tenantId, new TestDocument { Data = "foo-bar" });

			Assert.NotNull(result);
			Assert.NotEmpty(result);
		}

		[Fact]
		public async Task GetExisting() {
			var id = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar" });

			var result = await store.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal("foo-bar", result.Data);
		}

		[Fact]
		public async Task GetPage() {
			var id1 = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar" });
			var id2 = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar-2" });

			var result = await store.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(2, result.TotalItems);
			Assert.Equal(2, result.Items.Count());
			Assert.Equal(id1, result.Items.ElementAt(0).Id);
			Assert.Equal(id2, result.Items.ElementAt(1).Id);
		}

		public void Dispose() => dbRunner?.Dispose();

		#region TestDocument

		class TestDocument : IMongoDocument, ITest {
			public ObjectId Id { get; set; }

			string IMongoDocument.Id => Id.ToEntityId();

			string IEntity.Id => Id.ToEntityId();

			public string Data { get; set; }
		}

		#endregion

		#region ITest

		interface ITest : IEntity {
			string Data { get; }
		}

		#endregion
	}
}
