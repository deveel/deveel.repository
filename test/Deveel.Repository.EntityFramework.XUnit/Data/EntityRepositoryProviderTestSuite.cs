﻿using Bogus;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(SqlTenantConnectionCollection))]
    public class EntityRepositoryProviderTestSuite : RepositoryTestSuite<DbTenantPerson, Guid, DbTenantPersonRelationship> {
        private readonly SqliteConnection sqliteConnection;

        public EntityRepositoryProviderTestSuite(SqlTestConnection sql, ITestOutputHelper outputHelper) : base(outputHelper) {
            this.sqliteConnection = sql.Connection;

			TenantInfo = new TenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test",
				Name = "Test Tenant",
				ConnectionString = sqliteConnection.ConnectionString
			};

			PersonFaker = new DbTenantPersonFaker(TenantInfo.Id);
        }

        protected override Faker<DbTenantPerson> PersonFaker { get; }

		protected override Faker<DbTenantPersonRelationship> RelationshipFaker => new DbTenantPersonRelationshipFaker();

		protected TenantInfo TenantInfo { get; }

		protected override Guid GeneratePersonId() => Guid.NewGuid();

		protected string TenantId => TenantInfo.Id;

        protected IRepositoryProvider<DbTenantPerson, Guid> RepositoryProvider 
			=> Services.GetRequiredService<IRepositoryProvider<DbTenantPerson, Guid>>();

        protected override IRepository<DbTenantPerson, Guid> Repository => RepositoryProvider.GetRepository(TenantId);

		protected override IEnumerable<DbTenantPerson> NaturalOrder(IEnumerable<DbTenantPerson> source) => source.OrderBy(x => x.Id);

		protected override Task AddRelationshipAsync(DbTenantPerson person, DbTenantPersonRelationship relationship) {
			if (person.Relationships == null)
				person.Relationships = new List<DbTenantPersonRelationship>();

			person.Relationships.Add(relationship);

			return Task.CompletedTask;
		}

		protected override Task RemoveRelationshipAsync(DbTenantPerson person, DbTenantPersonRelationship relationship) {
			if (person.Relationships != null)
				person.Relationships.Remove(relationship);

			return Task.CompletedTask;
		}

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(TenantInfo);
				});

			services.AddRepositoryTenantResolver<TenantInfo>();
			services.AddDbContext<DbContext, TestDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
				builder.UseSqlite(sqliteConnection, x => x.UseNetTopologySuite());
				builder.LogTo(TestOutput!.WriteLine);
			})
				.AddRepositoryProvider<EntityRepositoryProvider<TestDbContext, DbTenantPerson, Guid>>();

			base.ConfigureServices(services);
		}

		protected override async Task InitializeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<TestDbContext>>();
			using var dbContext = new TestDbContext(TenantInfo, options);

			await dbContext.Database.EnsureDeletedAsync();
			await dbContext.Database.EnsureCreatedAsync();

			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<TestDbContext>>();
			using var dbContext = new TestDbContext(TenantInfo, options);

			dbContext.People!.RemoveRange(dbContext.People);
			await dbContext.SaveChangesAsync(true);

			await dbContext.Database.EnsureDeletedAsync();
		}

		protected class TestDbContext : MultiTenantDbContext {
            public TestDbContext(ITenantInfo tenantInfo, DbContextOptions<TestDbContext> options) : base(tenantInfo, options) {
            }

			public virtual DbSet<DbTenantPerson> People { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<DbTenantPerson>()
                    .HasMany(x => x.Relationships)
                    .WithOne(x => x.Person)
                    .HasForeignKey(x => x.PersonId)
					.OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<DbTenantPerson>()
                    .IsMultiTenant();

                modelBuilder.Entity<DbTenantPersonRelationship>()
					.HasOne(x => x.Person)
					.WithMany(x => x.Relationships)
					.HasForeignKey(x => x.PersonId)
					.OnDelete(DeleteBehavior.Cascade);

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
