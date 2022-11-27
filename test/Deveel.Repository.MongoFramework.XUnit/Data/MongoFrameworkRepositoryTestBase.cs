using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]
	public abstract class MongoFrameworkRepositoryTestBase : IAsyncLifetime {
		private MongoFrameworkTestFixture mongo;
		private readonly IServiceProvider serviceProvider;

		protected MongoFrameworkRepositoryTestBase(MongoFrameworkTestFixture mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepository(services);

			serviceProvider = services.BuildServiceProvider();
		}

		protected string ConnectionString => mongo.ConnectionString;

		protected MongoRepository<MongoPerson> MongoRepository => serviceProvider.GetRequiredService<MongoRepository<MongoPerson>>();

		protected IRepository<MongoPerson> Repository => serviceProvider.GetRequiredService<IRepository<MongoPerson>>();

		protected IRepository<IPerson> FacadeRepository => serviceProvider.GetRequiredService<IRepository<IPerson>>();

		protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

		protected virtual void AddRepository(IServiceCollection services) {
			services
				.AddMongoContext(options => { 
					options.ConnectionString = ConnectionString;
					options.DatabaseName = "test_db";
				})
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

		protected class MongoPerson : IPerson {
			[Key]
			public ObjectId Id { get; set; }


			string IEntity.Id => Id.ToEntityId();

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateOnly? BirthDate { get; set; }

			public string? Description { get; set; }

			[Version]
			public string Version { get; set; }
		}

		protected interface IPerson : IEntity {
			public string FirstName { get; }

			public string LastName { get; }

			public DateOnly? BirthDate { get; }

			public string? Description { get; }
		}
	}
}
