namespace Deveel.Data {
	/// <summary>
	/// A service that creates instances of <see cref="IOperationError"/>
	/// that can be used to report errors in an operation
	/// for a specific entity.
	/// </summary>
	public interface IOperationErrorFactory<TEntity> : IOperationErrorFactory where TEntity : class {
	}
}
