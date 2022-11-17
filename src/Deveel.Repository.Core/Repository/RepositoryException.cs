using System;
using System.Runtime.Serialization;

namespace Deveel.Repository {
	/// <summary>
	/// An exception that is thrown during the execution
	/// of an operation on the repository
	/// </summary>
	public class RepositoryException : Exception {
		public RepositoryException() {
		}

		public RepositoryException(string? message) : base(message) {
		}

		public RepositoryException(string? message, Exception? innerException) : base(message, innerException) {
		}
	}
}
