using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
    public static class MongoRepositoryResolutionTests {
        [Fact]
        public static void ResolveFromMongoDbOptions() {
            var services = new ServiceCollection()
                .AddMongoOptions(options => options
                    .ConnectionString("mongodb://127.0.0.1:5617/")
                    .Database("test_db")
                    .Collection("TestEntity", "test_collection"))
                .AddCollectionKeyProvider()
                .AddMongoRepository<MongoTestEntityRepository, MongoTestEntity>()
                .BuildServiceProvider();

            var repository = services.GetRequiredService<MongoRepository<MongoTestEntity>>();

            Assert.NotNull(repository);
        }

        [Fact]
        public static void ResolveFromMongoStoreOptions() {
            var services = new ServiceCollection()
                .AddMongoStoreOptions<MongoTestEntity>(options =>  options
                    .ConnectionString("mongodb://127.0.0.1:5617/")
                    .Database("test_db")
                    .Collection("TestEntity"))
                .AddMongoRepository<MongoTestEntity>()
                .BuildServiceProvider();

            var repository = services.GetRequiredService<MongoRepository<MongoTestEntity>>();

            Assert.NotNull(repository);
        }


        class MongoTestEntity : IDataEntity {
            public string Id { get; set; }
        }

        class MongoTestEntityRepository : MongoRepository<MongoTestEntity> {
            public MongoTestEntityRepository(IOptions<MongoDbOptions> options, ICollectionKeyProvider keyProvider, IDocumentFieldMapper<MongoTestEntity>? fieldMapper = null, ILogger<MongoTestEntityRepository>? logger = null) 
                : base(options, keyProvider, fieldMapper, logger) {
            }
        }
    }
}
