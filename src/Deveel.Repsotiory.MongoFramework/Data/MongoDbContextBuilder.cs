using CommunityToolkit.Diagnostics;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

namespace Deveel.Data {
    public sealed class MongoDbContextBuilder<TContext> where TContext : class, IMongoDbContext {
        private readonly ServiceLifetime defaultLifetime;

        public MongoDbContextBuilder(IServiceCollection services, ServiceLifetime defaultLifetime = ServiceLifetime.Singleton) {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            if (typeof(IMultiTenantContext).IsAssignableFrom(typeof(TContext)) && defaultLifetime == ServiceLifetime.Singleton)
				throw new ArgumentException("Multi-tenant context can only be scoped or transient", nameof(defaultLifetime));

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
            Services.TryAdd(new ServiceDescriptor(typeof(MongoDbTenantConnection<TContext>), typeof(MongoDbTenantConnection<TContext>), defaultLifetime));
            Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbTenantConnection<TContext>), typeof(MongoDbTenantConnection<TContext>), defaultLifetime));
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

            Services.Add(new ServiceDescriptor(typeof(IMongoDbConnection<TContext>), typeof(TConnection), defaultLifetime));
            Services.Add(new ServiceDescriptor(typeof(MongoDbConnection<TContext>), typeof(TConnection), defaultLifetime));

            if (typeof(TConnection) != typeof(MongoDbConnection<TContext>))
                Services.Add(new ServiceDescriptor(typeof(TConnection), typeof(TConnection), defaultLifetime));

            Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection), typeof(TConnection), defaultLifetime));

            return this;
        }

        public MongoDbContextBuilder<TContext> UseConnection(string connectionString) {
            ThrowIfMultiTenant();

            Guard.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));

            var factory = (IServiceProvider provider) =>
                MongoDbConnection<TContext>.FromConnectionString(connectionString);

            Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection), factory, defaultLifetime));
            Services.Add(new ServiceDescriptor(typeof(IMongoDbConnection<TContext>), factory, defaultLifetime));
            Services.Add(new ServiceDescriptor(typeof(MongoDbConnection<TContext>), factory, defaultLifetime));
            // Services.Add(new ServiceDescriptor(typeof(IMongoDbConnection<TContext>), _ => MongoDbConnection<TContext>.FromConnectionString(connectionString), defaultLifetime));

            return this;
        }


        public MongoDbContextBuilder<TContext> UseTenantConnection<TTenantInfo>()
            where TTenantInfo : class, ITenantInfo, new() {

            Services.Add(new ServiceDescriptor(typeof(MongoDbTenantConnection<TContext, TTenantInfo>), typeof(MongoDbTenantConnection<TContext, TTenantInfo>), defaultLifetime));
            Services.Add(new ServiceDescriptor(typeof(IMongoDbTenantConnection<TContext>), typeof(MongoDbTenantConnection<TContext, TTenantInfo>), defaultLifetime));

            Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection), typeof(MongoDbTenantConnection<TContext, TTenantInfo>), defaultLifetime));

            // UseConnection<MongoDbTenantConnection<TContext, TTenantInfo>>();

            return this;
        }

        public MongoDbContextBuilder<TContext> UseTenantConnection() {

            Services.Add(new ServiceDescriptor(typeof(MongoDbTenantConnection<TContext>), typeof(MongoDbTenantConnection<TContext>), defaultLifetime));
            Services.Add(new ServiceDescriptor(typeof(IMongoDbTenantConnection<TContext>), typeof(MongoDbTenantConnection<TContext>), defaultLifetime));

            Services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection), typeof(MongoDbTenantConnection<TContext>), defaultLifetime));

            // UseConnection<MongoDbTenantConnection<TContext>>();

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> AddRepository<TEntity>(ServiceLifetime? lifetime = null)
            where TEntity : class
            => new MongoRepositoryBuilder<TContext, TEntity>(Services, lifetime ?? defaultLifetime);
	}
}
