using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public static class DependencyInjectionTests {
		[Fact]
		public static void AddDefaultManager() {
			var services = new ServiceCollection();
			services.AddRepository<InMemoryRepository<Person>>();
			services.AddManagerFor<Person>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<EntityManager<Person>>());

			var manager = provider.GetRequiredService<EntityManager<Person>>();

			Assert.NotNull(manager);
			Assert.True(manager.SupportsPaging);
			Assert.True(manager.SupportsQueries);
			Assert.False(manager.IsMultiTenant);
		}

		[Fact]
		public static void AddCustomManager() {
			var services = new ServiceCollection();
			services.AddRepository<InMemoryRepository<Person>>();
			services.AddEntityManager<MyPersonManager>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<EntityManager<Person>>());
			Assert.NotNull(provider.GetService<MyPersonManager>());

			var manager = provider.GetRequiredService<EntityManager<Person>>();

			Assert.NotNull(manager);
			Assert.True(manager.SupportsPaging);
			Assert.True(manager.SupportsQueries);
			Assert.False(manager.IsMultiTenant);
		}

		[Fact]
		public static void AddEntityManager_NotValid() {
			var services = new ServiceCollection();
			Assert.Throws<ArgumentException>(() => services.AddEntityManager<NotEntityManager>());
		}

		[Fact]
		public static void AddEntityValidator() {
			var services = new ServiceCollection();
			services.AddEntityValidator<PersonValidator>();

			var provider = services.BuildServiceProvider();

			var validator = provider.GetRequiredService<IEntityValidator<Person>>();
			Assert.NotNull(validator);
		}

		[Fact]
		public static void AddOperationCancellationSource() {
			var services = new ServiceCollection();
			services.AddOperationTokenSource<TestCancellationTokenSource>();

			var provider = services.BuildServiceProvider();

			var source = provider.GetService<IOperationCancellationSource>();

			Assert.NotNull(source);
			Assert.IsType<TestCancellationTokenSource>(source);
		}

		[Fact]
		public static void AddHttpRequestCancellationSource() {
			var services = new ServiceCollection();
			services.AddHttpRequestTokenSource();

			var provider = services.BuildServiceProvider();

			var source = provider.GetService<IOperationCancellationSource>();

			Assert.NotNull(source);
			Assert.IsType<HttpRequestCancellationSource>(source);
			Assert.Equal(CancellationToken.None, source.Token);
		}

		#region NotEntityManager

		class NotEntityManager {
		}

		#endregion

		#region PersonValidator

		class PersonValidator : IEntityValidator<Person> {
			public async IAsyncEnumerable<ValidationResult> ValidateAsync(EntityManager<Person> manager, Person entity, CancellationToken cancellationToken = default) {
				yield return new ValidationResult("Test error");
			}
		}

		#endregion
	}
}
