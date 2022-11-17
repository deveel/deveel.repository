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
	public class MongoDbFacadeTests : IDisposable {
		private MongoDbRunner dbRunner;
		private IRepository<ITest> facadeStore;
		private IRepository<TestDocument> baseStore;

		private IRepositoryProvider<ITest> storeProvider;

		public MongoDbFacadeTests() {
			dbRunner = MongoDbRunner.Start(logger: NullLogger.Instance);

			var services = new ServiceCollection()
				.AddMongoOptions(options => options
					.ConnectionString(dbRunner.ConnectionString)
					.Database("test_db")
					.Collection(nameof(TestDocument), "tests")
					.WithTenantDatabase())
				.AddMongoStoreOptions<TestDocument>(options => options
					.ConnectionString(dbRunner.ConnectionString)
					.Database("test_db")
					.Collection("tests"))
				.AddMongoDbFacadeStore<TestDocument, ITest>()
				.AddMongoDbFacadeStoreProvider<TestDocument, ITest>()
				.AddMongoDbStore<TestDocument>()
				.AddMongoDbStoreProvider<TestDocument>();


			var provider = services.BuildServiceProvider();

			baseStore = provider.GetRequiredService<IRepository<TestDocument>>();
			facadeStore = provider.GetRequiredService<IRepository<ITest>>();

			storeProvider = provider.GetRequiredService<IRepositoryProvider<ITest>>();
		}

		[Fact]
		public async Task CreateNewInstance() {
			var result = await facadeStore.CreateAsync(new TestDocument { Data = "foo-bar" });

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

			var result = await facadeStore.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal("foo-bar", result.Data);
		}

		[Fact]
		public async Task GetPage() {
			var id1 = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar" });
			var id2 = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar-2" });

			var result = await facadeStore.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(2, result.TotalItems);
			Assert.Equal(2, result.Items.Count());
			Assert.Equal(id1, result.Items.ElementAt(0).Id);
			Assert.Equal(id2, result.Items.ElementAt(1).Id);
		}

		[Fact]
		public async Task FindUsingIdObject() {
			var id = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar" });

			var result = await baseStore.FindAsync(x => x.Id == ObjectId.Parse(id));

			Assert.NotNull(result);
			Assert.Equal(id, result.Id.ToEntityId());
			Assert.Equal("foo-bar", result.Data);
		}

		[Fact]
		public async Task FilteredPage() {
			var id1 = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar" });
			var id2 = await baseStore.CreateAsync(new TestDocument { Data = "foo-bar-2" });

			var page = new PageRequest(1, 10) {
				Filter = Filter.Equal("Data", "foo-bar")
			};

			var result = await facadeStore.GetPageAsync(page);

			Assert.NotNull(result);
			Assert.Equal(1, result.TotalItems);
			Assert.Equal(1, result.TotalPages);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
		}

		[Fact]
		public async Task Update() {
			var doc = new TestDocument { Data = "foobar" };
			var id = await baseStore.CreateAsync(doc);

			doc.Data = "foo-bar";

			Assert.True(await facadeStore.UpdateAsync(doc));

			var modified = await facadeStore.FindByIdAsync(id);

			Assert.NotNull(modified);
			Assert.Equal("foo-bar", modified.Data);
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
