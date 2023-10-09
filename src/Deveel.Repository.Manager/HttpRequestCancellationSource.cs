using Microsoft.AspNetCore.Http;

namespace Deveel {
	/// <summary>
	/// An implementation of <see cref="IOperationCancellationSource"/> that
	/// uses the <see cref="HttpContext.RequestAborted"/> token.
	/// </summary>
	/// <remarks>
	/// This implementation requires the <see cref="IHttpContextAccessor"/> to
	/// be registered in the context of the application.
	/// </remarks>
	public sealed class HttpRequestCancellationSource : IOperationCancellationSource {
		private readonly IHttpContextAccessor httpContextAccessor;

		/// <summary>
		/// Constructs the cancellation source using the given <see cref="IHttpContextAccessor"/>.
		/// </summary>
		/// <param name="httpContextAccessor"></param>
		public HttpRequestCancellationSource(IHttpContextAccessor httpContextAccessor) {
			this.httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Gets the cancellation token from the current HTTP context.
		/// </summary>
		public CancellationToken Token => httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
	}
}
