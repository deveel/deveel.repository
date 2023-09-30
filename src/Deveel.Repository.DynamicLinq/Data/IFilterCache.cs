namespace Deveel.Data {
	public interface IFilterCache {
		Delegate Get(string expression);

		void Set(string expression, Delegate lambda);
	}
}
