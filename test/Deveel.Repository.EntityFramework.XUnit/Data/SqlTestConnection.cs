using Microsoft.Data.Sqlite;

namespace Deveel.Data {
	public class SqlTestConnection : IDisposable {
		public SqlTestConnection() {
			Connection = new SqliteConnection("DataSource=:memory:");
			if (Connection.State != System.Data.ConnectionState.Open)
				Connection.Open();
		}

		public SqliteConnection Connection { get; }

		public void Dispose() {
			Connection?.Close();
			Connection?.Dispose();
		}
	}
}
