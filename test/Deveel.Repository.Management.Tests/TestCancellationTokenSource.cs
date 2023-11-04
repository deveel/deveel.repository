namespace Deveel {
	public class TestCancellationTokenSource : IOperationCancellationSource, IDisposable {
		private CancellationTokenSource? source;

		public TestCancellationTokenSource() {
			source = new CancellationTokenSource();
		}

		public CancellationToken Token => source?.Token ?? default;

		public void Dispose() {
			if (source != null) {
				source.Dispose();
				source = null;
			}
		}
	}
}
