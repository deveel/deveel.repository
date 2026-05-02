using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
    [Category("Integration")]
    public abstract class MongoRepositoryNoKeyTestSuite<TPerson> : RepositoryTestSuite<TPerson, MongoPersonRelationship>, IAsyncInitializer, IAsyncDisposable
        where TPerson : MongoPerson {
        private MongoSingleDatabase _mongo = default!;

        protected string ConnectionString { get; private set; } = default!;

        protected override string GeneratePersonId() => ObjectId.GenerateNewId().ToString();

        protected override Faker<MongoPersonRelationship> RelationshipFaker => new MongoPersonRelationshipFaker();

        protected IMongoCollection<TPerson> MongoCollection => new MongoClient(_mongo.ConnectionString)
            .GetDatabase(new MongoUrl(ConnectionString).DatabaseName)
            .GetCollection<TPerson>("persons");

        protected override Task AddRelationshipAsync(TPerson person, MongoPersonRelationship relationship) {
            if (person.Relationships == null)
                person.Relationships = new List<MongoPersonRelationship>();

            person.Relationships.Add(relationship);

            return Task.CompletedTask;
        }

        protected override Task RemoveRelationshipAsync(TPerson person, MongoPersonRelationship relationship) {
            if (person.Relationships != null)
                person.Relationships.Remove(relationship);

            return Task.CompletedTask;
        }

        // Explicitly implement IAsyncInitializer so we can start MongoDB before base initialization
        async Task IAsyncInitializer.InitializeAsync() {
            _mongo = new MongoSingleDatabase();
            await _mongo.StartAsync();
            ConnectionString = _mongo.ConnectionString;

            await base.InitializeAsync();
        }

        // Create the repository collection before seeding
        protected override async Task InitializeAsync(IRepository<TPerson> repository) {
            var controller = Services.GetRequiredService<IRepositoryController>();
            await controller.CreateRepositoryAsync<MongoPerson>();

            await base.InitializeAsync(repository);
        }

        // CleanupAsync is called by base DisposeAsync() before the scope is disposed
        protected override async Task CleanupAsync() {
            var controller = Services.GetRequiredService<IRepositoryController>();
            await controller.DropRepositoryAsync<MongoPerson>();
        }

        // Explicitly implement IAsyncDisposable to stop MongoDB after base disposes the scope
        async ValueTask IAsyncDisposable.DisposeAsync() {
            await base.DisposeAsync();
            await _mongo.DisposeAsync();
        }

        [Test]
        [Category("Integration")]
        public async Task FindByObjectId() {
            var person = await RandomPersonAsync();

            var found = await Repository.FindAsync(person.Id, CancellationToken.None);

            await Assert.That(found).IsNotNull();
            await Assert.That(found!.Id).IsEqualTo(person.Id);
        }

        [Test]
        [Category("Integration")]
        public async Task FindByObjectId_NotExisting() {
            var personId = ObjectId.GenerateNewId();

            var found = await Repository.FindAsync(personId, CancellationToken.None);

            await Assert.That(found).IsNull();
        }

        [Test]
        [Category("Integration")]
        public async Task FindFirstByGeoDistance() {
            var person = await RandomPersonAsync(x => x.Location != null);

            var point = new GeoPoint(person.Location!.Coordinates.Latitude, person.Location.Coordinates.Longitude);
            var found = await Repository.FindFirstByGeoDistanceAsync(x => x.Location!, point, 100);

            await Assert.That(found).IsNotNull();
            await Assert.That(found!.Id).IsEqualTo(person.Id);
        }

        [Test]
        [Category("Integration")]
        public async Task FindAllByGeoDistance() {
            var person = await RandomPersonAsync(x => x.Location != null);

            var point = new GeoPoint(person.Location!.Coordinates.Latitude, person.Location.Coordinates.Longitude);
            var found = await Repository.FindAllByGeoDistanceAsync(x => x.Location!, point, 100);

            await Assert.That(found).IsNotNull();
            await Assert.That(found.Count).IsGreaterThan(0);
            await Assert.That(found.Any(x => x.Id == person.Id)).IsTrue();
        }
    }
}


