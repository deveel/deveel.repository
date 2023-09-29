namespace Deveel.Data {
	public class TestTime : ISystemTime {
		public TestTime() {
			UtcNow = DateTimeOffset.UtcNow;
			Now = DateTimeOffset.Now;
		}

		public DateTimeOffset UtcNow { get; }

		public DateTimeOffset Now { get; }
	}
}