// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Finbuckle.MultiTenant;

using MongoFramework;

#if NET7_0_OR_GREATER
using ITenantInfo = Finbuckle.MultiTenant.TenantInfo;
#endif

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
