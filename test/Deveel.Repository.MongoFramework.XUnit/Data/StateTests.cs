using Bogus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
    [Collection(nameof(MongoSingleDatabaseCollection))]
    public class StateTests : IAsyncLifetime {
        private Faker<MongoPersonWithStatus> personFaker;
        private IList<MongoPersonWithStatus> persons;
        private readonly MongoSingleDatabase mongo;

        public StateTests(MongoSingleDatabase mongo) {
            this.mongo = mongo;

            var services = new ServiceCollection();
            AddRepository(services);

            Services = services.BuildServiceProvider();

            personFaker = new Faker<MongoPersonWithStatus>()
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.BirthDate, f => f.Date.Past(20))
                .RuleFor(x => x.Status, f => f.Random.Enum<PersonStatus>().OrNull(f))
                .RuleFor(x => x.LastActorId, (f, p) => p.Status != null ? f.Random.Guid().ToString() : null)
                .RuleFor(x => x.LastStatusAt, (f, p) => p.Status != null ? f.Date.PastOffset() : null);

            persons = personFaker.Generate(100).ToList();
        }

        protected IServiceProvider Services { get; }

        private IRepository<MongoPersonWithStatus> Repository => Services.GetRequiredService<IRepository<MongoPersonWithStatus>>();

        private IStateRepository<MongoPersonWithStatus, PersonStatus>? StateRepository => Repository as IStateRepository<MongoPersonWithStatus, PersonStatus>;


        protected virtual void AddRepository(IServiceCollection services) {
            services
                .AddMongoDbContext<MongoDbContext>(builder => {
                    builder.UseConnection(mongo.SetDatabase("testdb"));
                })
                .AddRepositoryController();

			services.AddRepository<MongoStateRepository>();
		}

        public async Task InitializeAsync() {
            var controller = Services.GetRequiredService<IRepositoryController>();
            await controller.CreateRepositoryAsync<MongoPersonWithStatus>();

            await SeedAsync(Repository);

        }

        public async Task DisposeAsync() {
            var controller = Services.GetRequiredService<IRepositoryController>();
            await controller.DropRepositoryAsync<MongoPersonWithStatus>();
        }

        protected virtual async Task SeedAsync(IRepository<MongoPersonWithStatus> repository) {
            foreach (var person in persons) {
                await repository.AddAsync(person);
            }
        }

        [Fact]
        public async Task AddNewState() {
            var person = persons.First(x => x.Status == null);

            var state = new EntityStateInfo<PersonStatus>(PersonStatus.Alive, Guid.NewGuid().ToString(), DateTimeOffset.Now);

			Assert.NotNull(StateRepository);

            await StateRepository.AddStateAsync(person, state);
            await StateRepository.UpdateAsync(person);

            var updated = await StateRepository.FindByKeyAsync(person.Id.ToString());

            Assert.NotNull(updated);
            Assert.Equal(PersonStatus.Alive, updated.Status);
            Assert.Equal(state.ActorId, updated.LastActorId);
            Assert.Equal(state.TimeStamp, updated.LastStatusAt);
        }

        [Fact]
        public async Task GetPersonWithState() {
            var person = persons.First(x => x.Status != null);

			Assert.NotNull(StateRepository);

			var states = await StateRepository.GetStatesAsync(person);

            Assert.NotNull(states);
            Assert.NotEmpty(states);
            Assert.Equal(person.Status, states[0].Status);
            Assert.Equal(person.LastActorId, states[0].ActorId);
            Assert.Equal(person.LastStatusAt, states[0].TimeStamp);
        }

        [Fact]
        public async Task RemoveState() {
            var person = persons.First(x => x.Status != null);
            
            var state = new EntityStateInfo<PersonStatus>(person.Status!.Value, person.LastActorId!, person.LastStatusAt!);

			Assert.NotNull(StateRepository);

			await StateRepository.RemoveStateAsync(person, state);

            var updated = await StateRepository.FindByKeyAsync(person.Id.ToString());
            Assert.NotNull(updated);
            Assert.Null(updated.Status);
            Assert.Null(updated.LastActorId);
            Assert.Null(updated.LastStatusAt);
        }

        #region StateRepository

        class MongoStateRepository : MongoRepository<MongoPersonWithStatus>, IStateRepository<MongoPersonWithStatus, PersonStatus> {
            public MongoStateRepository(MongoDbContext context, ILogger<MongoStateRepository>? logger = null) 
                : base(context, null, logger) {
            }

            public Task AddStateAsync(MongoPersonWithStatus entity, EntityStateInfo<PersonStatus> state, CancellationToken cancellationToken = default) {
                entity.Status = state.Status;
                entity.LastStatusAt = state.TimeStamp;
                entity.LastActorId = state.ActorId;
                return Task.CompletedTask;
            }

            public Task<IList<EntityStateInfo<PersonStatus>>> GetStatesAsync(MongoPersonWithStatus entity, CancellationToken cancellationToken = default) {
                var result = new List<EntityStateInfo<PersonStatus>>();
                if (entity.Status != null) {
                    result.Add(new EntityStateInfo<PersonStatus>(entity.Status.Value, entity.LastActorId!, entity.LastStatusAt!));
                }

                return Task.FromResult<IList<EntityStateInfo<PersonStatus>>>(result);
            }

            public Task RemoveStateAsync(MongoPersonWithStatus entity, EntityStateInfo<PersonStatus> state, CancellationToken cancellationToken = default) {
                entity.Status = null;
                entity.LastStatusAt = null;
                entity.LastActorId = null;

                return Task.CompletedTask;
            }
        }

        #endregion

        #region IPerson

        interface IPerson {
            string? Id { get; }

            string FirstName { get; }

            string LastName { get; }

            DateTime? BirthDate { get; }
        }

		#endregion

		#region MngoPersonWithState

// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618


		protected class MongoPersonWithStatus : IPerson {
            public ObjectId Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime? BirthDate { get; set; }  

            public PersonStatus? Status { get; set; }

            public string? LastActorId { get; set; }

            public DateTimeOffset? LastStatusAt { get; set; }

            string? IPerson.Id => Id.ToEntityId();
        }

#pragma warning restore CS8618

        #endregion

        #region PersonState

        protected enum PersonStatus {
            Unknown = 0,
            Alive,
            Dead
        }

        #endregion
    }
}
