namespace Deveel {
	public static class EnumerableExtensions {
		public static TEntity? Random<TEntity>(this IEnumerable<TEntity> source, Func<TEntity, bool>? predicate = null, int maxRetries = 100) {
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			var count = source.Count();
			if (count == 0)
				return default;

			var retries = 0;

			while(true) {
				var index = new Random().Next(0, count - 1);
				var entity = source.ElementAt(index);

				if (predicate == null || predicate(entity))
					return entity;

				if (retries++ > maxRetries)
					return default;
			}
		}
	}
}
