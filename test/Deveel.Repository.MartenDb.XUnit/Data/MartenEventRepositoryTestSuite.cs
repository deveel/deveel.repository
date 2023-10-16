using Bogus;

using Marten;
using Marten.Events;
using Marten.Events.Projections;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(PostgresTestCollection))]
	public class MartenEventRepositoryTestSuite : RepositoryTestSuite<PersonAggregate, PersonRelationship> {
		private readonly PostgresDatabase postgres;

		public MartenEventRepositoryTestSuite(PostgresDatabase postgres, ITestOutputHelper? testOutput) : base(testOutput) {
			this.postgres = postgres;
		}

		protected override Faker<PersonAggregate> PersonFaker => new PersonAggregateFaker();

		protected override Faker<PersonRelationship> RelationshipFaker => new PersonRelationshipFaker();

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddMarten(options => {
				options.Connection(postgres.ConnectionString);
				options.Events.StreamIdentity = StreamIdentity.AsString;
				options.Projections.Add<PersonProjection>(ProjectionLifecycle.Inline);
			});
			services.AddRepository<MartenEventRepository<PersonAggregate>>();
		}

		protected override void SetFirstName(PersonAggregate person, string firstName) {
			person.Apply(new PersonNameChanged(firstName, null));
		}

		protected override void SetPersonId(PersonAggregate person, string id) {
			person.Apply(new PersonIdChanged(id));
		}

		protected override Task AddRelationshipAsync(PersonAggregate person, PersonRelationship relationship) {
			person.Apply(new RelationshipAdded(relationship.Type, relationship.FullName));
			return Task.CompletedTask;
		}

		protected override Task RemoveRelationshipAsync(PersonAggregate person, PersonRelationship relationship) {
			person.Apply(new RelationshipRemoved(relationship.Type, relationship.FullName));
			return Task.CompletedTask;
		}

		protected override async Task SeedAsync(IRepository<PersonAggregate> repository) {
			var store = Services.GetRequiredService<IDocumentStore>();
			using var session = store.LightweightSession();

			foreach (var person in People) {
				session.Events.StartStream(person.Id, person.Events.Uncommitted);
			}

			await session.SaveChangesAsync();
		}

		protected override async Task DisposeAsync() {
			var store = Services.GetRequiredService<IDocumentStore>();
			await store.Advanced.Clean.DeleteAllEventDataAsync();
		}

		protected override async Task<IList<PersonAggregate>> FindAllPeopleAsync() {
			var store = Services.GetRequiredService<IDocumentStore>();
			using var session = store.QuerySession();

			var streamKeys = session.Events.QueryAllRawEvents().Select(x => x.StreamKey).Distinct().ToArray();

			var people = new List<PersonAggregate>();
			foreach (var stream in streamKeys) {
				var person = await session.Events.AggregateStreamAsync<PersonAggregate>(stream);

				if (person != null)
					people.Add(person);
			}

			return people;
		}

		public override Task CountAll() => Task.CompletedTask;

		public override void CountAll_Sync() {
			return;
		}

		public override Task CountFiltered() => Task.CompletedTask;

		public override Task CountFiltered_Sync() => Task.CompletedTask;

		public override Task ExistsFiltered() => Task.CompletedTask;

		public override Task ExistsFiltered_Sync() => Task.CompletedTask;

		public override Task FindFirst() => Task.CompletedTask;

		public override void FindFirstSync() {
			return;
		}

		public override Task FindFirstFiltered() => Task.CompletedTask;

		public override Task FindFirstFiltered_Sync() => Task.CompletedTask;

		public override Task FindAll() => Task.CompletedTask;

		public override Task FindAllFiltered() => Task.CompletedTask;

		public override Task FindAllFiltered_BadFilter() => Task.CompletedTask;

		public override void FindAll_Sync() {
			return;
		}

		public override Task GetSimplePage() => Task.CompletedTask;

		public override Task GetSortedPage() => Task.CompletedTask;

		public override Task GetDescendingSortedPage() => Task.CompletedTask;

		public override Task GetFilteredPage() => Task.CompletedTask;

		public override void GetPage_Sync() {
			return;
		}

		public override Task GetPage_ChainedFilters() => Task.CompletedTask;

		public override Task GetPage_MultipleFilters() => Task.CompletedTask;

		public override Task GetSimplePage_WithParameters() => Task.CompletedTask;

		[Fact]
		public async Task FindByKeyAndVersion() {
			var person = await RandomPersonAsync();

			var result = await Repository.FindByKeyAsync(person.Id!, person.Version);

			Assert.NotNull(result);
			Assert.Equal(person.Id, result.Id);
			Assert.Equal(person.Version, result.Version);
		}
	}
}
