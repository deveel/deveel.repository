using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Deveel.Data {
    [Collection("Mongo Single Database")]
    public abstract class MongoRepositoryTestBase : IAsyncLifetime {
        private MongoDbTestFixture mongo;
        private readonly IServiceProvider serviceProvider;

		protected MongoRepositoryTestBase(MongoDbTestFixture mongo) {
			this.mongo = mongo;

            var services = new ServiceCollection();
            AddRepository(services);

            services.AddMongoTransactionFactory();

            serviceProvider = services.BuildServiceProvider();
		}

        protected string ConnectionString => mongo.ConnectionString;

        protected MongoRepository<MongoPerson> MongoRepository => serviceProvider.GetRequiredService<MongoRepository<MongoPerson>>();

        protected IRepository<MongoPerson> Repository => serviceProvider.GetRequiredService<IRepository<MongoPerson>>();

        protected IRepository<IPerson> FacadeRepository => serviceProvider.GetRequiredService<IRepository<IPerson>>();

        protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

        protected virtual void AddRepository(IServiceCollection services) {
            services
                .AddMongoOptions(options => options.ConnectionString(mongo.ConnectionString))
                .AddMongoStoreOptions<MongoPerson>(options => options
                    .ConnectionString(ConnectionString)
                    .Database("test_db")
                    .Collection("persons"))
                .AddMongoRepository<MongoPerson>()
                .AddMongoFacadeRepository<MongoPerson, IPerson>()
                .AddRepositoryController();
        }

        protected virtual Task SeedAsync(MongoRepository<MongoPerson> repository) { 
            return Task.CompletedTask;
        }

        public virtual async Task InitializeAsync() {
            var controller = serviceProvider.GetRequiredService<IRepositoryController>();
            await controller.CreateRepositoryAsync<MongoPerson>();
			
			await SeedAsync(MongoRepository);
        }

        public virtual async Task DisposeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.DropRepositoryAsync<MongoPerson>();
		}

		protected class MongoPerson : IMongoDocument, IPerson {
            [BsonId]
            public ObjectId Id { get; set; }

            string? IMongoDocument.Id => Id.ToEntityId();

            string? IEntity.Id => Id.ToEntityId();

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateOnly? BirthDate { get; set; }

            public string? Description { get; set; }
        }

        protected interface IPerson : IEntity {
            public string FirstName { get; }

            public string LastName { get; }

            public DateOnly? BirthDate { get; }

            public string? Description { get; }
        }
    }
}