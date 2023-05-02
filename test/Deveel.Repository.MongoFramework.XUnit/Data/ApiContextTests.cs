using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Deveel.Data {
    [Collection("Mongo Single Database")]
    public class ApiContextTests : IAsyncLifetime {
        private readonly WebApplicationFactory<Program> api;
        private readonly MongoFrameworkTestFixture mongo;

        private readonly string userToken;

        public ApiContextTests(MongoFrameworkTestFixture mongo) {
            this.mongo = mongo;

            Environment.SetEnvironmentVariable("ConnectionStrings:Mongo", mongo.ConnectionString);

            api = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(webHostBuilder => {
                    webHostBuilder.ConfigureTestServices(ConfigureServices);
                });

            userToken = GenerateUserToken();
        }

        private static string GenerateUserToken() {
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("sub", "1234567890"),
                    new Claim("tenant", "abc1234567890"),
                    new Claim("role", "admin"),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
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
            var response = await client.PostAsync("/1234567890/person", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var person = JsonSerializer.Deserialize<PersonModel>(content);
            Assert.NotNull(person);
            Assert.Equal("John", person.FirstName);
            Assert.Equal("Doe", person.LastName);
            Assert.Equal(new DateTime(1980, 1, 1), person.BirthDate);
        }
    }
}
