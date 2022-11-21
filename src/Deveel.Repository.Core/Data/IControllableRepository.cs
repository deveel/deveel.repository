using System;

namespace Deveel.Data {
	public interface IControllableRepository : IRepository {
		Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

		Task CreateAsync(CancellationToken cancellationToken = default);

		Task DropAsync(CancellationToken cancellationToken = default);
	}
}
