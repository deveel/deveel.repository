using EasyCaching.Core;

using Microsoft.Extensions.Options;

namespace Deveel.Data.Caching {
	public class EntityEasyCache<TEntity> : EntityEasyCache<TEntity, TEntity> where TEntity : class {
		public EntityEasyCache(
			IEasyCachingProvider cacheProvider, 
			IOptions<EntityCacheOptions<TEntity>>? options = null, 
			IEntityCacheKeyGenerator<TEntity>? keyGenerator = null, 
			IEntityEasyCacheConverter<TEntity, TEntity>? converter = null) 
			: base(cacheProvider, options, keyGenerator, converter) {
		}
	}
}
