using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Deveel.Data {
	public class SqlTestConnection : IDisposable {
		public SqlTestConnection() : this("deveel-test") {
		}

		protected SqlTestConnection(string databaseName) {
			Connection = new SqliteConnection($"Data Source={databaseName};Mode=Memory;Cache=Shared");
			if (Connection.State != System.Data.ConnectionState.Open)
				Connection.Open();

			Connection.EnableExtensions();
			SpatialiteLoader.Load(Connection);
		}

		public SqliteConnection Connection { get; }

		public void Dispose() {
			if (Connection.State != System.Data.ConnectionState.Closed)
				Connection?.Close();

			Connection?.Dispose();
		}
	}
}
