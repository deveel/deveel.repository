﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	[Obsolete("This class is obsolete: please use the Deveel.Repository.MongoFramework instead")]
	public static class ServiceCollectionExtensions {

        #region AddMongoRepository<T>

        public static IServiceCollection AddMongoRepository<TRepository, TEntity>(this IServiceCollection services)
            where TEntity : class
            where TRepository : MongoRepository<TEntity> {
            services.AddRepository<TRepository>(ServiceLifetime.Singleton)
                .AddSingleton<IRepository, TRepository>()
                .AddSingleton<IRepository<TEntity>, TRepository>();

            if (typeof(TRepository) != typeof(MongoRepository<TEntity>))
                services.AddSingleton<MongoRepository<TEntity>, TRepository>();

            return services;
        }

        public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services)
            where TEntity : class
            => services.AddMongoRepository<MongoRepository<TEntity>, TEntity>();

        public static IServiceCollection AddMongoRepository<TRepository, TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
            where TEntity : class
            where TRepository : MongoRepository<TEntity>
            => services.AddMongoStoreOptions<TEntity>(configure).AddMongoRepository<TRepository, TEntity>();

        public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
            where TEntity : class
            => services.AddMongoRepository<MongoRepository<TEntity>, TEntity>(configure);

        public static IServiceCollection AddMongoRepository<TRepository, TEntity>(this IServiceCollection services, string sectionName)
            where TEntity : class
            where TRepository : MongoRepository<TEntity>
            => services.AddMongoStoreOptions<TEntity>(sectionName).AddMongoRepository<TRepository, TEntity>();

        public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services, string sectionName)
            where TEntity : class
            => services.AddMongoRepository<MongoRepository<TEntity>, TEntity>(sectionName);


        #endregion

        #region AddMongoFacadeRepository<TEntity,TFacade>

        public static IServiceCollection AddMongoFacadeRepository<TRepository, TEntity, TFacade>(this IServiceCollection services)
            where TEntity : class, TFacade
            where TFacade : class
            where TRepository : MongoRepository<TEntity, TFacade>
            => services
                .AddRepository<TRepository, TEntity>(ServiceLifetime.Singleton)
                .AddRepository<TRepository, TFacade>(ServiceLifetime.Singleton)
                .AddSingleton<IRepository, TRepository>()
                .AddSingleton<IRepository<TEntity>, TRepository>()
                .AddSingleton<IRepository<TFacade>, TRepository>();

        public static IServiceCollection AddMongoFacadeRepository<TEntity, TFacade>(this IServiceCollection services)
            where TEntity : class, TFacade
            where TFacade : class
            => services
				.AddMongoFacadeRepository<MongoRepository<TEntity, TFacade>, TEntity, TFacade>();


		#endregion

		#region AddMongoRepositoryProvider<TEntity>

		public static IServiceCollection AddMongoRepositoryProvider<TProvider, TEntity>(this IServiceCollection services)
			where TEntity : class
			where TProvider : MongoRepositoryProvider<TEntity>
			=> services	.AddRepositoryProvider<TProvider, TEntity>(ServiceLifetime.Singleton);

        public static IServiceCollection AddMongoRepositoryProvider<TEntity>(this IServiceCollection services)
            where TEntity : class
            => services.AddMongoRepositoryProvider<MongoRepositoryProvider<TEntity>, TEntity>();

        public static IServiceCollection AddMongoRepositoryProvider<TProvider, TEntity>(this IServiceCollection services, string sectionName)
            where TEntity : class
            where TProvider : MongoRepositoryProvider<TEntity>
            => services.AddMongoStoreOptions<TEntity>(sectionName).AddMongoRepositoryProvider<TProvider, TEntity>();

        public static IServiceCollection AddMongoRepositoryProvider<TEntity>(this IServiceCollection services, IConfiguration configuration, string sectionName = null, string connectionStringName = null)
            where TEntity : class
            => services.AddMongoRepositoryProvider<TEntity>(configuration, sectionName, connectionStringName);

        public static IServiceCollection AddMongoRepositoryProvider<TProvider, TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
            where TEntity : class
            where TProvider : MongoRepositoryProvider<TEntity>
            => services.AddMongoStoreOptions<TEntity>(configure).AddMongoRepositoryProvider<TProvider, TEntity>();

        public static IServiceCollection AddMongoRepositoryProvider<TEntity>(this IServiceCollection services, Action<MongoDbStoreOptions> configure)
            where TEntity : class
            => services.AddMongoRepositoryProvider<MongoRepositoryProvider<TEntity>, TEntity>(configure);

        #endregion

        #region AddMongoFacadeRepositoryProvider<TEntity, TFacade>

        public static IServiceCollection AddMongoFacadeRepositoryProvider<TProvider, TEntity, TFacade>(this IServiceCollection services)
            where TEntity : class, TFacade
            where TFacade : class
            where TProvider : MongoRepositoryProvider<TEntity, TFacade>
            => services
                .AddRepositoryProvider<TProvider, TEntity>()
                .AddRepositoryProvider<TProvider, TFacade>()
                .AddSingleton<IRepositoryProvider, TProvider>()
                .AddSingleton<IRepositoryProvider<TEntity>, TProvider>()
                .AddSingleton<IRepositoryProvider<TFacade>, TProvider>();

        public static IServiceCollection AddMongoFacadeRepositoryProvider<TEntity, TFacade>(this IServiceCollection services)
            where TEntity : class, TFacade
            where TFacade : class
            => services.AddMongoFacadeRepositoryProvider<MongoRepositoryProvider<TEntity, TFacade>, TEntity, TFacade>();

        #endregion

        #region AddFieldMapper

        public static IServiceCollection AddDocumentFieldMapper<TDocument, TMapper>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TDocument : class
            where TMapper : class, IDocumentFieldMapper<TDocument> {
            services.Add(new ServiceDescriptor(typeof(IDocumentFieldMapper<TDocument>), typeof(TMapper), lifetime));
            services.Add(new ServiceDescriptor(typeof(TMapper), typeof(TMapper), lifetime));

            return services;
        }

        public static IServiceCollection AddDocumentFieldMapper<TDocument, TMapper>(this IServiceCollection services, TMapper mapper)
            where TDocument : class
            where TMapper : class, IDocumentFieldMapper<TDocument> {
            services.Add(new ServiceDescriptor(typeof(IDocumentFieldMapper<TDocument>), mapper));
            services.Add(new ServiceDescriptor(typeof(TMapper), mapper));

            return services;
        }

        public static IServiceCollection AddDocumentFieldMapper<TDocument>(this IServiceCollection services, Func<string, string> fieldMapper)
            where TDocument : class
            => services.AddDocumentFieldMapper<TDocument, DelegatedDocumentFieldMapper<TDocument>>(new DelegatedDocumentFieldMapper<TDocument>(fieldMapper));

		#endregion

		#region AddMongoTransactionFactory

        public static IServiceCollection AddMongoTransactionFactory(this IServiceCollection services) {
            return services
                .AddSingleton<MongoSessionProvider>()
                .AddSingleton<IDataTransactionFactory, MongoTransactionFactory>();
        }

		#endregion
	}
}