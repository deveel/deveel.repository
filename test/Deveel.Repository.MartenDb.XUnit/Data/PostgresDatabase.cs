using Testcontainers.PostgreSql;

namespace Deveel.Data {
	public class PostgresDatabase : IAsyncLifetime {
		private PostgreSqlContainer _container;

		public PostgresDatabase() {
			_container = new PostgreSqlBuilder()
				.WithDatabase("test")
				.Build();
		}

		public string ConnectionString => _container.GetConnectionString();

		public async Task InitializeAsync() {
			await _container.StartAsync();
		}

		public async Task DisposeAsync() {
			if (_container != null)
				await _container.DisposeAsync();
		}
	}
}
