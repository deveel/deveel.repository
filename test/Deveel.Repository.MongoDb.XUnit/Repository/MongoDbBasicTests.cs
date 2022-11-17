using System;
using System.Linq;

using Deveel.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Mongo2Go;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Xunit;

namespace Deveel.Repository {
	public class MongoDbBasicTests : IDisposable {
		private readonly MongoDbRunner _server;
		private readonly MongoRepository<TestDocument> _store;

		public MongoDbBasicTests() {
			_server = MongoDbRunner.Start(logger: NullLogger.Instance);

			var services = new ServiceCollection()
				.AddMongoStoreOptions<TestDocument>(options => options
					.ConnectionString(_server.ConnectionString)
					.Database("testdb")
					.Collection("test"))
				.AddMongoDbStore<TestDocument>()
				.BuildServiceProvider();

			_store = services.GetRequiredService<MongoRepository<TestDocument>>();
		}

		[Fact]
		public async void CreateEntity() {
			var doc = new TestDocument();
			await _store.CreateAsync(doc);

			Assert.NotEqual(ObjectId.Empty, doc.Id);
		}

		[Fact]
		public async void CreateAndDeleteEntity() {
			var doc = new TestDocument();
			await _store.CreateAsync(doc);

			var result = await _store.DeleteAsync(doc);

			Assert.True(result);
		}

		[Fact]
		public async void CreateAndUpdateEntity() {
			var doc = new TestDocument();
			doc.Value = "test";
			await _store.CreateAsync(doc);

			doc.Value = "another test";

			await _store.UpdateAsync(doc);

			Assert.Equal("another test", doc.Value);
		}

		[Fact]
		public async void CreateAndGetPage() {
			for (int i = 0; i < 100; i++) {
				await _store.CreateAsync(new TestDocument {Value = $"test_{i}"});
			}

			var page = await _store.GetPageAsync(new PageRequest<TestDocument>(1, 10), default);

			Assert.NotNull(page);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(100, page.TotalItems);
			Assert.Equal(10, page.TotalPages);
			Assert.NotEmpty(page.Items);
			Assert.Equal(10, page.Items.Count());
		}

		[Fact]
		public async void CreateAndGetPageByValue() {
			for (int i = 0; i < 100; i++) {
				await _store.CreateAsync(new TestDocument { Value = $"test_{i}" });
			}

			var request = new PageRequest<TestDocument>(1, 10) {
				Filter = doc => doc.Value == "test_10"
			};

			var page = await _store.GetPageAsync(request, default);

			Assert.NotNull(page);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(1, page.TotalItems);
			Assert.Equal(1, page.TotalPages);
			Assert.NotEmpty(page.Items);
			Assert.Single(page.Items);
		}

		[Fact]
		public async void CreateAndGetAll() {
			for (int i = 0; i < 100; i++) {
				await _store.CreateAsync(new TestDocument { Value = $"test_{i}" });
			}

			var result = (await _store.FindAllAsync()).ToList();

			Assert.NotNull(result);
			Assert.Equal(100, result.Count);
		}

		[Fact]
		public async void CreateAndFindById() {
			var id = await _store.CreateAsync(new TestDocument{Value = "test_value"});

			Assert.NotNull(id);
			Assert.NotEmpty(id);

			var result = await _store.FindByIdAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id.ToString());
			Assert.Equal("test_value", result.Value);
		}

		/// <inheritdoc />
		public void Dispose() {
			_store?.Dispose();
			_server?.Dispose();
		}

		#region TestDocument

		public class TestDocument : IMongoDocument, IEntity {
			/// <inheritdoc />
			string IMongoDocument.Id => Id.ToString();

			/// <inheritdoc />
			public DateTimeOffset CreatedAt { get; set; }

			/// <inheritdoc />
			[BsonId]
			public ObjectId Id { get; set; }

			string? IEntity.Id => Id.ToEntityId();

			public string Value { get; set; }
		}

		#endregion
	}
}
