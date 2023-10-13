using Deveel.Data.Caching;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class PersonManagerCachingTests : EntityManagerTestSuite<PersonManager> {
		public PersonManagerCachingTests(ITestOutputHelper testOutput) : base(testOutput) {
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

		[Fact]
		public async Task FindByEmail() {
			var person = People.Random(x => x.Email != null);

			Assert.NotNull(person);
			Assert.NotNull(person.Email);

			var found = await Manager.FindByEmailAsync(person.Email);

			Assert.NotNull(found);
			Assert.Equal(person.Id, found.Id);
			Assert.Equal(person.Email, found.Email);
		}
	}
}
