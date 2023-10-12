using Deveel.Data.Caching;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class EntityManagerCachingTests : EntityManagerTestSuite {
		public EntityManagerCachingTests(ITestOutputHelper testOutput) : base(testOutput) {
		}

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddEasyCaching(options => {
				options.UseInMemory("default");
			});

			services.AddEntityEasyCacheFor<Person>(options => {
				options.Expiration = TimeSpan.FromMinutes(15);
			});

			services.AddEntityCacheKeyGenerator<PersonCacheKeyGenerator>();

			base.ConfigureServices(services);
		}
	}
}
