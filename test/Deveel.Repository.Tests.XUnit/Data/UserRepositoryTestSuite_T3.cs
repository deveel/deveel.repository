using Bogus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Immutable;
using System.Linq.Expressions;

using Xunit.Abstractions;

namespace Deveel.Data
{
	[Trait("Feature", "OwnerFilter")]
	public abstract class UserRepositoryTestSuite<TBook, TKey, TUserKey> : IAsyncLifetime, IAsyncDisposable
		where TBook : class, IBook<TKey>, IHaveOwner<TUserKey>
		where TKey : notnull
	{
		private IServiceProvider? services;
		private AsyncServiceScope scope;

		protected UserRepositoryTestSuite(ITestOutputHelper? outputHelper)
		{
			TestOutput = outputHelper;
			UserId = GenerateUserId();
		}

		protected ITestOutputHelper? TestOutput { get; }

		protected TUserKey UserId { get; }

		protected virtual int EntitySetCount => 100;

		protected IReadOnlyList<TBook>? Books { get; private set; } = new List<TBook>();

		protected IServiceProvider Services => scope.ServiceProvider;

		protected virtual IRepository<TBook, TKey> Repository => Services.GetRequiredService<IRepository<TBook, TKey>>();

		protected abstract Faker<TBook> BookFaker { get; }

		protected TBook GenerateBook() => BookFaker.Generate();

		protected TBook GenerateUserBook()
		{
			var book = BookFaker.Generate();
			book.SetOwner(UserId);
			return book;
		}

		protected IList<TBook> GenerateBooks(int count) => BookFaker.Generate(count);

		protected ISystemTime TestTime { get; } = new TestTime();

		protected abstract TUserKey GenerateUserId();

		protected abstract TKey GenerateBookId();

		protected virtual void ConfigureServices(IServiceCollection services)
		{
			if (TestOutput != null)
				services.AddLogging(logging => { logging.ClearProviders(); logging.AddXUnit(TestOutput); });

			services.AddSingleton<IUserAccessor<TUserKey>>(new StaticUserAccessor(this));
		}

		private void BuildServices()
		{
			var services = new ServiceCollection();
			services.AddSystemTime(TestTime);

			ConfigureServices(services);

			this.services = services.BuildServiceProvider();
			scope = this.services.CreateAsyncScope();
		}

		async Task IAsyncLifetime.InitializeAsync()
		{
			BuildServices();

			Books = GenerateBooks(EntitySetCount).ToImmutableList();

			await InitializeAsync();
		}

		protected virtual async Task InitializeAsync()
		{
			await SeedAsync();
		}

		async ValueTask IAsyncDisposable.DisposeAsync()
		{
			Books = null;

			await scope.DisposeAsync();
			(services as IDisposable)?.Dispose();
		}

		async Task IAsyncLifetime.DisposeAsync()
		{
			await DisposeAsync();
		}

		protected virtual Task DisposeAsync()
		{
			return Task.CompletedTask;
		}

		protected virtual async Task SeedAsync()
		{
			if (Books != null)
				await Repository.AddRangeAsync(Books);
		}

		protected virtual Task<TBook?> FindBookAsync(object id)
		{
			var entity = Books?.FirstOrDefault(x => Repository.GetEntityKey(x)?.Equals(id) ?? false);
			return Task.FromResult(entity);
		}

		protected virtual Task<TBook> RandomBookAsync(Expression<Func<TBook, bool>>? predicate = null)
		{
			var result = Books?.Random(predicate?.Compile());

			if (result == null)
				throw new InvalidOperationException("No book found");

			return Task.FromResult(result);
		}

		[Fact]
		public async Task AddNewUserBook()
		{
			var book = GenerateUserBook();

			await Repository.AddAsync(book);

			var id = Repository.GetEntityKey(book);

			Assert.NotNull(id);

			var found = await Repository.FindAsync(id);
			Assert.NotNull(found);
			Assert.Equal(book.Title, found.Title);
			Assert.Equal(book.Author, found.Author);
			Assert.Equal(book.Synopsis, found.Synopsis);
		}

		[Fact]
		public async Task AddNewBook()
		{
			var book = GenerateBook();

			await Repository.AddAsync(book);

			var id = Repository.GetEntityKey(book);

			Assert.NotNull(id);

			var found = await Repository.FindAsync(id);
			Assert.NotNull(found);
			Assert.Equal(book.Title, found.Title);
			Assert.Equal(book.Author, found.Author);
			Assert.Equal(book.Synopsis, found.Synopsis);
		}

		[Fact]
		public async Task FindBookById()
		{
			var book = await RandomBookAsync(x => Equals(x.Owner, UserId));

			var found = await Repository.FindAsync(book.Id!);

			Assert.NotNull(found);
			Assert.Equal(UserId, found.Owner);
			Assert.Equal(book.Title, found.Title);
			Assert.Equal(book.Author, found.Author);
			Assert.Equal(book.Synopsis, found.Synopsis);
		}

		[Fact]
		public async Task FindBookById_NotFound()
		{
			var id = GenerateBookId();

			var found = await Repository.FindAsync(id);

			Assert.Null(found);
		}

		[Fact]
		public async Task FindFirstBookById_OtherUser()
		{
			var book = await RandomBookAsync(x => !Equals(x.Owner, UserId));

			var found = await Repository.FindFirstAsync(x => x.Id.Equals(book.Id));

			Assert.Null(found);

			var bookInDb = await FindBookAsync(book.Id!);
			Assert.NotNull(bookInDb);
			Assert.NotEqual(UserId, bookInDb.Owner);
		}

		[Fact]
		public async Task FindBookById_OtherUser()
		{
			var book = await RandomBookAsync(x => !Equals(x.Owner, UserId));

			var found = await Repository.FindAsync(book.Id!);

			Assert.Null(found);

			var bookInDb = await FindBookAsync(book.Id!);
			Assert.NotNull(bookInDb);
			Assert.NotEqual(UserId, bookInDb.Owner);
		}



		class StaticUserAccessor : IUserAccessor<TUserKey>
		{
			private readonly UserRepositoryTestSuite<TBook, TKey, TUserKey> suite;

			public StaticUserAccessor(UserRepositoryTestSuite<TBook, TKey, TUserKey> suite)
			{
				this.suite = suite;
			}

			public TUserKey GetUserId() => suite.UserId;
		}
	}
}
