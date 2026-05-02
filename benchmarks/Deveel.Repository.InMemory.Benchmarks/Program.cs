// Copyright 2023-2025 Antonello Provenzano
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using Bogus;

using Deveel.Data;

using System.ComponentModel.DataAnnotations;

// ---------------------------------------------------------------------------
// Domain model used by all benchmarks below
// ---------------------------------------------------------------------------

public class BenchPerson {
    [Key]
    public string? Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }
}

// ---------------------------------------------------------------------------
// Single-thread vs. multi-thread benchmark suite for InMemoryRepository
//
// Goals
// -----
// 1. Prove there is no meaningful single-thread regression after the
//    ReaderWriterLockSlim guard was introduced.
// 2. Quantify the throughput under realistic concurrency scenarios (parallel
//    test runners, background services, etc.).
//
// Run with:
//   dotnet run -c Release -- --filter *InMemoryRepositoryBenchmarks*
// ---------------------------------------------------------------------------

[MemoryDiagnoser]
[ThreadingDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[PlainExporter]
[MarkdownExporterAttribute.GitHub]
[HideColumns("Baseline", "RatioSD", "RatioSDMean", "RatioSDMedian")]
public class InMemoryRepositoryBenchmarks {
    // -----------------------------------------------------------------------
    // Parameters
    // -----------------------------------------------------------------------

    /// <summary>
    /// Number of entities queued for each batch benchmark.
    /// BenchmarkDotNet will run one iteration per value.
    /// </summary>
    [Params(10, 100, 1_000)]
    public int EntityCount { get; set; }

    /// <summary>
    /// Degree of parallelism used by the multi-thread benchmarks.
    /// </summary>
    [Params(4, 16, 64)]
    public int Parallelism { get; set; }

    // -----------------------------------------------------------------------
    // Shared state
    // -----------------------------------------------------------------------

    private static readonly Faker<BenchPerson> PersonFaker = new Faker<BenchPerson>("en")
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.Email, f => f.Internet.Email());

    // Shared Faker instance used for generating individual values during benchmarks.
    // Creating a new Faker() per entity is extremely expensive and dominates timings.
    private static readonly Faker _faker = new Faker();

    // A pre-seeded, shared repository that is rebuilt before every iteration so
    // each benchmark starts from a clean, known state.
    private InMemoryRepository<BenchPerson, string> _repository = default!;

    // A fixed set of already-added entities used by read/update/remove benchmarks
    private List<BenchPerson> _seeded = default!;

    // Entities staged for write benchmarks (not yet in the repository)
    private List<BenchPerson> _toAdd = default!;

    [GlobalSetup]
    public void GlobalSetup() {
        // nothing expensive here; per-iteration setup does the real work
    }

    [IterationSetup]
    public void IterationSetup() {
        _repository?.Dispose();
        _repository = new InMemoryRepository<BenchPerson, string>();

        // Seed EntityCount entities so that reads/updates/removes have something to work with
        _seeded = PersonFaker.Generate(EntityCount);
        foreach (var p in _seeded)
            _repository.AddAsync(p).GetAwaiter().GetResult();

        // Prepare fresh entities for write benchmarks (no key yet)
        _toAdd = PersonFaker.Generate(EntityCount);
    }

    [GlobalCleanup]
    public void GlobalCleanup() => _repository?.Dispose();

    // -----------------------------------------------------------------------
    // Single-thread — establishes the happy-path baseline
    // -----------------------------------------------------------------------

    /// <summary>
    /// Adds <see cref="EntityCount"/> entities one by one on a single thread.
    /// </summary>
    [Benchmark]
    public async Task SingleThread_AddSequential() {
        using var repo = new InMemoryRepository<BenchPerson, string>();
        var people = PersonFaker.Generate(EntityCount);
        foreach (var p in people)
            await repo.AddAsync(p);
    }

    /// <summary>
    /// Reads every seeded entity by its key on a single thread.
    /// </summary>
    [Benchmark]
    public async Task SingleThread_FindByKeySequential() {
        foreach (var p in _seeded)
            await _repository.FindAsync(p.Id!);
    }

    /// <summary>
    /// Updates every seeded entity's email on a single thread.
    /// </summary>
    [Benchmark]
    public async Task SingleThread_UpdateSequential() {
        foreach (var p in _seeded) {
            p.Email = _faker.Internet.Email();
            await _repository.UpdateAsync(p);
        }
    }

    // -----------------------------------------------------------------------
    // Multi-thread — validates correctness and measures concurrency overhead
    // -----------------------------------------------------------------------

    /// <summary>
    /// Adds <see cref="EntityCount"/> entities concurrently with
    /// <see cref="Parallelism"/> concurrent tasks.
    /// </summary>
    [Benchmark]
    public async Task MultiThread_AddConcurrent() {
        using var repo = new InMemoryRepository<BenchPerson, string>();

        var semaphore = new SemaphoreSlim(Parallelism);
        var tasks = _toAdd.Select(async p => {
            await semaphore.WaitAsync();
            try {
                await repo.AddAsync(p);
            } finally {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Reads all seeded entities concurrently with <see cref="Parallelism"/>
    /// concurrent tasks — exercises the shared read-lock path, which allows
    /// multiple readers to run simultaneously.
    /// </summary>
    [Benchmark]
    public async Task MultiThread_FindByKeyConcurrent() {
        var semaphore = new SemaphoreSlim(Parallelism);
        var tasks = _seeded.Select(async p => {
            await semaphore.WaitAsync();
            try {
                await _repository.FindAsync(p.Id!);
            } finally {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Mixed workload: concurrent reads and writes happening simultaneously.
    /// This is the most realistic scenario for parallel integration test runners
    /// and background services sharing a single in-memory repository instance.
    /// </summary>
    [Benchmark]
    public async Task MultiThread_MixedReadWrite() {
        var semaphore = new SemaphoreSlim(Parallelism);

        var writeTasks = _toAdd.Select(async p => {
            await semaphore.WaitAsync();
            try {
                await _repository.AddAsync(p);
            } finally {
                semaphore.Release();
            }
        });

        var readTasks = _seeded.Select(async p => {
            await semaphore.WaitAsync();
            try {
                await _repository.FindAsync(p.Id!);
            } finally {
                semaphore.Release();
            }
        });

        await Task.WhenAll(writeTasks.Concat(readTasks));
    }

    /// <summary>
    /// Reads the <see cref="InMemoryRepository{TEntity,TKey}.Entities"/> snapshot
    /// concurrently from <see cref="Parallelism"/> tasks.
    /// </summary>
    [Benchmark]
    public Task MultiThread_EntitiesSnapshot() {
        var tasks = Enumerable.Range(0, Parallelism).Select(_ =>
            Task.Run(() => { var snapshot = _repository.Entities; _ = snapshot.Count; })
        );
        return Task.WhenAll(tasks);
    }
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<InMemoryRepositoryBenchmarks>(null, args);
    }
}



