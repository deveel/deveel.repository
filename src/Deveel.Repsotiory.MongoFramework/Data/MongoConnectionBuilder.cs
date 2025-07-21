using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

using System.Reflection;

namespace Deveel.Data
{
	public sealed class MongoConnectionBuilder<TContext>
		where TContext : class, IMongoDbContext
	{
		private readonly IServiceCollection services;
		private readonly ServiceLifetime lifetime;

		internal MongoConnectionBuilder(IServiceCollection services, ServiceLifetime lifetime)
		{
			this.services = services;
			this.lifetime = lifetime;
		}

		public MongoConnectionBuilder<TContext> UseConnection(string connectionString)
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

			var connectionType = typeof(IMongoDbConnection<>).MakeGenericType(typeof(TContext));
			
			services.TryAdd(ServiceDescriptor.Describe(connectionType, sp =>
			{
				var implementationType = typeof(MongoDbConnection<>).MakeGenericType(typeof(TContext));
				var ctor = implementationType.GetConstructor(new[] { typeof(string) });
				if (ctor == null)
					throw new InvalidOperationException($"No suitable constructor found for {implementationType.FullName} that accepts IMongoDbConnection.");

				return ctor.Invoke(new object[] { connectionString });
			}, lifetime));

			services.TryAdd(ServiceDescriptor.Describe(typeof(IMongoDbConnection), sp => (IMongoDbConnection) sp.GetRequiredService(connectionType), lifetime));

			return this;
		}

		public MongoConnectionBuilder<TContext> UseTenantConnection(string? defaultConnection = null)
		{
			services.AddOptions<MongoTenantConnectionOptions>()
				.Configure(options => options.DefaultConnectionString = defaultConnection);

			var connectionType = typeof(IMongoDbConnection<>).MakeGenericType(typeof(TContext));
			services.TryAdd(ServiceDescriptor.Describe(connectionType, sp =>
			{
				var implementationType = typeof(MongoDbTenantConnection<>).MakeGenericType(typeof(TContext));
				return ActivatorUtilities.CreateInstance(sp, implementationType);
			},lifetime));

			services.TryAdd(ServiceDescriptor.Describe(typeof(IMongoDbConnection), sp =>
			{
				var connection = sp.GetRequiredService(connectionType);
				return (IMongoDbConnection)connection;
			}, lifetime));

			return this;
		}
	}
}
