using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Deveel.Data {
	public class SqlTestConnection : IDisposable {
		public SqlTestConnection() {
			Connection = new SqliteConnection("DataSource=:memory:");
			if (Connection.State != System.Data.ConnectionState.Open)
				Connection.Open();

			Connection.EnableExtensions();
			SpatialiteLoader.Load(Connection);
		}

		public SqliteConnection Connection { get; }

		public void Dispose() {
			Connection?.Close();
			Connection?.Dispose();
		}
	}
}
