using Bogus;

using Marten;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(PostgresTestCollection))]
	public class MartenDocumentRepositoryTestSuite : RepositoryTestSuite<PersonDocument, PersonRelationship> {
		private readonly PostgresDatabase postgres;

		public MartenDocumentRepositoryTestSuite(PostgresDatabase postgres, ITestOutputHelper? testOutput) : base(testOutput) {
			this.postgres = postgres;
		}

		protected override Faker<PersonDocument> PersonFaker => new PersonDocumentFaker();

		protected override Faker<PersonRelationship> RelationshipFaker => new PersonRelationshipFaker();

		protected override Task AddRelationshipAsync(PersonDocument person, PersonRelationship relationship) {
			if (person.Relationships == null)
				person.Relationships = new List<PersonRelationship>();

			person.Relationships.Add(relationship);

			return Task.CompletedTask;
		}

		protected override Task RemoveRelationshipAsync(PersonDocument person, PersonRelationship relationship) {
			if (person.Relationships == null)
				return Task.CompletedTask;

			var rel = person.Relationships.FirstOrDefault(x => x.Type == relationship.Type && x.FullName == relationship.FullName);
			if (rel != null)
				person.Relationships.Remove(rel);

			return Task.CompletedTask;
		}

		protected override void SetFirstName(PersonDocument person, string firstName) {
			person.FirstName = firstName;
		}

		protected override void SetPersonId(PersonDocument person, string id) {
			person.Id = id;
		}

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddMarten(options => {
				options.Connection(postgres.ConnectionString);
			});

			services.AddRepository<MartenDocumentRepository<PersonDocument>>();
		}

		protected override async Task<IList<PersonDocument>> FindAllPeopleAsync() {
			var store = Services.GetRequiredService<IDocumentStore>();
			using var session = await store.QuerySerializableSessionAsync();

			return (await session.Query<PersonDocument>().ToListAsync()).ToList();
		}

		protected override async Task SeedAsync(IRepository<PersonDocument> repository) {
			var store = Services.GetRequiredService<IDocumentStore>();
			using var session = store.LightweightSession();

			session.Insert<PersonDocument>(People);
			await session.SaveChangesAsync();
		}

		protected override async Task DisposeAsync() {
			var store = Services.GetRequiredService<IDocumentStore>();
			await store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(PersonDocument));
		}
	}
}
