using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Bogus;
using Deveel.Repository.TestApi.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public abstract class PageQueryTestBase : IDisposable {
		private readonly WebApplicationFactory<Program> webApp;

		protected PageQueryTestBase() {
			webApp = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => {
					services.AddSingleton<IRepository<PersonEntity>>(_ => CreateTestRepository());
				}));
		}

		protected abstract string RouteName { get; }

		private static InMemoryRepository<PersonEntity> CreateTestRepository() {
			var faker = new Faker<PersonEntity>()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString("N"))
				.RuleFor(x => x.FirstName, f => f.Name.FirstName())
				.RuleFor(x => x.LastName, f => f.Name.LastName())
				.RuleFor(x => x.BirthDate, f => f.Date.Past(20));

			var mapper = FieldMapper.Map<PersonEntity>(f => {
				return f.ToUpperInvariant() switch {
					"ID" => p => p.Id,
					"FIRSTNAME" => p => p.FirstName,
					"LASTNAME" => p => p.LastName,
					_ => throw new NotSupportedException()
				};
			});

			return new InMemoryRepository<PersonEntity>(faker.Generate(120), mapper);
		}

		public void Dispose() {
			webApp?.Dispose();
		}

		[Fact]
		public async Task SimplePageQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>($"/person/{RouteName}?p=1&c=10");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);
			Assert.NotNull(response.Next);
			Assert.NotNull(response.Self);
			Assert.NotNull(response.First);
			Assert.NotNull(response.Last);
			Assert.Null(response.Previous);

			var queryParams = HttpUtility.ParseQueryString(response.Next.Query);
			Assert.NotNull(queryParams["p"]);
			Assert.Equal("2", queryParams["p"]);
		}

		[Fact]
		public async Task SortedPageQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>($"/person/{RouteName}?p=1&c=10&s=lastName");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);
			Assert.NotNull(response.Next);
			Assert.NotNull(response.Self);
			Assert.NotNull(response.First);
			Assert.NotNull(response.Last);
			Assert.Null(response.Previous);

			var hasSorts = QueryStringResultSort.TryParseQueryString(response.Self.Query, "s", out var sorts);
			Assert.True(hasSorts);
			Assert.NotNull(sorts);
			Assert.NotEmpty(sorts);
			Assert.Single(sorts);

			var fieldSort = Assert.IsType<FieldResultSort>(sorts[0]);
			Assert.Equal("lastName", fieldSort.FieldName);

			var repository = webApp.Services.GetRequiredService<IRepository<PersonEntity>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<PersonEntity>>(repository);

			var surnames = inMemoryRepo.Entities.OrderBy(x => x.LastName).Take(10).Select(x => x.LastName);

			Assert.True(surnames.SequenceEqual(response.Items.Select(x => x.LastName)));
		}

		[Fact]
		public async Task FilteredByAgeQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>($"/person/{RouteName}?p=1&c=10&maxAge=23");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);
			Assert.NotNull(response.Next);
			Assert.NotNull(response.Self);
			Assert.NotNull(response.First);
			Assert.NotNull(response.Last);
			Assert.Null(response.Previous);

			var queryParams = HttpUtility.ParseQueryString(response.Self.Query);
			Assert.NotNull(queryParams["maxAge"]);
			Assert.Equal("23", queryParams["maxAge"]);

			var repository = webApp.Services.GetRequiredService<IRepository<PersonEntity>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<PersonEntity>>(repository);

			var byAge = inMemoryRepo.Entities.Where(x => x.BirthDate != null && (DateTimeOffset.Now.Subtract(x.BirthDate.Value).Days / 365.25) <= 23)
				.Take(10).Select(x => x.Id);

			Assert.True(byAge.SequenceEqual(response.Items.Select(x => x.Id)));
		}


		[Fact]
		public async Task DefaultPageSizeQuery() {
			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>($"/person/{RouteName}");

			Assert.NotNull(response);
			Assert.Equal(120, response.TotalItems);
			Assert.Equal(12, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Equal(10, response.Items.Count);
			Assert.NotNull(response.Next);
			Assert.NotNull(response.Self);
			Assert.NotNull(response.First);
			Assert.NotNull(response.Last);
			Assert.Null(response.Previous);

			var repository = webApp.Services.GetRequiredService<IRepository<PersonEntity>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<PersonEntity>>(repository);

			var surnames = inMemoryRepo.Entities.Take(10).Select(x => x.LastName);

			Assert.True(surnames.SequenceEqual(response.Items.Select(x => x.LastName)));
		}

		[Fact]
		public async Task FilterPageQuery() {
			var repository = webApp.Services.GetRequiredService<IRepository<PersonEntity>>();
			var inMemoryRepo = Assert.IsType<InMemoryRepository<PersonEntity>>(repository);

			var lastName = inMemoryRepo.Entities.Select(x => x.LastName).First();
			var persons = inMemoryRepo.Entities.Where(x => x.LastName == lastName);
			var totalPersons = persons.Count();
			var totalPages = (int)Math.Ceiling((double)totalPersons / 10);

			var client = webApp.CreateClient();
			var response = await client.GetFromJsonAsync<TestPersonPage>($"/person/{RouteName}?p=1&c=10&lastName={lastName}");

			Assert.NotNull(response);
			Assert.Equal(totalPersons, response.TotalItems);
			Assert.Equal(totalPages, response.TotalPages);
			Assert.NotNull(response.Items);
			Assert.NotEmpty(response.Items);
			Assert.Null(response.Next);
		}


		class TestPersonPage {
			public int? TotalItems { get; set; }

			public int? TotalPages { get; set; }

			public IList<TestPerson> Items { get; set; }

			public Uri? Next { get; set; }

			public Uri? Previous { get; set; }

			public Uri? Self { get; set; }

			public Uri? First { get; set; }

			public Uri? Last { get; set; }
		}

		class TestPerson {
			public string Id { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateTime? BirthDate { get; set; }
		}
	}
}
