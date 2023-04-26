using System;
using System.Runtime.Serialization;

namespace Deveel.Data {
	/// <summary>
	/// An exception that is thrown during the execution
	/// of an operation on the repository
	/// </summary>
	public class RepositoryException : Exception {
		/// <summary>
		/// Constructs an empty instance of the <see cref="RepositoryException"/> class.
		/// </summary>
		public RepositoryException() {
		}

		/// <summary>
		/// Constructs an instance of the <see cref="RepositoryException"/> class
		/// </summary>
		/// <param name="message">
		/// The message that describes the error.
		/// </param>
		public RepositoryException(string? message) : base(message) {
		}

		/// <summary>
		/// Constructs an instance of the <see cref="RepositoryException"/> class
		/// </summary>
		/// <param name="message">
		/// The message that describes the error.
		/// </param>
		/// <param name="innerException">
		/// An exception that is the cause of the current exception.
		/// </param>
		public RepositoryException(string? message, Exception? innerException) : base(message, innerException) {
		}
	}
}
