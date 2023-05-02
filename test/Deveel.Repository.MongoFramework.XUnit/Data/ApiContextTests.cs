using System.Net;
using System.Text.Json;

using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    [Collection("Mongo Single Database")]
    public class ApiContextTests : IAsyncLifetime {
        private readonly WebApplicationFactory<Program> api;
        private readonly MongoFrameworkTestFixture mongo;

        public ApiContextTests(MongoFrameworkTestFixture mongo) {
            this.mongo = mongo;

            Environment.SetEnvironmentVariable("ConnectionStrings:Mongo", mongo.ConnectionString);

            api = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(webHostBuilder => {
                    webHostBuilder.ConfigureTestServices(ConfigureServices);
                });
        }

        public async Task DisposeAsync() {
            await api.DisposeAsync();
        }

        public Task InitializeAsync() {
            return Task.CompletedTask;
        }

        private void ConfigureServices(IServiceCollection services) {
            
        }

        [Fact]
        public async Task GetPerson() {
            var client = api.CreateClient();
            var response = await client.GetAsync("/person/5e9f1b7b9d9d9e0b7c6d9d9d");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            var person = JsonSerializer.Deserialize<PersonModel>(content);
            Assert.NotNull(person);
            Assert.Equal("John", person.FirstName);
            Assert.Equal("Doe", person.LastName);
        }
    }
}
