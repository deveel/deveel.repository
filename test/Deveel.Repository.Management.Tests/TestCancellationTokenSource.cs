using Xunit;

namespace Deveel {
	public class TestCancellationTokenSource : IOperationCancellationSource
    {
        public CancellationToken Token => TestContext.Current.CancellationToken;
    }
}
