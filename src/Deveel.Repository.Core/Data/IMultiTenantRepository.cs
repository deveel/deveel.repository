using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a repository that is capable of segregating the
	/// data by the tenant that owns it.
	/// </summary>
    public interface IMultiTenantRepository<TEntity> : IRepository<TEntity> where TEntity : class {
		/// <summary>
		/// Gets the identifier of the tenant that owns the data
		/// </summary>
        string? TenantId { get; }
    }
}
