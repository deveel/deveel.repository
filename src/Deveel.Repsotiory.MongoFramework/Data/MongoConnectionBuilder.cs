using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class MongoConnectionBuilder {
		private MongoUrl? mongoUrl;
		private Action<MongoClientSettings>? settings;

		public MongoConnectionBuilder() {
		}

		public virtual IMongoDbConnection Connection {
			get {
				if (mongoUrl == null)
					throw new InvalidOperationException("No connection string or URL was specified");

				return MongoDbConnection.FromUrl(mongoUrl, settings);
			}
		}

		public MongoConnectionBuilder UseConnection(string connectionString) {
			ArgumentNullException.ThrowIfNull(connectionString);

			mongoUrl = MongoUrl.Create(connectionString);
			return this;
		}

		public MongoConnectionBuilder UseUrl(MongoUrl url) {
			ArgumentNullException.ThrowIfNull(url);

			mongoUrl = url;
			return this;
		}

		public MongoConnectionBuilder UseSettings(Action<MongoClientSettings> settings) {
			ArgumentNullException.ThrowIfNull(settings);

			this.settings = settings;
			return this;
		}
	}
}
