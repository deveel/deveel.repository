using Deveel.Data.Caching;

namespace Deveel.Data;

[Category("Integration")]
public class EntityManagerCachingTests : EntityManagerTests
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddEasyCaching(options => options.UseInMemory("default"));
        services.AddEntityEasyCacheFor<Person>(options => {
            options.Expiration = TimeSpan.FromMinutes(15);
        });
        services.AddEntityCacheKeyGenerator<PersonCacheKeyGenerator>();
        base.ConfigureServices(services);
    }
}

