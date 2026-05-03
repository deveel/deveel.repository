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

    public int Group { get; set; }
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
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[PlainExporter]
[MarkdownExporterAttribute.GitHub]
[HideColumns("Baseline", "RatioSD", "RatioSDMean")]
public class EfRepositoryCoreBenchmarks
{
    [Params(100, 1_000)]
    public int EntityCount { get; set; }

    [Params(25, 100)]
    public int BatchSize { get; set; }

    private MySqlContainer _container = default!;
    private string _connectionString = string.Empty;
    private PersonContext _context = default!;
    private IRepository<DbPerson, int> _repository = default!;
    private List<DbPerson> _seedTemplate = default!;
    private List<DbPerson> _addRangeTemplate = default!;
    private DbPerson _singleToAdd = default!;
    private List<DbPerson> _batchToAdd = default!;
    private int _targetId;
    private DbPerson _targetForUpdate = default!;
    private DbPerson _targetForRemove = default!;

    [GlobalSetup]
    public void GlobalSetup() {
        _container = new MySqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        _container.StartAsync().GetAwaiter().GetResult();
        _connectionString = _container.GetConnectionString();

        using var context = new PersonContext(_connectionString);
        context.Database.EnsureCreated();

        _seedTemplate = Enumerable.Range(0, EntityCount)
            .Select(i => new DbPerson {
                Name = $"seed-{(i % 2 == 0 ? "even" : "odd")}-{i}",
                Group = i % 2
            })
            .ToList();

        var rangeSize = Math.Max(EntityCount, BatchSize);
        _addRangeTemplate = Enumerable.Range(0, rangeSize)
            .Select(i => new DbPerson { Name = $"batch-{i}", Group = i % 2 })
            .ToList();
    }

    [IterationSetup(Target = nameof(AddAsync_One))]
    public void SetupForAddOne() {
        ResetContext(seedData: false);
        _singleToAdd = new DbPerson { Name = "insert-one", Group = 0 };
    }

    [IterationSetup(Target = nameof(AddRangeAsync_Batch))]
    public void SetupForAddRange() {
        ResetContext(seedData: false);
        _batchToAdd = _addRangeTemplate
            .Take(BatchSize)
            .Select(item => new DbPerson { Name = item.Name, Group = item.Group })
            .ToList();
    }

    [IterationSetup(
        Targets =
        [
            nameof(FindAsync_ByKey),
            nameof(UpdateAsync_Entity),
            nameof(RemoveAsync_Entity),
            nameof(CountAsync_Filtered),
            nameof(ExistsAsync_Filtered)
        ])]
    public void SetupForSeededOperations() {
        ResetContext(seedData: true);
    }

    [Benchmark]
    public Task AddAsync_One() {
        return _repository.AddAsync(_singleToAdd);
    }

    [Benchmark]
    public Task AddRangeAsync_Batch() {
        return _repository.AddRangeAsync(_batchToAdd);
    }

    [Benchmark]
    public Task<DbPerson?> FindAsync_ByKey() => _repository.FindAsync(_targetId);

    [Benchmark]
    public Task<bool> UpdateAsync_Entity() => _repository.UpdateAsync(_targetForUpdate);

    [Benchmark]
    public Task<bool> RemoveAsync_Entity() => _repository.RemoveAsync(_targetForRemove);

    [Benchmark]
    public Task<long> CountAsync_Filtered() => _repository.CountAsync(x => x.Name.StartsWith("seed-even-"));

    [Benchmark]
    public Task<bool> ExistsAsync_Filtered() => _repository.ExistsAsync(x => x.Name.StartsWith("seed-odd-"));

    [GlobalCleanup]
    public void GlobalCleanup() {
        DisposeContextAndRepository();

        if (!string.IsNullOrEmpty(_connectionString)) {
            using var context = new PersonContext(_connectionString);
            context.Database.EnsureDeleted();
        }

        _container.DisposeAsync().GetAwaiter().GetResult();
    }

    private void ResetContext(bool seedData) {
        DisposeContextAndRepository();

        _context = new PersonContext(_connectionString);
        _context.Database.EnsureCreated();

        _context.People.RemoveRange(_context.People.ToList());
        _context.SaveChanges();

        if (seedData) {
            var seeded = _seedTemplate
                .Select(item => new DbPerson { Name = item.Name, Group = item.Group })
                .ToList();
            _context.People.AddRange(seeded);
            _context.SaveChanges();

            var middle = seeded[seeded.Count / 2];
            _targetId = middle.Id;

            _targetForUpdate = middle;
            _targetForUpdate.Name = $"updated-{middle.Id}";

            _targetForRemove = seeded[^1];
        }

        _repository = new EntityRepository<DbPerson, int>(_context);
    }

    private void DisposeContextAndRepository() {
        (_repository as IDisposable)?.Dispose();
        _context?.Dispose();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<EfRepositoryCoreBenchmarks>(null, args);
    }
}
