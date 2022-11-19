using System;

namespace Deveel.Data {
	public static class RepositoryExtensions {
		public static void AddState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, StateInfo<TStatus> stateInfo)
			where TEntity : class, IEntity
			=> repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

		public static void AddState<TStatus>(this IStateRepository<TStatus> repository, IEntity entity, StateInfo<TStatus> stateInfo)
			=> repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

		public static void RemoveState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, StateInfo<TStatus> stateInfo)
			where TEntity : class, IEntity
			=> repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

		public static void RemoveState<TStatus>(this IStateRepository<TStatus> repository, IEntity entity, StateInfo<TStatus> stateInfo)
			=> repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();
	}
}
