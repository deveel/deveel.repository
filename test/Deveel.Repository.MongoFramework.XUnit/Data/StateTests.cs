﻿using Bogus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
    [Collection("Mongo Single Database")]
    public class StateTests : IAsyncLifetime {
        private Faker<MongoPersonWithStatus> personFaker;
        private IList<MongoPersonWithStatus> persons;
        private readonly MongoFrameworkTestFixture mongo;

        public StateTests(MongoFrameworkTestFixture mongo) {
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

        private IStateRepository<MongoPersonWithStatus, PersonStatus> StateRepository => Repository as IStateRepository<MongoPersonWithStatus, PersonStatus>;


        protected virtual void AddRepository(IServiceCollection services) {
            services
                .AddMongoContext(builder => {
                    builder.UseConnection(mongo.SetDatabase("testdb"));
                    AddRepository(builder);
                })
                .AddRepositoryController();
        }

        protected virtual void AddRepository(MongoDbContextBuilder<MongoDbContext> builder) {
            builder.AddRepository<MongoPersonWithStatus>()
                .Use<MongoStateRepository>();
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
                await repository.CreateAsync(person);
            }
        }

        [Fact]
        public async Task AddNewState() {
            var person = persons.First(x => x.Status == null);

            var state = new EntityStateInfo<PersonStatus>(PersonStatus.Alive, Guid.NewGuid().ToString(), DateTimeOffset.Now);

            await StateRepository.AddStateAsync(person, state);
            await StateRepository.UpdateAsync(person);

            var updated = await StateRepository.FindByIdAsync(person.Id.ToString());

            Assert.NotNull(updated);
            Assert.Equal(PersonStatus.Alive, updated.Status);
            Assert.Equal(state.ActorId, updated.LastActorId);
            Assert.Equal(state.TimeStamp, updated.LastStatusAt);
        }

        [Fact]
        public async Task GetPersonWithState() {
            var person = persons.First(x => x.Status != null);

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

            await StateRepository.RemoveStateAsync(person, state);

            var updated = await StateRepository.FindByIdAsync(person.Id.ToString());
            Assert.NotNull(updated);
            Assert.Null(updated.Status);
            Assert.Null(updated.LastActorId);
            Assert.Null(updated.LastStatusAt);
        }

        #region StateRepository

        class MongoStateRepository : MongoRepository<MongoPersonWithStatus>, IStateRepository<MongoPersonWithStatus, PersonStatus> {
            public MongoStateRepository(MongoDbContext context, ILogger<MongoStateRepository>? logger = null) 
                : base(context, logger) {
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