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

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[PlainExporter]
[MarkdownExporterAttribute.GitHub]
[HideColumns("Baseline", "RatioSD", "RatioSDMean", "RatioSDMedian")]
public class InMemoryRepositoryCoreBenchmarks {
    [Params(10, 100, 1_000)]
    public int EntityCount { get; set; }

    private InMemoryRepository<BenchPerson, string> _repository = default!;
    private List<BenchPerson> _seedTemplate = default!;
    private List<BenchPerson> _addRangeTemplate = default!;
    private BenchPerson _singleToAdd = default!;
    private List<BenchPerson> _batchToAdd = default!;
    private string _targetId = string.Empty;
    private BenchPerson _targetForUpdate = default!;
    private BenchPerson _targetForRemove = default!;

    [GlobalSetup]
    public void GlobalSetup() {
        _seedTemplate = Enumerable.Range(0, EntityCount)
            .Select(CreateSeedPerson)
            .ToList();
        _addRangeTemplate = Enumerable.Range(EntityCount, EntityCount)
            .Select(CreateSeedPerson)
            .ToList();
    }

    [IterationSetup(Target = nameof(AddAsync_One))]
    public void SetupForAddOne() {
        ResetRepository();
        _singleToAdd = new BenchPerson {
            FirstName = "Insert",
            LastName = "One",
            Email = "insert.one@example.test"
        };
    }

    [IterationSetup(Target = nameof(AddRangeAsync_Batch))]
    public void SetupForAddRange() {
        ResetRepository();
        _batchToAdd = _addRangeTemplate.Select(CloneWithoutId).ToList();
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
        _repository?.Dispose();
        _repository = new InMemoryRepository<BenchPerson, string>();

        var seeded = _seedTemplate.Select(CloneWithoutId).ToList();
        _repository.AddRangeAsync(seeded).GetAwaiter().GetResult();

        var middle = seeded[seeded.Count / 2];
        _targetId = middle.Id!;

        _targetForUpdate = Clone(middle);
        _targetForUpdate.Email = "updated@example.test";

        _targetForRemove = seeded[^1];
    }

    [GlobalCleanup]
    public void GlobalCleanup() => _repository?.Dispose();

    [Benchmark]
    public Task AddAsync_One() {
        return _repository.AddAsync(_singleToAdd);
    }

    [Benchmark]
    public Task AddRangeAsync_Batch() {
        return _repository.AddRangeAsync(_batchToAdd);
    }

    [Benchmark]
    public Task<BenchPerson?> FindAsync_ByKey() {
        return _repository.FindAsync(_targetId);
    }

    [Benchmark]
    public Task<bool> UpdateAsync_Entity() {
        return _repository.UpdateAsync(_targetForUpdate);
    }

    [Benchmark]
    public Task<bool> RemoveAsync_Entity() {
        return _repository.RemoveAsync(_targetForRemove);
    }

    [Benchmark]
    public Task<long> CountAsync_Filtered() {
        return _repository.CountAsync(x => x.Email != null && x.Email.StartsWith("seed-even-"));
    }

    [Benchmark]
    public Task<bool> ExistsAsync_Filtered() {
        return _repository.ExistsAsync(x => x.Email != null && x.Email.StartsWith("seed-odd-"));
    }

    private void ResetRepository() {
        _repository?.Dispose();
        _repository = new InMemoryRepository<BenchPerson, string>();
    }

    private static BenchPerson CreateSeedPerson(int index) {
        var group = index % 2 == 0 ? "even" : "odd";
        return new BenchPerson {
            FirstName = $"Name{index}",
            LastName = $"Surname{index}",
            Email = $"seed-{group}-{index}@example.test"
        };
    }

    private static BenchPerson CloneWithoutId(BenchPerson person) {
        return new BenchPerson {
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email
        };
    }

    private static BenchPerson Clone(BenchPerson person) {
        return new BenchPerson {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email
        };
    }
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<InMemoryRepositoryCoreBenchmarks>(null, args);
    }
}



