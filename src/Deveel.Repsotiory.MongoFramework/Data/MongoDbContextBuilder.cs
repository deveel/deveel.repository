using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

namespace Deveel.Data {
    public sealed class MongoDbContextBuilder<TContext> where TContext : class, IMongoDbContext {
        private readonly ServiceLifetime defaultLifetime;

        public MongoDbContextBuilder(IServiceCollection services, ServiceLifetime defaultLifetime = ServiceLifetime.Singleton) {
            Services = services ?? throw new ArgumentNullException(nameof(services));

			if (typeof(IMultiTenantContext).IsAssignableFrom(typeof(TContext)) &&
				defaultLifetime == ServiceLifetime.Singleton) {
				throw new ArgumentException(nameof(defaultLifetime), "Multi-tenant context can only be scoped or transient");
			}

            this.defaultLifetime = defaultLifetime;

            RegisterContext();
            
        }

        public IServiceCollection Services { get; }

        private void RegisterContext() {
            if (typeof(IMongoDbTenantContext).IsAssignableFrom(typeof(TContext))) {
                RegisterTenantConnection();
                RegisterTenantContext();
            } else {
                Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbContext), typeof(TContext), defaultLifetime));

                if (typeof(MongoDbContext).IsAssignableFrom(typeof(TContext)))
                    Services.TryAdd(new ServiceDescriptor(typeof(MongoDbContext), typeof(TContext), defaultLifetime));

                if (typeof(TContext) != typeof(MongoDbContext))
                    Services.TryAdd(new ServiceDescriptor(typeof(TContext), typeof(TContext), defaultLifetime));
            }
        }

        private void RegisterTenantConnection() {
            Services.TryAddScoped<MongoDbTenantConnection<TContext>, MongoDbTenantConnection<TContext>>();
            Services.TryAddScoped<IMongoDbTenantConnection<TContext>, MongoDbTenantConnection<TContext>>();
        }

        private void RegisterTenantContext() {
            if (typeof(TContext) == typeof(MongoDbTenantContext)) {
                var factory = (IServiceProvider provider) => {
                    var connection = provider.GetRequiredService<IMongoDbTenantConnection<TContext>>();
                    return new MongoDbTenantContext(connection, connection.TenantInfo.Id);
                };

                Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbContext), factory, defaultLifetime));
                Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbTenantContext), factory, defaultLifetime));
                Services.TryAdd(new ServiceDescriptor(typeof(MongoDbContext), factory, defaultLifetime));
                Services.TryAdd(new ServiceDescriptor(typeof(TContext), factory, defaultLifetime));
            } else {
                Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbContext), typeof(TContext), defaultLifetime));
                Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbTenantContext), typeof(TContext), defaultLifetime));

                if (typeof(MongoDbContext).IsAssignableFrom(typeof(TContext)))
                    Services.TryAdd(new ServiceDescriptor(typeof(MongoDbContext), typeof(TContext), defaultLifetime));

                Services.TryAdd(new ServiceDescriptor(typeof(TContext), typeof(TContext), defaultLifetime));
            }
        }

        private void ThrowIfMultiTenant() {
            if (typeof(IMultiTenantContext).IsAssignableFrom(typeof(TContext)))
                throw new InvalidOperationException($"MongoDB Context of type '{typeof(TContext)}' is multi-tenant");
        }


        public MongoDbContextBuilder<TContext> UseConnection<TConnection>()
            where TConnection : MongoDbConnection {

            Services.Add(new ServiceDescriptor(typeof(IMongoDbConnection), typeof(TConnection), defaultLifetime));
            Services.Add(new ServiceDescriptor(typeof(MongoDbConnection), typeof(TConnection), defaultLifetime));

            if (typeof(TConnection) != typeof(IMongoDbConnection))
                Services.Add(new ServiceDescriptor(typeof(TConnection), typeof(TConnection), defaultLifetime));

            return this;
        }

        public MongoDbContextBuilder<TContext> UseConnection(string connectionString) {
            ThrowIfMultiTenant();

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace.", nameof(connectionString));

            var factory = (IServiceProvider provider) =>
                MongoDbConnection.FromConnectionString(connectionString);

            Services.AddSingleton<IMongoDbConnection>(factory);
            Services.AddSingleton<MongoDbConnection>(factory);

            return this;
        }


        public MongoDbContextBuilder<TContext> UseTenantConnection<TTenantInfo>()
            where TTenantInfo : class, ITenantInfo, new() {

            Services.AddScoped<MongoDbTenantConnection<TContext, TTenantInfo>, MongoDbTenantConnection<TContext, TTenantInfo>>();
            Services.AddScoped<IMongoDbTenantConnection<TContext>, MongoDbTenantConnection<TContext, TTenantInfo>>();

            UseConnection<MongoDbTenantConnection<TContext, TTenantInfo>>();

            return this;
        }

        public MongoDbContextBuilder<TContext> UseTenantConnection() {

            Services.AddScoped<MongoDbTenantConnection<TContext>>();
            Services.AddScoped<IMongoDbTenantConnection<TContext>, MongoDbTenantConnection<TContext>>();

            return UseConnection<MongoDbTenantConnection<TContext>>();
        }

        public MongoRepositoryBuilder<TEntity> AddRepository<TEntity>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TEntity : class
            => new MongoRepositoryBuilder<TEntity>(Services, lifetime);
    }
}
