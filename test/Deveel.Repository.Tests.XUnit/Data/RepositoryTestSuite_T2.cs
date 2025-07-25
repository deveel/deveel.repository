﻿using Bogus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Net.Mail;

using Xunit.Abstractions;

namespace Deveel.Data {
	public abstract class RepositoryTestSuite<TPerson, TRelationship> : IAsyncLifetime
		where TPerson : class, IPerson
		where TRelationship : class, IRelationship {
		private IServiceProvider? services;
		private AsyncServiceScope scope;

		protected RepositoryTestSuite(ITestOutputHelper? testOutput) {
			TestOutput = testOutput;
		}

		protected ITestOutputHelper? TestOutput { get; }

		protected virtual int EntitySetCount => 100;

		protected IReadOnlyList<TPerson>? People { get; private set; }

		protected int PeopleCount => People?.Count ?? 0;

		protected IServiceProvider Services => scope.ServiceProvider;

		protected virtual IRepository<TPerson> Repository { get; private set; }

		protected abstract Faker<TPerson> PersonFaker { get; }

		protected abstract Faker<TRelationship> RelationshipFaker { get; }

		protected TPerson GeneratePerson() => PersonFaker.Generate();

		protected TRelationship GenerateRelationship() => RelationshipFaker.Generate();

		protected ISystemTime TestTime { get; } = new TestTime();

		protected IList<TPerson> GeneratePeople(int count) => PersonFaker.Generate(count);

		protected abstract string GeneratePersonId();

		protected virtual void ConfigureServices(IServiceCollection services) {
			if (TestOutput != null)
				services.AddLogging(logging => { logging.ClearProviders(); logging.AddXUnit(TestOutput); });
		}

		protected virtual Task<IRepository<TPerson>> GetRepositoryAsync()
		{
			return Task.FromResult(Services.GetRequiredService<IRepository<TPerson>>());
		}

		private void BuildServices() {
			var services = new ServiceCollection();
			services.AddSystemTime(TestTime);

			ConfigureServices(services);

			this.services = services.BuildServiceProvider();
			scope = this.services.CreateAsyncScope();
		}

		async Task IAsyncLifetime.InitializeAsync() {
			BuildServices();

			People = GeneratePeople(EntitySetCount).ToImmutableList();
			Repository = await GetRepositoryAsync();

			await InitializeAsync();
		}

		protected virtual async Task InitializeAsync() {
			await SeedAsync(Repository);
		}

		async Task IAsyncLifetime.DisposeAsync() {
			await DisposeAsync();

			People = null;

			await scope.DisposeAsync();
			(services as IDisposable)?.Dispose();
		}

		protected virtual Task DisposeAsync() {
			return Task.CompletedTask;
		}

		protected virtual async Task SeedAsync(IRepository<TPerson> repository) {
			if (People != null)
				await repository.AddRangeAsync(People);
		}

		protected virtual IEnumerable<TPerson> NaturalOrder(IEnumerable<TPerson> source) {
			return source;
		}

		protected abstract Task AddRelationshipAsync(TPerson person, TRelationship relationship);

		protected abstract Task RemoveRelationshipAsync(TPerson person, TRelationship relationship);

		protected virtual Task<TPerson?> FindPersonAsync(object id) {
			var entity = People?.FirstOrDefault(x => Repository.GetEntityKey(x)?.Equals(id) ?? false);
			return Task.FromResult(entity);
		}

		protected virtual Task<TPerson> RandomPersonAsync(Expression<Func<TPerson, bool>>? predicate = null) {
			var result = People?.Random(predicate?.Compile());

			if (result == null)
				throw new InvalidOperationException("No person found");

			return Task.FromResult(result);
		}

		[Fact]
		public async Task AddNewPerson() {
			var person = GeneratePerson();

			await Repository.AddAsync(person);

			var id = Repository.GetEntityKey(person);

			Assert.NotNull(id);

			var found = await Repository.FindAsync(id);
			Assert.NotNull(found);
			Assert.Equal(person.FirstName, found.FirstName);
			Assert.Equal(person.LastName, found.LastName);
			Assert.Equal(person.Email, found.Email);
		}

		[Fact]
		public async Task AddNewPerson_Sync() {
			var person = GeneratePerson();

			Repository.Add(person);

			var id = Repository.GetEntityKey(person);

			Assert.NotNull(id);

			var found = await Repository.FindAsync(id);

			Assert.NotNull(found);
			Assert.Equal(person.FirstName, found.FirstName);
			Assert.Equal(person.LastName, found.LastName);
			Assert.Equal(person.Email, found.Email);
		}

		[Fact]
		public async Task AddNewPersons() {
			var entities = GeneratePeople(10);

			await Repository.AddRangeAsync(entities);

			foreach (var item in entities) {
				var key = Repository.GetEntityKey(item);
				Assert.NotNull(key);

				var found = await Repository.FindAsync(key);
				Assert.NotNull(found);
			}
		}

		[Fact]
		public async Task AddNullPerson_ThrowsArgumentNullException() {
			await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.AddAsync(null!));
		}

		[Fact]
		public async Task AddRange_NullList_ThrowsArgumentNullException() {
			await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.AddRangeAsync(null!));
		}

		[Fact]
		public async Task RemoveNullPerson_ThrowsArgumentNullException() {
			await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.RemoveAsync(null!));
		}

		[Fact]
		public async Task RemoveRange_NullList_ThrowsArgumentNullException() {
			await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.RemoveRangeAsync(null!));
		}

		[Fact]
		public async Task FindAsync_NullKey_ThrowsArgumentNullException() {
			await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.FindAsync(default!));
		}

		[Fact]
		public async Task AddDuplicatePerson() {
			var person = GeneratePerson();
			await Repository.AddAsync(person);
			// Try adding the same person again, expect exception or specific behavior
			await Assert.ThrowsAsync<RepositoryException>(() => Repository.AddAsync(person));
		}

		[Fact]
		public async Task RemoveExisting() {
			var person = await RandomPersonAsync();

			Assert.NotNull(person);

			var result = await Repository.RemoveAsync(person);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveNotExisting() {
			var entity = GeneratePerson();

			entity.Id = GeneratePersonId();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public void RemoveExisting_Sync() {
			var person = People!.Random();

			Assert.NotNull(person);

			var result = Repository.Remove(person);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveByKey_Existing() {
			var key = Repository.GetEntityKey(People!.Random()!);

			Assert.NotNull(key);

			var result = await Repository.RemoveByKeyAsync(key);

			Assert.True(result);
		}

		[Fact]
		public void RemoveByKeySync_Existing() {
			var key = Repository.GetEntityKey(People!.Random()!);

			Assert.NotNull(key);

			var result = Repository.RemoveByKey(key);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveByKey_NotExisting() {
			var id = GeneratePersonId();

			var result = await Repository.RemoveByKeyAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task RemoveRangeOfExisting() {
			var peopleCount = PeopleCount;
			var people = People!.Take(10).ToList();

			await Repository.RemoveRangeAsync(people);

			var result = await Repository.FindAllAsync();
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount - 10, result.Count);
		}

		[Fact]
		public async Task RemoveRangeWithOneNotExisting() {
			var peopleCount = PeopleCount;
			var people = People!.Take(9).ToList();

			var entity = GeneratePerson();
			entity.Id = GeneratePersonId();

			people.Add(entity);

			await Assert.ThrowsAsync<RepositoryException>(() => Repository.RemoveRangeAsync(people));

			var result = await Repository.FindAllAsync();
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task CountAll() {
			var result = await Repository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(PeopleCount, result);
		}

		[Fact]
		public void CountAll_Sync() {
			var result = Repository.CountAll();

			Assert.NotEqual(0, result);
			Assert.Equal(PeopleCount, result);
		}

		[Fact]
		public async Task CountFiltered() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People?.Count(x => x.FirstName == firstName) ?? 0;

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task CountFiltered_Sync() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People?.Count(x => x.FirstName == firstName) ?? 0;

			var count = Repository.Count(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task FindByKey() {
			var person = await RandomPersonAsync();
			var id = person.Id!;

			var result = await Repository.FindAsync(id);

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FindFirstFiltered() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = await Repository.FindFirstAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task FindFirstFilteredAndSorted() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var expected = People?.Where(x => x.FirstName == firstName)
				.OrderBy(x => x.FirstName)
				.FirstOrDefault();

			Assert.NotNull(expected);

			var query = new QueryBuilder<TPerson>()
				.Where(x => x.FirstName == firstName)
				.OrderBy(x => x.FirstName)
				.Query;

			var result = await Repository.FindFirstAsync(query);

			Assert.NotNull(result);
			Assert.Equal(expected.FirstName, result.FirstName);
			Assert.Equal(expected.LastName, result.LastName);
		}

		[Fact]
		public void FindFirstSync() {
			var result = Repository.FindFirst();

			Assert.NotNull(result);
			Assert.NotNull(result.Id);
		}

		[Fact]
		public async Task ExistsFiltered() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task ExistsFiltered_Sync() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = Repository.Exists(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task FindByKey_Existing() {
			var person = await RandomPersonAsync();

			var result = await Repository.FindAsync(person.Id!);

			Assert.NotNull(result);
			Assert.Equal(person.Id, result.Id);

			Assert.Equal(person.FirstName, result.FirstName);
			Assert.Equal(person.LastName, result.LastName);
		}

		[Fact]
		public async Task FindByKey_NotFound() {
			var id = GeneratePersonId();

			var result = await Repository.FindAsync(id);

			Assert.Null(result);
		}

		[Fact]
		public async Task FindByKey_Sync() {
			var person = await RandomPersonAsync();

			var result = Repository.Find(person.Id!);

			Assert.NotNull(result);
			Assert.Equal(person.Id, result.Id);
		}

		[Fact]
		public async Task FindByKey_WithRelationsips() {
			var person = await RandomPersonAsync(x => x.Relationships != null && x.Relationships.Any());

			Assert.NotNull(person);

			var result = await Repository.FindAsync(person.Id!);

			Assert.NotNull(result);
			Assert.NotNull(result.Relationships);
			Assert.NotEmpty(result.Relationships);
		}

		[Fact]
		public async Task FindFirst() {
			var ordered = NaturalOrder(People!).ToList();

			var result = await Repository.FindFirstAsync();

			Assert.NotNull(result);
			Assert.Equal(ordered[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task FindFirstFiltered_Sync() {
			var person = await RandomPersonAsync(x => x.FirstName != null);
			var ordered = NaturalOrder(People!.Where(x => x.FirstName == person.FirstName)).ToList();

			// TODO: make an extension method for this
			var result = Repository.FindFirst(QueryFilter.Where<TPerson>(x => x.FirstName == person.FirstName));

			Assert.NotNull(result);
			Assert.Equal(ordered[0].Id, result.Id);
			Assert.Equal(ordered[0].FirstName, result.FirstName);
			Assert.Equal(ordered[0].LastName, result.LastName);
		}

		[Fact]
		public async Task FindAll() {
			var result = await Repository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(PeopleCount, result.Count);
		}

		[Fact]
		public void FindAll_Sync() {
			var result = Repository.FindAll();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(PeopleCount, result.Count);
		}

		[Fact]
		public async Task FindAllFiltered() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People?.Count(x => x.FirstName == firstName) ?? 0;

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task FindAllFilteredAndSorted() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var expected = People?.Where(x => x.FirstName == firstName)
				.OrderBy(x => x.FirstName)
				.ToList();

			Assert.NotNull(expected);

			var query = new QueryBuilder<TPerson>()
				.Where(x => x.FirstName == firstName)
				.OrderBy(x => x.FirstName);

			var result = await Repository.FindAllAsync(query);
			Assert.NotNull(result);
			Assert.NotEmpty(result);
		}

		[Fact]
		public async Task FindAllFiltered_BadFilter() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;

			var result = await Assert.ThrowsAsync<RepositoryException>(
				() => Repository.FindAllAsync(QueryFilter.Where<MailAddress>(m => m.Address == null)));
		}

		[Fact]
		public async Task GetSimplePage() {
			var totalItems = PeopleCount;
			var totalPages = (int)Math.Ceiling((double)totalItems / 10);

			var result = await Repository.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(totalItems, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public async Task GetSimplePage_WithParameters() {
			var totalItems = PeopleCount;
			var totalPages = (int)Math.Ceiling((double)totalItems / 10);

			var result = await Repository.GetPageAsync(1, 10);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(totalItems, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public async Task GetFilteredPage() {
			var person = await RandomPersonAsync();
			var firstName = person.FirstName;
			var peopleCount = People?.Count(x => x.FirstName == firstName) ?? 0;
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new PageQuery<TPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task GetPage_MultipleFilters() {
			var person = await RandomPersonAsync(x => x.LastName != null);
			var firstName = person.FirstName;
			var lastName = person.LastName;

			var peopleCount = People?.Count(x => x.FirstName == firstName && x.LastName == lastName) ?? 0;
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new PageQuery<TPerson>(1, 10)
				.Where(x => x.FirstName == firstName && x.LastName == lastName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count);
		}

		[Fact]
		public async Task GetPage_ChainedFilters() {
			var person = await RandomPersonAsync(x => x.DateOfBirth != null);
			var firstName = person.FirstName;
			var birthDate = person.DateOfBirth!.Value;

			var peopleCount = People?
				.Where(x => x.FirstName == firstName)
				.Where(x => x.DateOfBirth >= birthDate)
				.Count() ?? 0;

			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new PageQuery<TPerson>(1, 10)
				.Where(x => x.FirstName == firstName)
				.Where(x => x.DateOfBirth >= birthDate);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count);
		}

		[Fact]
		public async Task GetDescendingSortedPage() {
			var sorted = People!.Where(x => x.LastName != null)
				.OrderByDescending(x => x.LastName).Skip(0).Take(10).ToList();

			var request = new PageQuery<TPerson>(1, 10)
				.OrderByDescending(x => x.LastName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());

			for (int i = 0; i < sorted.Count; i++) {
				Assert.Equal(sorted[i].LastName, result.Items.ElementAt(i).LastName);
			}
		}

		[Fact]
		public async Task GetSortedPage() {
			var totalPages = (int)Math.Ceiling((double)PeopleCount / 10);

			var request = new PageQuery<TPerson>(1, 10)
				.OrderBy(x => x.FirstName);

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(PeopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public void GetPage_Sync() {
			var totalPages = (int)Math.Ceiling((double)PeopleCount / 10);

			var request = new PageQuery<TPerson>(1, 10);

			var result = Repository.GetPage(request);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(PeopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);
		}

		[Fact]
		public async Task GetPersonId() {
			var person = await RandomPersonAsync();

			var id = Repository.GetEntityKey(person);

			Assert.NotNull(id);
			Assert.Equal(person.Id, id?.ToString());
		}

		[Fact]
		public async Task UpdateExisting() {
			var person = await RandomPersonAsync(x => x.FirstName != "John");

			var toUpdate = await Repository.FindAsync(person.Id!);

			Assert.NotNull(toUpdate);

			toUpdate.FirstName = "John";

			var result = await Repository.UpdateAsync(toUpdate);

			Assert.True(result);

			var updated = await Repository.FindAsync(person.Id!);

			Assert.NotNull(updated);
			Assert.Equal(toUpdate.FirstName, updated.FirstName);
			Assert.Equal(toUpdate.LastName, updated.LastName);
			Assert.Equal(toUpdate.Email, updated.Email);
			Assert.Equal(toUpdate.DateOfBirth, updated.DateOfBirth);
		}

		[Fact]
		public async Task UpdateExisting_Sync() {
			var person = await RandomPersonAsync(x => x.FirstName != "John");

			var toUpdate = await Repository.FindAsync(person.Id!);

			Assert.NotNull(toUpdate);

			toUpdate.FirstName = "John";

			var result = Repository.Update(toUpdate);

			Assert.True(result);

			var updated = await Repository.FindAsync(person.Id!);

			Assert.NotNull(updated);
			Assert.Equal(toUpdate.FirstName, updated.FirstName);
			Assert.Equal(toUpdate.LastName, updated.LastName);
			Assert.Equal(toUpdate.Email, updated.Email);
			Assert.Equal(toUpdate.DateOfBirth, updated.DateOfBirth);
		}

		[Fact]
		public async Task UpdateNotExisting() {
			var person = GeneratePerson();

			person.Id = GeneratePersonId();

			var result = await Repository.UpdateAsync(person);

			Assert.False(result);
		}

		[Fact]
		public async Task UpdateNullPerson_ThrowsArgumentNullException() {
			await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.UpdateAsync(null!));
		}

		[Fact]
		public async Task UpdatePerson_CancellationRequested() {
			var person = await RandomPersonAsync();
			using var cts = new CancellationTokenSource();
			cts.Cancel();
			await Assert.ThrowsAsync<OperationCanceledException>(() => Repository.UpdateAsync(person, cts.Token));
		}

		[Fact]
		public async Task AddRange_EmptyList() {
			var emptyList = new List<TPerson>();
			await Repository.AddRangeAsync(emptyList);
			// Should not throw, and repository should remain unchanged
			var count = await Repository.CountAllAsync();
			Assert.Equal(PeopleCount, count);
		}

		[Fact]
		public async Task RemoveRange_EmptyList() {
			var emptyList = new List<TPerson>();
			await Repository.RemoveRangeAsync(emptyList);
			// Should not throw, and repository should remain unchanged
			var count = await Repository.CountAllAsync();
			Assert.Equal(PeopleCount, count);
		}

		[Fact]
		public async Task UpdateExisting_NoChange() {
			var person = await RandomPersonAsync();

			var toUpdate = await Repository.FindAsync(person.Id!);

			Assert.NotNull(toUpdate);

			await Repository.UpdateAsync(toUpdate);

			var updated = await Repository.FindAsync(person.Id!);
			Assert.NotNull(updated);
			Assert.Equal(toUpdate, updated);
		}

		[Fact]
		public async Task UpdateExisting_AddNewRelationship() {
			var person = People!.Random(x => x.Relationships == null || !x.Relationships.Any());

			Assert.NotNull(person);

			var relationship = GenerateRelationship();

			var toUpdate = await Repository.FindAsync(person.Id!);

			Assert.NotNull(toUpdate);

			await AddRelationshipAsync(toUpdate, relationship);

			var result = await Repository.UpdateAsync(toUpdate);

			Assert.True(result);

			var updated = await Repository.FindAsync(person.Id!);

			Assert.NotNull(updated);
			Assert.NotNull(updated.Relationships);
			Assert.NotEmpty(updated.Relationships);
			Assert.Single(updated.Relationships);
		}

		[Fact]
		public async Task UpdateExisting_RemoveRelationship() {
			var person = People!.Random(x => x.Relationships?.Any() ?? false);

			Assert.NotNull(person);

			var toUpdate = await Repository.FindAsync(person.Id!);

			Assert.NotNull(toUpdate);

			var relCount = toUpdate.Relationships.Count();

			await RemoveRelationshipAsync(toUpdate, (TRelationship)toUpdate.Relationships!.First());

			var result = await Repository.UpdateAsync(toUpdate);

			Assert.True(result);

			var updated = await Repository.FindAsync(person.Id!);
			Assert.NotNull(updated);
			Assert.NotNull(updated.Relationships);
			Assert.Equal(relCount - 1, updated.Relationships.Count());
		}
	}
}
