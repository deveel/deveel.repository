using Deveel.Data.Caching;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
	public static class DependencyInjectionTests {
		[Fact]
		public static void RegisterEntityCacheOptions() {
			var services = new ServiceCollection();
			services.AddEntityCacheOptions<Person>(options => {
				options.Expiration = TimeSpan.FromMinutes(15);
			});

			var provider = services.BuildServiceProvider();

			var options = provider.GetService<IOptions<EntityCacheOptions<Person>>>();

			Assert.NotNull(options);
			Assert.NotNull(options.Value);
			Assert.Equal(TimeSpan.FromMinutes(15), options.Value.Expiration);
		}

		[Fact]
		public static void RegisterEntityCacheOptions_FromConfig() {
			var services = new ServiceCollection();
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?> {
					{"EntityCacheOptions:Person:Expiration", "00:15:00"}
				});
			services.AddSingleton<IConfiguration>(config.Build());
			services.AddEntityCacheOptions<Person>("EntityCacheOptions:Person");

			var provider = services.BuildServiceProvider();

			var options = provider.GetService<IOptions<EntityCacheOptions<Person>>>();

			Assert.NotNull(options);
			Assert.NotNull(options.Value);
			Assert.Equal(TimeSpan.FromMinutes(15), options.Value.Expiration);
		}

		[Fact]
		public static void RegisterEntityCache_InMemory() {
			var services = new ServiceCollection();
			services.AddEasyCaching(options => {
				options.UseInMemory("default");
			});

			services.AddEntityEasyCacheFor<Person>(options => {
				options.Expiration = TimeSpan.FromMinutes(15);
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IEntityCache<Person>>());
			Assert.NotNull(provider.GetService<EntityEasyCache<Person>>());
		}

		[Fact]
		public static void RegisterNotCache() {
			var services = new ServiceCollection();
			Assert.Throws<ArgumentException>(() => services.AddEntityEasyCache<NotCache>());
		}

		[Fact]
		public static void RegisterEntityCacheKeyGenerator() {
			var services = new ServiceCollection();
			services.AddEntityCacheKeyGenerator<PersonCacheKeyGenerator>();

			var provider = services.BuildServiceProvider();

			var generator = provider.GetService<IEntityCacheKeyGenerator<Person>>();

			Assert.NotNull(generator);
			Assert.IsType<PersonCacheKeyGenerator>(generator);
		}

		[Fact]
		public static void RegisterEntityCacheConverter() {
			var services = new ServiceCollection();
			services.AddEntityEasyCacheConverter<PersonCacheConverter>();

			var provider = services.BuildServiceProvider();

			var converter = provider.GetService<IEntityEasyCacheConverter<Person, CachedPerson>>();

			Assert.NotNull(converter);
			Assert.IsType<PersonCacheConverter>(converter);
		}

		class PersonCacheConverter : IEntityEasyCacheConverter<Person, CachedPerson> {
			public Person ConvertFromCached(CachedPerson cached) {
				return new Person {
					Id = cached.Id,
					FirstName = cached.FirstName,
					LastName = cached.LastName,
					DateOfBirth = cached.DateOfBirth,
					Email = cached.Email,
					PhoneNumber = cached.PhoneNumber
				};
			}

			public CachedPerson ConvertToCached(Person entity) {
				return new CachedPerson {
					Id = entity.Id,
					FirstName = entity.FirstName,
					LastName = entity.LastName,
					DateOfBirth = entity.DateOfBirth,
					Email = entity.Email,
					PhoneNumber = entity.PhoneNumber
				};
			}
		}

		class CachedPerson : Person {

		}

		class NotCache {
		}
	}
}
