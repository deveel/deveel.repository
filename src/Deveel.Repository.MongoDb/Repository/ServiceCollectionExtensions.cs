using System;
using System.Linq.Expressions;

using Deveel.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using static System.Collections.Specialized.BitVector32;

namespace Deveel.Data {
	public static class ServiceCollectionExtensions {

		#region AddMongoDbStore<T>

		public static IServiceCollection AddMongoDbStore<TStore, TEntity>(this IServiceCollection services)
			where TEntity : class, IEntity
			where TStore : MongoRepository<TEntity>
			=> services.AddStore<TStore>();

		public static IServiceCollection AddMongoDbStore<TEntity>(this IServiceCollection services)
			where TEntity : class, IEntity
			=> services.AddMongoDbStore<MongoRepository<TEntity>, TEntity>();

		public static IServiceCollection AddMongoDbStore<TStore, TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
			where TEntity : class, IEntity
			where TStore : MongoRepository<TEntity>
			=> services.AddMongoStoreOptions<TEntity>(configure).AddMongoDbStore<TStore, TEntity>();

		public static IServiceCollection AddMongoDbStore<TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
			where TEntity : class, IEntity
			=> services.AddMongoDbStore<MongoRepository<TEntity>, TEntity>(configure);

		public static IServiceCollection AddMongoDbStore<TStore, TEntity>(this IServiceCollection services, string sectionName)
			where TEntity : class, IEntity
			where TStore : MongoRepository<TEntity>
			=> services.AddMongoStoreOptions<TEntity>(sectionName).AddMongoDbStore<TStore, TEntity>();

		public static IServiceCollection AddMongoDbStore<TEntity>(this IServiceCollection services, string sectionName)
			where TEntity : class, IEntity
			=> services.AddMongoDbStore<MongoRepository<TEntity>, TEntity>(sectionName);


		#endregion

		#region AddMongoDbFacadeStore<TEntity,TFacade>

		public static IServiceCollection AddMongoDbFacadeStore<TStore, TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IEntity
			where TFacade : class, IEntity
			where TStore : MongoRepository<TEntity, TFacade>
			=> services.AddStore<TStore, TEntity>().AddStore<TStore, TFacade>();

		public static IServiceCollection AddMongoDbFacadeStore<TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IEntity
			where TFacade : class, IEntity
			=> services.AddMongoDbFacadeStore<MongoRepository<TEntity, TFacade>, TEntity, TFacade>();


		#endregion

		#region AddMongoDbStoreProvider<TEntity>

		public static IServiceCollection AddMongoDbStoreProvider<TProvider, TEntity>(this IServiceCollection services)
			where TEntity : class, IEntity
			where TProvider : MongoRepositoryProvider<TEntity>
			=> services.AddStoreProvider<TProvider, TEntity>();

		public static IServiceCollection AddMongoDbStoreProvider<TEntity>(this IServiceCollection services)
			where TEntity : class, IEntity
			=> services.AddMongoDbStoreProvider<MongoRepositoryProvider<TEntity>, TEntity>();

		public static IServiceCollection AddMongoDbStoreProvider<TProvider, TEntity>(this IServiceCollection services, string sectionName)
			where TEntity : class, IEntity
			where TProvider : MongoRepositoryProvider<TEntity>
			=> services.AddMongoStoreOptions<TEntity>(sectionName).AddMongoDbStoreProvider<TProvider, TEntity>();

		public static IServiceCollection AddMongoDbStoreProvider<TEntity>(this IServiceCollection services, IConfiguration configuration, string sectionName = null, string connectionStringName = null)
			where TEntity : class, IEntity
			=> services.AddMongoDbStoreProvider<TEntity>(configuration, sectionName, connectionStringName);

		public static IServiceCollection AddMongoDbStoreProvider<TProvider, TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
			where TEntity : class, IEntity
			where TProvider : MongoRepositoryProvider<TEntity>
			=> services.AddMongoStoreOptions<TEntity>(configure).AddMongoDbStoreProvider<TProvider, TEntity>();

		public static IServiceCollection AddMongoDbStoreProvider<TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
			where TEntity : class, IEntity
			=> services.AddMongoDbStoreProvider<MongoRepositoryProvider<TEntity>, TEntity>(configure);

		#endregion

		#region AddMongoDbFacadeStoreProvider<TEntity, TFacade>

		public static IServiceCollection AddMongoDbFacadeStoreProvider<TProvider, TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IEntity
			where TFacade : class, IEntity
			where TProvider : MongoRepositoryProvider<TEntity, TFacade>
			=> services.AddStoreProvider<TProvider, TEntity>().AddStoreProvider<TProvider, TFacade>();

		public static IServiceCollection AddMongoDbFacadeStoreProvider<TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IEntity
			where TFacade : class, IEntity
			=> services.AddMongoDbFacadeStoreProvider<MongoRepositoryProvider<TEntity, TFacade>, TEntity, TFacade>();

		#endregion

		#region AddFieldMapper

		public static IServiceCollection AddDocumentFieldMapper<TDocument, TMapper>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TDocument : class, IEntity
			where TMapper : class, IDocumentFieldMapper<TDocument> {
			services.Add(new ServiceDescriptor(typeof(IDocumentFieldMapper<TDocument>), typeof(TMapper), lifetime));
			services.Add(new ServiceDescriptor(typeof(TMapper), typeof(TMapper), lifetime));

			return services;
		}

		public static IServiceCollection AddDocumentFieldMapper<TDocument, TMapper>(this IServiceCollection services, TMapper mapper)
			where TDocument : class, IEntity
			where TMapper : class, IDocumentFieldMapper<TDocument> {
			services.Add(new ServiceDescriptor(typeof(IDocumentFieldMapper<TDocument>),mapper));
			services.Add(new ServiceDescriptor(typeof(TMapper), mapper));

			return services;
		}

		public static IServiceCollection AddDocumentFieldMapper<TDocument>(this IServiceCollection services, Func<string, string> fieldMapper)
			where TDocument : class, IEntity
			=> services.AddDocumentFieldMapper<TDocument, DelegatedDocumentFieldMapper<TDocument>>(new DelegatedDocumentFieldMapper<TDocument>(fieldMapper));

		#endregion
	}
}
