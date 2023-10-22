using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Finbuckle.MultiTenant;
using MongoFramework;

namespace Deveel.Data {
	static class MongoDbContextUtil {
		public static IMongoDbContext CreateContext<TContext>(IMongoDbConnection connection, ITenantInfo tenantInfo)
			where TContext : class, IMongoDbContext {
			if (typeof(TContext) == typeof(MongoDbTenantContext))
				return new MongoDbTenantContext(connection, tenantInfo.Id);
			if (typeof(TContext) == typeof(MongoDbContext))
				return new MongoDbContext(connection);

			var ctors = typeof(TContext).GetConstructors();
			foreach (var ctor in ctors) {
				var parameters = ctor.GetParameters();
				if (parameters.Length == 2) {
					if (typeof(IMongoDbConnection<TContext>).IsAssignableFrom(parameters[0].ParameterType) &&
						typeof(ITenantInfo).IsAssignableFrom(parameters[1].ParameterType)) {
						return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>(), tenantInfo })!;
					} else if (typeof(IMongoDbConnection<TContext>).IsAssignableFrom(parameters[0].ParameterType) &&
						parameters[1].ParameterType == typeof(string)) {
						return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>(), tenantInfo.Id })!;
					} else if (typeof(IMongoDbConnection).IsAssignableFrom(parameters[0].ParameterType) &&
						typeof(ITenantInfo).IsAssignableFrom(parameters[0].ParameterType)) {
						return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { connection, tenantInfo })!;
					} else if (typeof(IMongoDbConnection).IsAssignableFrom(parameters[0].ParameterType) &&
						parameters[1].ParameterType == typeof(string)) {
						return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { connection, tenantInfo.Id! })!;
					}
				} else if (parameters.Length == 1) {
					if (typeof(IMongoDbConnection<TContext>).IsAssignableFrom(parameters[0].ParameterType)) {
						return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>() })!;
					} else if (typeof(IMongoDbConnection).IsAssignableFrom(parameters[0].ParameterType)) {
						return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { connection })!;
					}
				}
			}

			throw new NotSupportedException($"Cannot create '{typeof(TContext)}' MongoDB Context");
		}

		public static IMongoDbContext CreateContext<TContext>(MongoConnectionBuilder<TContext> builder, ITenantInfo tenantInfo)
			where TContext : class, IMongoDbContext
			=> CreateContext<TContext>(builder.Connection, tenantInfo);
	}
}
