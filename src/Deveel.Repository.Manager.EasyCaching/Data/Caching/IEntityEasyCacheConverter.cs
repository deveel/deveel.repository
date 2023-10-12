namespace Deveel.Data.Caching {
	public interface IEntityEasyCacheConverter<TEntity, TCached> {
		TCached ConvertToCached(TEntity entity);

		TEntity ConvertFromCached(TCached cached);
	}
}
