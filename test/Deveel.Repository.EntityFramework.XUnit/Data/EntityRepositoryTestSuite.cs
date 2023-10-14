using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(SqlConnectionCollection))]
	public class EntityRepositoryTestSuite : RepositoryTestSuite<DbPerson, DbRelationship> {
		private readonly SqlTestConnection sql;

		public EntityRepositoryTestSuite(SqlTestConnection sql, ITestOutputHelper? testOutput) : base(testOutput) {
			this.sql = sql;
		}

		protected string ConnectionString => sql.Connection.ConnectionString;

		protected override Faker<DbPerson> PersonFaker => new DbPersonFaker();

		protected override Faker<DbRelationship> RelationshipFaker => new DbPersonRelationshipFaker();

		protected DbPersonRepository PersonRepository => (DbPersonRepository)Repository;

		protected override Task AddRelationshipAsync(DbPerson person, DbRelationship relationship) {
			if (person.Relationships == null)
				person.Relationships = new List<DbRelationship>();

			person.Relationships.Add(relationship);

			return Task.CompletedTask;
		}

		protected override Task RemoveRelationshipAsync(DbPerson person, DbRelationship relationship) {
			if (person.Relationships != null)
				person.Relationships.Remove(relationship);

			return Task.CompletedTask;
		}

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddDbContext<DbContext, PersonDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
				builder.UseSqlite(sql.Connection);
			})
			.AddRepository<DbPersonRepository>();

			base.ConfigureServices(services);
		}

		protected override async Task InitializeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<PersonDbContext>>();
			using var dbContext = new PersonDbContext(options);

			await dbContext.Database.EnsureDeletedAsync();
			await dbContext.Database.EnsureCreatedAsync();

			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<PersonDbContext>>();
			using var dbContext = new PersonDbContext(options);

			dbContext.People!.RemoveRange(dbContext.People);
			await dbContext.SaveChangesAsync(true);

			await dbContext.Database.EnsureDeletedAsync();
		}

		protected override IEnumerable<DbPerson> NaturalOrder(IEnumerable<DbPerson> source) => source.OrderBy(x => x.Id);

		[Fact]
		public async Task FindByKey_WithRelationships() {
			var person = await RandomPersonAsync(x => x.Relationships != null);

			var found = await Repository.FindByKeyAsync(person.Id!);

			Assert.NotNull(found);
			Assert.NotNull(found.Relationships);
			Assert.NotEmpty(found.Relationships);
		}

		[Fact]
		public async Task UpdateEmail() {
			var newEmail = new Faker().Internet.Email();
			var person = await RandomPersonAsync(x => x.Email != newEmail);

			var toUpdate = await PersonRepository.FindByKeyAsync(person.Id!);

			await PersonRepository.SetEmailAsync(toUpdate!, newEmail);
			Assert.True(await PersonRepository.UpdateAsync(toUpdate!));

			var updated = await PersonRepository.FindByKeyAsync(person.Id!);

			Assert.NotNull(updated);
			Assert.Equal(newEmail, updated.Email);
		}
	}
}
