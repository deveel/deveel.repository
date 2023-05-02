using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]
    public class ApiContextTests : IAsyncLifetime {
        private readonly WebApplicationFactory<Program> api;
        private readonly MongoFrameworkTestFixture mongo;

		private readonly JwtTokenGenerator jwt;

		private string Tenant1Id { get; } = Guid.NewGuid().ToString();

		private string Tenant2Id { get; } = Guid.NewGuid().ToString();

        private readonly string userToken;

        public ApiContextTests(MongoFrameworkTestFixture mongo, ITestOutputHelper outputHelper) {
			Environment.SetEnvironmentVariable("Tenant1:TenantId", Tenant1Id);
			Environment.SetEnvironmentVariable("Tenant2:TenantId", Tenant2Id);

			var secretKey = Guid.NewGuid().ToString("N");
			Environment.SetEnvironmentVariable("JWT:SecretKey", secretKey);

			jwt = new JwtTokenGenerator(secretKey, "deveel");

			this.mongo = mongo;

            Environment.SetEnvironmentVariable("ConnectionStrings:Mongo", mongo.ConnectionString);

            api = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(webHostBuilder => {
					webHostBuilder.ConfigureLogging(logging => logging.AddXUnit(outputHelper).SetMinimumLevel(LogLevel.Trace));
                    webHostBuilder.ConfigureTestServices(ConfigureServices);
                });

            userToken = GenerateUserToken();
        }

        private string GenerateUserToken() {
			return jwt.GenerateToken("tenant2", "user1", "user");
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
        public async Task CreateNewPerson() {
            var client = api.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            var model = new PersonModel {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1)
            };
            var response = await client.PostAsync("/person", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();

            var person = JsonSerializer.Deserialize<PersonModel>(content);
            Assert.NotNull(person);
            Assert.Equal("John", person.FirstName);
            Assert.Equal("Doe", person.LastName);
            Assert.Equal(new DateTime(1980, 1, 1), person.BirthDate);
			Assert.Equal("tenant2", person.TenantId);
        }

        [Fact]
        public async Task CreateNewPersonForTenant() {
            var client = api.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            var model = new PersonModel {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1)
            };
            var response = await client.PostAsync("/tenant/tenant1/person", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var person = JsonSerializer.Deserialize<PersonModel>(content);
            Assert.NotNull(person);
            Assert.Equal("John", person.FirstName);
            Assert.Equal("Doe", person.LastName);
            Assert.Equal(new DateTime(1980, 1, 1), person.BirthDate);
			Assert.Equal(Tenant1Id, person.TenantId);
        }

		[Fact]
		public async Task CreateNewPersonForTenantFromClaim() {
			var client = api.CreateClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
			var model = new PersonModel {
				FirstName = "John",
				LastName = "Doe",
				BirthDate = new DateTime(1980, 1, 1)
			};
			var response = await client.PostAsync("/tenant/person", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var content = await response.Content.ReadAsStringAsync();
			var person = JsonSerializer.Deserialize<PersonModel>(content);
			Assert.NotNull(person);
			Assert.Equal("John", person.FirstName);
			Assert.Equal("Doe", person.LastName);
			Assert.Equal(new DateTime(1980, 1, 1), person.BirthDate);
			Assert.Equal(Tenant2Id, person.TenantId);

		}

		[Fact]
		public async Task CreateNewUser() {
			var client = api.CreateClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
			var model = new UserModel {
				Username = "user1",
				Email = "user1@example.com",
			};

			var response = await client.PostAsync("/user", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var content = await response.Content.ReadAsStringAsync();
			var user = JsonSerializer.Deserialize<UserModel>(content);
			Assert.NotNull(user);
			Assert.Equal("user1", user.Username);
			Assert.Equal("user1@example.com", user.Email);
			Assert.Equal(Tenant2Id, user.TenantId);

			response = await client.GetAsync($"/user/{user.Id}");
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			client = api.CreateClient();
			var user2Token = jwt.GenerateToken("tenant1", "user2", "user");

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);

			response = await client.GetAsync($"/user/{user.Id}");
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}
}
