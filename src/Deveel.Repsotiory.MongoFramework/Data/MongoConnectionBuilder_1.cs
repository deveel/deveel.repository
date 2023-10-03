using MongoFramework;

namespace Deveel.Data {
	public sealed class MongoConnectionBuilder<TContext> : MongoConnectionBuilder where TContext : class, IMongoDbContext  {
		private readonly MongoConnectionBuilder builder;

		internal MongoConnectionBuilder(MongoConnectionBuilder builder) {
			this.builder = builder;
		}

		public override IMongoDbConnection Connection => new MongoDbConnection<TContext>(builder.Connection);
	}
}
