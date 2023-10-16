using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public class DependencyInjectionTests {
		[Fact]
		public static void RegisterRepositoryFromCustomContract() {
			var services = new ServiceCollection();
			services.AddRepository<MyPersonRepository>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IPersonRepository>());
			Assert.NotNull(provider.GetService<IRepository<Person>>());
			Assert.NotNull(provider.GetService<MyRepository<Person>>());
			Assert.NotNull(provider.GetService<MyPersonRepository>());
		}
	}

	interface IPersonRepository : IRepository<Person> {
		Task<Person?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
	}

	class MyRepository<TEntity> : IRepository<TEntity>
		where TEntity : class {
		protected IRepository<TEntity> Repository { get; }

		public MyRepository() {
			Repository = new List<TEntity>(200).AsRepository();
		}

		public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => Repository.AddAsync(entity, cancellationToken);
		public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => Repository.AddRangeAsync(entities, cancellationToken);
		public Task<TEntity?> FindByKeyAsync(object key, CancellationToken cancellationToken = default) => Repository.FindByKeyAsync(key, cancellationToken);
		public object? GetEntityKey(TEntity entity) => Repository.GetEntityKey(entity);
		public Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) => Repository.RemoveAsync(entity, cancellationToken);
		public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => Repository.RemoveRangeAsync(entities, cancellationToken);
		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => Repository.UpdateAsync(entity, cancellationToken);
	}

	class MyPersonRepository : MyRepository<Person>, IPersonRepository {

		public Task<Person?> FindByNameAsync(string name, CancellationToken cancellationToken = default) {
			return Repository.FindFirstAsync(x => x.FirstName == name, cancellationToken);
		}
	}
}
