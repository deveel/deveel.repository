using System.Net.Http.Json;

using Bogus;

using Deveel.Data.Models;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public class RepositoryPageQueryTests : IDisposable {
		private readonly WebApplicationFactory<Program> webApp;

		public RepositoryPageQueryTests() {
			webApp = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => {
					services.AddSingleton<IRepository<TestPersonModel>>(_ => CreateTestRepository());
				}));
		}

		private static InMemoryRepository<TestPersonModel> CreateTestRepository() {
			var faker = new Faker<TestPersonModel>()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString("N"))
				.RuleFor(x => x.FirstName, f => f.Name.FirstName())
				.RuleFor(x => x.LastName, f => f.Name.LastName())
				.RuleFor(x => x.BirthDate, f => f.Date.Past(20));

			var mapper = FieldMapper.Map<TestPersonModel>(f => {
				return f.ToUpperInvariant() switch {
					"ID" => p => p.Id,
					"FIRSTNAME" => p => p.FirstName,
					"LASTNAME" => p => p.LastName,
					_ => throw new NotSupportedException()
				};
			});

			return new InMemoryRepository<TestPersonModel>(faker.Generate(120), mapper);
		}

		public void Dispose() {
			webApp?.Dispose();
		}

		[Fact]
		public async Task SimplePageQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>("/person/page?p=1&c=10");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);
		}

		[Fact]
		public async Task SortedPageQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>("/person/page?p=1&c=10&s=lastName");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);

			var repository = webApp.Services.GetRequiredService<IRepository<TestPersonModel>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<TestPersonModel>>(repository);

			var surnames = inMemoryRepo.Entities.OrderByDescending(x => x.LastName).Take(10).Select(x => x.LastName);

			Assert.True(surnames.SequenceEqual(response.Items.Select(x => x.LastName)));
		}

		[Fact]
		public async Task FilteredByAgeQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>("/person/page?p=1&c=10&maxAge=23");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);

			var repository = webApp.Services.GetRequiredService<IRepository<TestPersonModel>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<TestPersonModel>>(repository);

			var byAge = inMemoryRepo.Entities.Where(x => x.BirthDate != null && (DateTimeOffset.Now.Subtract(x.BirthDate.Value).Days / 365.25) <= 23)
				.Take(10).Select(x => x.Id);

			Assert.True(byAge.SequenceEqual(response.Items.Select(x => x.Id)));
		}


		[Fact]
		public async Task DefaultPageSizeQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>("/person/page");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);

			var repository = webApp.Services.GetRequiredService<IRepository<TestPersonModel>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<TestPersonModel>>(repository);

			var surnames = inMemoryRepo.Entities.Take(10).Select(x => x.LastName);

			Assert.True(surnames.SequenceEqual(response.Items.Select(x => x.LastName)));
		}

		[Fact]
		public async Task FilterPageQuery() {
			var repository = webApp.Services.GetRequiredService<IRepository<TestPersonModel>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<TestPersonModel>>(repository);

			var lastName = inMemoryRepo.Entities.Select(x => x.LastName).First();
			var persons = inMemoryRepo.Entities.Where(x => x.LastName == lastName);
			var totalPersons = persons.Count();
			var totalPages = (int)Math.Ceiling((double) totalPersons / 10);

			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>($"/person/page?p=1&c=10&lastName={lastName}");

			Assert.NotNull(response);
			Assert.Equal(totalPersons, response.TotalItems);
			Assert.Equal(totalPages, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
		}

		class TestPersonPage {
			public int? TotalItems { get; set; }

			public int? TotalPages { get; set; }

			public IList<TestPerson> Items { get; set; }
		}

		class TestPerson {
			public string Id { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }	

			public DateTime? BirthDate { get; set; }
		}
	}
}
