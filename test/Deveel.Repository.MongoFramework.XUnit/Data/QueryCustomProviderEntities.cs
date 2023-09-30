namespace Deveel.Data {
    public class QueryCustomProviderEntities : CustomMongoRepositoryProviderTests {
        private readonly IList<MongoTenantPerson> people;

        public QueryCustomProviderEntities(MongoSingleDatabase mongo) : base(mongo) {
            people = GeneratePersons(100);
        }

        protected override async Task SeedAsync(IRepository<MongoTenantPerson> repository) {
            await repository.AddRangeAsync(people);
        }


        [Fact]
        public void ValidateRepositoryTypes() {
            Assert.IsType<PersonRepository>(Repository);
            Assert.IsType<PersonRepositoryProvider>(RepositoryProvider);
        }


        [Fact]
        public async Task Mongo_CountAll() {
            var result = await MongoRepository.CountAllAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(people.Count, result);
        }

        [Fact]
        public async Task Repository_CountAll() {
            var result = await FilterableRepository.CountAllAsync();

            Assert.NotEqual(0, result);
            Assert.Equal(people.Count, result);
        }

        [Fact]
        public async Task Mongo_CountFiltered() {
            var firstName = people.Random()!.FirstName;
            var peopleCount = people.Count(x => x.FirstName == firstName);

            var count = await MongoRepository.CountAsync(p => p.FirstName == firstName);

            Assert.Equal(peopleCount, count);
        }

        [Fact]
        public async Task Repository_CountFiltered() {
            var firstName = people.Random()!.FirstName;
            var peopleCount = people.Count(x => x.FirstName == firstName);

            var count = await FilterableRepository.CountAsync(p => p.FirstName == firstName);

            Assert.Equal(peopleCount, count);
        }


    }
}
