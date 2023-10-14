using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	/// <summary>
	/// A default implementation of <see cref="IOperationErrorFactory"/>
	/// providing methods to create instances of <see cref="IOperationError"/>.
	/// </summary>
	public class OperationErrorFactory : IOperationErrorFactory {
		/// <summary>
		/// Normalizes the given error code to a standard format.
		/// </summary>
		/// <param name="errorCode">
		/// The error code to normalize.
		/// </param>
		/// <remarks>
		/// The aim of this method is to provide a way to map
		/// a given error code to a normalized format, so that
		/// multiple factories can be registered for the same
		/// application.
		/// </remarks>
		/// <returns>
		/// Returns an error code normalized to a standard format.
		/// </returns>
		protected virtual string ResolveErrorCode(string errorCode) {
			return errorCode;
		}

		/// <summary>
		/// Gets the error message for the given error code.
		/// </summary>
		/// <param name="errorCode">
		/// The error code for which to get the message.
		/// </param>
		/// <returns>
		/// Returns a string containing the error message for the given
		/// code or <c>null</c> if no message is available.
		/// </returns>
		protected virtual string? GetErrorMessage(string errorCode) {
			return null;
		}

		/// <inheritdoc/>
		public virtual IOperationError CreateError(string errorCode, string? message = null)
			=> new OperationError(ResolveErrorCode(errorCode), message == null ? GetErrorMessage(ResolveErrorCode(errorCode)) : message);

		/// <inheritdoc/>
		public virtual IOperationError CreateError(Exception exception) {
			string? errorCode = null, errorMessage = null;
			if (exception is OperationException opError) {
				errorCode = opError.ErrorCode;
				errorMessage = opError.Message;
			} else {
				errorCode = EntityErrorCodes.UnknownError;
				errorMessage = exception.Message;
			}

			return new OperationError(errorCode, errorMessage);
		}

		/// <inheritdoc/>
		public virtual IValidationError CreateValidationError(string errorCode, IList<ValidationResult> validationResults) {
			return new EntityValidationError(ResolveErrorCode(errorCode), validationResults);
		}
	}
}
