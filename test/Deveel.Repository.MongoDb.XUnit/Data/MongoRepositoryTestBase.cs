using Bogus;

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

            PersonFaker = new Faker<MongoPerson>()
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.BirthDate, f => f.Date.Past(20));
		}

        protected string ConnectionString => mongo.ConnectionString;

        protected MongoRepository<MongoPerson> MongoRepository => serviceProvider.GetRequiredService<MongoRepository<MongoPerson>>();

        protected IRepository<MongoPerson> Repository => serviceProvider.GetRequiredService<IRepository<MongoPerson>>();

        protected IPageableRepository<MongoPerson> PageableRepository => Repository as IPageableRepository<MongoPerson>;

        protected IFilterableRepository<MongoPerson> FilterableRepository => Repository as IFilterableRepository<MongoPerson>;

        protected IRepository<IPerson> FacadeRepository => serviceProvider.GetRequiredService<IRepository<IPerson>>();

        protected IPageableRepository<IPerson> FacadePageableRepository => FacadeRepository as IPageableRepository<IPerson>;

        protected IFilterableRepository<IPerson> FilterableFacadeRepository => FacadeRepository as IFilterableRepository<IPerson>;

        protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

        protected Faker<MongoPerson> PersonFaker { get; set; }

        protected MongoPerson GeneratePerson() => PersonFaker.Generate();

        protected IList<MongoPerson> GeneratePersons(int count) => PersonFaker.Generate(count);

        protected virtual void AddRepository(IServiceCollection services) {
            services
                .AddMongoOptions(options => options
					.ConnectionString(mongo.ConnectionString)
					.Database("test_db")
					.Collection("Person", "persons"))
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

            string IMongoDocument.Id => Id.ToEntityId();

            string? IPerson.Id => Id.ToEntityId();

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime? BirthDate { get; set; }

            public string? Description { get; set; }
        }

        protected interface IPerson {
			string? Id { get; }

            string FirstName { get; }

            string LastName { get; }

            DateTime? BirthDate { get; }

            string? Description { get; }
        }
    }
}