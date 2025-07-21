using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using Deveel.Data;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

using Testcontainers.MySql;

public class DbPerson
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}

public class PersonContext : DbContext
{
    private readonly string _connectionString;

    public PersonContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<DbPerson> People => Set<DbPerson>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL(_connectionString);
}

[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Net60, baseline: true)]
//[SimpleJob(RuntimeMoniker.Net70)]
//[SimpleJob(RuntimeMoniker.Net80)]
public class EfRepositoryBenchmarks
{
    private MySqlContainer? _container;
    private string? _connectionString;
    private PersonContext? _context;
	private IRepository<DbPerson, int> repository;
    private int? _personId;

	[GlobalSetup]
    public void GlobalSetup()
    {
        _container = new MySqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        _container.StartAsync().GetAwaiter().GetResult();
        _connectionString = _container.GetConnectionString();

		var context = new PersonContext(_connectionString!);
		context.Database.EnsureCreated();
		context.Database.Migrate();

        var entry = context.People.Add(new DbPerson { Name = "Initial Person" });
        context.SaveChanges();

        _context = context;

        _personId = entry.Entity.Id;

		repository = new EntityRepository<DbPerson, int>(context);
	}

	[GlobalCleanup]
    public void GlobalCleanup()
    {
		_context = new PersonContext(_connectionString!);
		foreach (var person in _context.People)
		{
			_context.Remove(person);
		}
		_context.SaveChanges();
		_context.Database.EnsureDeleted();
        _context.Dispose();

		(repository as IDisposable)?.Dispose();

		if (_container != null)
            _container.DisposeAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task Repository_InsertPerson()
    {
        await repository.AddAsync(new DbPerson { Name = "John Doe" });
    }

    [Benchmark]
	public async Task Repository_InsertMultiplePeople()
    {
        var people = new List<DbPerson>();
        for (int i = 0; i < 100; i++)
        {
            people.Add(new DbPerson { Name = $"Person {i}" });
        }

        await repository.AddRangeAsync(people);
	}

	[Benchmark]
    public async Task Repository_FindPersonById()
    {
        var person = await repository.FindAsync(_personId!.Value);

        if (person == null)
            throw new Exception("Person not found");
    }

    [Benchmark]
    public async Task DbContext_InsertPerson()
    {
        _context!.People.Add(new DbPerson { Name = "Jane Doe" });
        await _context.SaveChangesAsync();
	}

    [Benchmark]
    public async Task DbContext_InsertMultiplePeople()
    {
        var people = new List<DbPerson>();
        for (int i = 0; i < 100; i++)
        {
            people.Add(new DbPerson { Name = $"Person {i}" });
        }
        _context!.People.AddRange(people);
        await _context.SaveChangesAsync();
    }

    [Benchmark]
    public async Task DbContext_FindPersonById()
    {
        var person = await _context!.People.FindAsync(_personId);

        if (person == null)
            throw new Exception("Person not found");
	}
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<EfRepositoryBenchmarks>();
    }
}
