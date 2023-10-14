using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Deveel.Data {
	public abstract class EntityManagerTestSuite<TManager> : IAsyncLifetime where TManager : EntityManager<Person> {
		private AsyncServiceScope scope;

		protected EntityManagerTestSuite(ITestOutputHelper testOutput) {
			TestOutput = testOutput;

			CreateServices();
		}

		protected IServiceProvider Services => scope.ServiceProvider ?? throw new InvalidOperationException();

		protected ITestOutputHelper TestOutput { get; }

		protected IRepository<Person> Repository => Services.GetRequiredService<IRepository<Person>>();

		protected IQueryable<Person> People => Repository.AsQueryable().AsQueryable();

		protected TManager Manager => Services.GetRequiredService<TManager>();

		protected ISystemTime TestTime { get; } = new TestSystemTime();

		private void CreateServices() {
			var services = new ServiceCollection();

			services.AddLogging(logging => logging.AddXUnit(TestOutput));
			services.AddSingleton<IOperationCancellationSource>(new TestCancellationTokenSource());
			services.AddSystemTime(TestTime);

			ConfigureServices(services);

			scope = services.BuildServiceProvider().CreateAsyncScope();
		}

		protected virtual void ConfigureServices(IServiceCollection services) {
			services.AddRepository<InMemoryRepository<Person>>();
			services.AddEntityValidator<PersonValidator>();
			services.AddEntityManager<TManager>();
		}

		public async Task InitializeAsync() {
			var people = new PersonFaker().Generate(100);

			await Repository.AddRangeAsync(people);
		}

		public async Task DisposeAsync() {
			await Repository.RemoveRangeAsync(People);

			await scope.DisposeAsync();
		}

		[Fact]
		public async Task AddEntity() {
			var person = new PersonFaker().Generate();

			var result = await Manager.AddAsync(person);

			Assert.True(result.IsSuccess());
			Assert.NotNull(person.Id);
			Assert.NotNull(person.CreatedAtUtc);
			Assert.Null(person.UpdatedAtUtc);
			Assert.Equal(TestTime.UtcNow, person.CreatedAtUtc.Value);

			var found = await Repository.FindByKeyAsync(person.Id);

			Assert.NotNull(found);
			Assert.Equal(person.Id, found.Id);
		}

		[Fact]
		public async Task AddEntity_InvalidEmail() {
			var person = new PersonFaker().Generate();

			person.Email = "invalid";

			var result = await Manager.AddAsync(person);

			Assert.True(result.IsValidationError());
			Assert.NotNull(result.Error);

			var validationError = Assert.IsAssignableFrom<IValidationError>(result.Error);

			Assert.NotNull(validationError);
			Assert.Single(validationError.ValidationResults);
			Assert.Equal(nameof(Person.Email), validationError.ValidationResults[0].MemberNames.First());
		}

		[Fact]
		public async Task AddRangeOfEntities() {
			var people = new PersonFaker().Generate(10);

			var peopleCount = People.Count();

			var result = await Manager.AddRangeAsync(people);

			Assert.True(result.IsSuccess());
			Assert.Equal(peopleCount + 10, People.Count());

			var found = await Repository.FindAllAsync();

			Assert.NotNull(found);
			Assert.Equal(peopleCount + 10, found.Count());

			foreach (var person in people) {
				Assert.NotNull(person.Id);
				Assert.Contains(person, found);
			}
		}

		[Fact]
		public async Task AddRangeOfEntities_InvalidEmail() {
			var people = new PersonFaker().Generate(10);

			people[Random.Shared.Next(0, 9)].Email = "invalid";

			var result = await Manager.AddRangeAsync(people);

			Assert.True(result.IsValidationError());
			Assert.NotNull(result.Error);

			var validationError = Assert.IsAssignableFrom<IValidationError>(result.Error);

			Assert.NotNull(validationError);
			Assert.Single(validationError.ValidationResults);

			var validationResult = validationError.ValidationResults[0];
			Assert.NotNull(validationResult);
			Assert.Equal(nameof(Person.Email), validationResult.MemberNames.First());
		}

		[Fact]
		public async Task UpdateEntity() {
			var person = People.Random()!;

			var copy = new Person {
				Id = person.Id,
				FirstName = person.FirstName,
				LastName = person.LastName,
				BirthDate = person.BirthDate,
				Email = new Bogus.Faker().Internet.Email(),
				Phone = person.Phone
			};

			var result = await Manager.UpdateAsync(copy);
			Assert.False(result.IsValidationError());
			Assert.True(result.IsSuccess());

			Assert.NotNull(copy.UpdatedAtUtc);
			Assert.Equal(TestTime.UtcNow, copy.UpdatedAtUtc.Value);
		}

		[Fact]
		public async Task UpdateEntity_NoChanges() {
			var person = People.Random()!;

			var result = await Manager.UpdateAsync(person);

			Assert.True(result.IsNotModified());
			Assert.False(result.IsSuccess());
			Assert.Null(result.Error);

			Assert.Null(person.UpdatedAtUtc);
		}

		[Fact]
		public async Task UpdateEntity_NotFound() {
			var person = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var result = await Manager.UpdateAsync(person);

			Assert.True(result.IsError());
			Assert.False(result.IsSuccess());
			Assert.NotNull(result.Error);
			Assert.Equal(EntityErrorCodes.NotFound, result.Error.ErrorCode);
		}

		[Fact]
		public async Task UpdateEntity_NoKey() {
			var person = new PersonFaker()
				.Generate();

			var result = await Manager.UpdateAsync(person);

			Assert.True(result.IsError());
			Assert.False(result.IsSuccess());
			Assert.NotNull(result.Error);
			Assert.Equal(EntityErrorCodes.NotValid, result.Error.ErrorCode);
		}

		[Fact]
		public async Task RemoveEntity() {
			var person = People.Random()!;

			var result = await Manager.RemoveAsync(person);

			Assert.True(result.IsSuccess());
			Assert.False(result.IsError());
			Assert.Null(result.Error);

			var found = await Repository.FindByKeyAsync(person.Id!);

			Assert.Null(found);
		}

		[Fact]
		public async Task RemoveEntity_NotFound() {
			var person = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var result = await Manager.RemoveAsync(person);

			Assert.True(result.IsError());
			Assert.False(result.IsSuccess());
			Assert.NotNull(result.Error);
			Assert.Equal(EntityErrorCodes.NotFound, result.Error.ErrorCode);
		}

		[Fact]
		public async Task RemoveEntity_NoKey() {
			var person = new PersonFaker()
				.Generate();

			var result = await Manager.RemoveAsync(person);

			Assert.True(result.IsError());
			Assert.False(result.IsSuccess());
			Assert.NotNull(result.Error);
			Assert.Equal(EntityErrorCodes.NotValid, result.Error.ErrorCode);
		}

		[Fact]
		public async Task RemoveRange() {
			var peopleCount = People.Count();
			var people = People
				.Where(x => x.FirstName.StartsWith("A"))
				.ToList();

			var result = await Manager.RemoveRangeAsync(people);

			Assert.True(result.IsSuccess());
			Assert.False(result.IsError());
			Assert.Null(result.Error);

			var found = await Repository.FindAllAsync();

			Assert.NotNull(found);
			Assert.Equal(peopleCount - people.Count, found.Count());
		}

		[Fact]
		public async Task FindByKey() {
			var person = People.Random()!;

			var found = await Manager.FindByKeyAsync(person.Id!);

			Assert.NotNull(found);
			Assert.Equal(person.Id, found.Id);
		}

		[Fact]
		public async Task FindByKey_NotFound() {
			var person = new PersonFaker()
				.RuleFor(x => x.Id, f => f.Random.Guid().ToString())
				.Generate();

			var found = await Manager.FindByKeyAsync(person.Id!);

			Assert.Null(found);
		}

        [Fact]
        public async Task FindByKey_WrongKey() {
            var key = Random.Shared.Next(0, 100);

            var error = await Assert.ThrowsAsync<OperationException>(() => Manager.FindByKeyAsync(key));

            Assert.Equal(EntityErrorCodes.UnknownError, error.ErrorCode);
        }

		[Fact]
		public async Task FindFirstFiltered() {
			var person = People
				.Where(x => x.FirstName.StartsWith("A"))
				.OrderBy(x => x.Id)
				.FirstOrDefault();

			Assert.NotNull(person);

			var found = await Manager.FindFirstAsync(x => x.FirstName.StartsWith("A"));

			Assert.NotNull(found);
			Assert.Equal(person.Id, found.Id);
		}

		[Fact]
		public async Task FindFirstDynamicLinq() {
			var person = People
				.Where(x => x.FirstName.StartsWith("A"))
				.OrderBy(x => x.Id)
				.FirstOrDefault();

			Assert.NotNull(person);

			var found = await Manager.FindFirstAsync("FirstName.StartsWith(\"A\")");

			Assert.NotNull(found);
			Assert.Equal(person.Id, found.Id);
		}

        [Fact]
        public async Task FindFirst() {
            var person = People
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            Assert.NotNull(person);

            var found = await Manager.FindFirstAsync();

            Assert.NotNull(found);
            Assert.Equal(person.Id, found.Id);
        }

		[Fact]
		public async Task FindAllFiltered() {
			var people = People
				.Where(x => x.FirstName.StartsWith("A"))
				.ToList();

			var found = await Manager.FindAllAsync(x => x.FirstName.StartsWith("A"));

			Assert.NotNull(found);
			Assert.Equal(people.Count, found.Count);
		}

		[Fact]
		public async Task FindAllDynamicLinq() {
			var people = People
				.Where(x => x.FirstName.StartsWith("A"))
				.ToList();

			var found = await Manager.FindAllAsync("FirstName.StartsWith(\"A\")");

			Assert.NotNull(found);
			Assert.Equal(people.Count, found.Count);
		}

		[Fact]
		public async Task FindAll() {
			var people = People
				.ToList();

			var found = await Manager.FindAllAsync();

			Assert.NotNull(found);
			Assert.Equal(people.Count, found.Count);
		}

		[Fact]
		public async Task CountAll() {
			var count = await Manager.CountAsync();

			Assert.Equal(People.Count(), count);
		}

		[Fact]
		public async Task CountDynamicLinq() {
			var count = await Manager.CountAsync("FirstName.StartsWith(\"A\")");

			Assert.Equal(People.Count(x => x.FirstName.StartsWith("A")), count);
		}

		[Fact]
		public async Task CountFiltered() {
			var count = await Manager.CountAsync(x => x.FirstName.StartsWith("A"));

			Assert.Equal(People.Count(x => x.FirstName.StartsWith("A")), count);
		}

        [Fact]
        public void QueryEntities() {
            var people = People
                .Where(x => x.FirstName.StartsWith("A"))
                .ToList();

            var entities = (Manager.GetType()
                .GetProperty("Entities", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(Manager) as IQueryable<Person>);

            Assert.NotNull(entities);

            var found = entities
                .Where(x => x.FirstName.StartsWith("A"))
                .ToList();

            Assert.NotNull(found);
            Assert.Equal(people.Count, found.Count);
        }

        [Fact]
        public async Task GetSimplePage() {
            var totalPeople = People.Count();
            var totalPages = (int)Math.Ceiling((double)totalPeople / 10);
			var perPage = Math.Min(10, totalPeople);

            var query = new PageQuery<Person>(1, 10);
            var page = await Manager.GetPageAsync(query);

            Assert.NotNull(page);
            Assert.Equal(1, page.Request.Page);
            Assert.Equal(10, page.Request.Size);
            Assert.Equal(totalPages, page.TotalPages);
            Assert.Equal(totalPeople, page.TotalItems);
            Assert.NotNull(page.Items);
            Assert.Equal(perPage, page.Items.Count);
        }

		[Fact]
		public async Task GetPage_DynamicLinqFiltered() {
			var totalPeople = People.Count(x => x.FirstName.StartsWith("A"));
			var totalPages = (int)Math.Ceiling((double)totalPeople / 10);
			var perPage = Math.Min(10, totalPeople);

			var query = new PageQuery<Person>(1, 10)
				.Where("FirstName.StartsWith(\"A\")");

			var page = await Manager.GetPageAsync(query);

			Assert.NotNull(page);
			Assert.Equal(1, page.Request.Page);
			Assert.Equal(10, page.Request.Size);
			Assert.Equal(totalPages, page.TotalPages);
			Assert.Equal(totalPeople, page.TotalItems);
			Assert.NotNull(page.Items);
			Assert.Equal(perPage, page.Items.Count);
		}

		private class TestSystemTime : ISystemTime {
			public TestSystemTime() {
				UtcNow = DateTimeOffset.UtcNow;
				Now = DateTimeOffset.Now;
			}

			public DateTimeOffset UtcNow { get; }

			public DateTimeOffset Now { get; }
		}
	}
}
