using System;

namespace Deveel.Data {
	/// <summary>
	/// Defines an entity that is owned by a tenant
	/// </summary>
	public interface ITenantDataEntity : IDataEntity {
		/// <summary>
		/// Gets the identifier of the tenant
		/// </summary>
		string TenantId { get; }
	}
}
