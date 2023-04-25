using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

namespace Deveel.Data {
    public sealed class MongoDbContextBuilder<TContext> where TContext : class, IMongoDbContext {
        private readonly ServiceLifetime defaultLifetime;

        public MongoDbContextBuilder(IServiceCollection services, ServiceLifetime defaultLifetime = ServiceLifetime.Singleton) {
            Services = services ?? throw new ArgumentNullException(nameof(services));
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
            Services.TryAddSingleton<MongoDbTenantConnection, MongoDbTenantConnection>();
            Services.TryAddSingleton<IMongoDbTenantConnection, MongoDbTenantConnection>();
        }

        private void RegisterTenantContext() {
            if (typeof(TContext) == typeof(MongoDbTenantContext)) {
                var factory = (IServiceProvider provider) => {
                    var connection = provider.GetRequiredService<IMongoDbTenantConnection>();
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

            Services.AddSingleton<IMongoDbConnection, TConnection>();
            Services.AddSingleton<MongoDbConnection, TConnection>();

            if (typeof(TConnection) != typeof(IMongoDbConnection))
                Services.AddSingleton<TConnection, TConnection>();

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

            Services.AddSingleton<MongoDbTenantConnection, MongoDbTenantConnection<TTenantInfo>>();
            Services.AddSingleton<IMongoDbTenantConnection, MongoDbTenantConnection<TTenantInfo>>();

            UseConnection<MongoDbTenantConnection<TTenantInfo>>();

            return this;
        }

        public MongoDbContextBuilder<TContext> UseTenantConnection() {

            Services.AddSingleton<MongoDbTenantConnection>();
            Services.AddSingleton<IMongoDbTenantConnection, MongoDbTenantConnection>();

            return UseConnection<MongoDbTenantConnection>();
        }

        public MongoRepositoryBuilder<TEntity> AddRepository<TEntity>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TEntity : class
            => new MongoRepositoryBuilder<TEntity>(Services, lifetime);
    }
}
