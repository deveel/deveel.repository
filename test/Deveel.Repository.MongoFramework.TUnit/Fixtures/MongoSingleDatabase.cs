using DotNet.Testcontainers.Containers;

using MongoDB.Driver;

using Testcontainers.MongoDb;

namespace Deveel.Data {
    public class MongoSingleDatabase : IAsyncDisposable, IDisposable {
        private readonly MongoDbContainer container;
        private bool disposedValue;

        public MongoSingleDatabase() {
            container = new MongoDbBuilder()
                .WithUsername("")
                .WithPassword("")
                .Build();
        }

        public const string DatabaseName = "test_db";

        public string ConnectionString =>
                SetDatabase(container.GetConnectionString(), DatabaseName);

        private static string SetDatabase(string connectionString, string database) {
            var urlBuilder = new MongoUrlBuilder(connectionString);
            urlBuilder.DatabaseName = database;
            return urlBuilder.ToString();
        }

        public async Task StartAsync() {
            await container.StartAsync();
        }

        public async ValueTask DisposeAsync() {
            if (!disposedValue) {
                await container.StopAsync();
                while (container.State != TestcontainersStates.Exited) {
                    await Task.Delay(100);
                }

                await container.DisposeAsync();
                disposedValue = true;
            }
        }

        public void Dispose() {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}

