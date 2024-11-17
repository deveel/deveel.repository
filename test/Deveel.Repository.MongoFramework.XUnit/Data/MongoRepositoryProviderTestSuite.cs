using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

using Xunit.Abstractions;

namespace Deveel.Data
{
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MongoRepositoryProviderTestSuite : RepositoryProviderTestSuite<MongoPerson, ObjectId, MongoPersonRelationship>
	{
		private MongoSingleDatabase mongo;

		public MongoRepositoryProviderTestSuite(MongoSingleDatabase mongo, ITestOutputHelper? testOutput) : base(testOutput)
		{
			this.mongo = mongo;
		}

		protected override Faker<MongoPerson> PersonFaker => new MongoPersonFaker();

		protected override void ConfigureRepositoryProvider(IServiceCollection services)
		{
			services.AddSingleton(new MongoDbTenantInfo
			{
				Id = Guid.NewGuid().ToString(),
				Identifier = TenantId,
				ConnectionString = mongo.ConnectionString
			});

			services.AddRepository<MongoRepository<MongoPerson, ObjectId>>();
			services.AddMongoRepositoryProvider<MongoRepository<MongoPerson, ObjectId>>();
		}

		protected override ObjectId GeneratePersonId() => ObjectId.GenerateNewId();

		protected override Faker<MongoPersonRelationship> RelationshipFaker => new MongoPersonRelationshipFaker();

		protected override Task AddRelationshipAsync(MongoPerson person, MongoPersonRelationship relationship)
		{
			if (person.Relationships == null)
				person.Relationships = new List<MongoPersonRelationship>();

			person.Relationships.Add(relationship);

			return Task.CompletedTask;
		}

		protected override Task RemoveRelationshipAsync(MongoPerson person, MongoPersonRelationship relationship)
		{
			if (person.Relationships != null)
				person.Relationships.Remove(relationship);

			return Task.CompletedTask;
		}
	}
}
