using System;

namespace Deveel.Repository {
	/// <summary>
	/// A marker interface that provides a contract to define
	/// specifications of a repository when creating it and seed it
	/// </summary>
	/// <remarks>
	/// This contract is empty and it is used only to force specific
	/// implementations of repositories to recognize it.
	/// </remarks>
	public interface IRepositoryDefinition {
	}
}
