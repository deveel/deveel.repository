namespace Deveel.Data {
	public static class RepositoryExtensions {
		public static Task<TAggregate?> FindByKeyAsync<TAggregate>(this IRepository<TAggregate> repository, object key, int? version = null)
			where TAggregate : Aggregate
			=> repository.FindByKeyAsync(new AggregateKey(key, version));
	}
}
