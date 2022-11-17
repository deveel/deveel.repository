using System;

namespace Deveel.Repository {
	/// <summary>
	/// Represents an provider of repositories that
	/// are isolating the entities of a given tenant
	/// </summary>
	public interface IRepositoryProvider {
		/// <summary>
		/// Gets an instance of a repository of entities
		/// that is isolating the scope for a given tenant
		/// </summary>
		/// <param name="tenantId">The identifier of the tenant that
		/// owns the repository</param>
		/// <remarks>
		/// The provider does not validate the format of the <paramref name="tenantId"/>,
		/// that is used only to identify the tenant.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="IRepository"/> that
		/// isolates the entities for a given tenant.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the given <paramref name="tenantId"/> is <c>null</c>
		/// or an empty string.
		/// </exception>
		IRepository GetRepository(string tenantId);
	}
}
