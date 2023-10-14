﻿namespace Deveel.Data {
	public sealed class PersonErrorFactory : OperationErrorFactory {
		protected override string ResolveErrorCode(string errorCode)
			=> errorCode switch {
				EntityErrorCodes.NotFound => PersonErrorCodes.NotFound,
				EntityErrorCodes.NotValid => PersonErrorCodes.NotValid,
				EntityErrorCodes.UnknownError => PersonErrorCodes.UnknownError,
				_ => base.ResolveErrorCode(errorCode)
			};

		protected override string? GetErrorMessage(string errorCode)
			=> errorCode switch {
				PersonErrorCodes.NotFound => "The person was not found",
				PersonErrorCodes.NotValid => "The person is not valid",
				PersonErrorCodes.UnknownError => "An unknown error occurred",
				_ => base.GetErrorMessage(errorCode)
			};
	}
}
