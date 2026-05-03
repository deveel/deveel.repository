# Deveel.Repository.Benchmarks

Unified BenchmarkDotNet runner for the repository implementations in this solution.

## Drivers

- `in-memory`
- `ef`
- `mongo`
- `all`

If no driver is specified, the runner defaults to `in-memory`.

## Exporters

- `markdown`
- `plain`
- `csv`
- `html`

You can repeat `--export`, pass a comma-separated list, or use `all`.

If no export is specified, the runner generates plain text and GitHub-flavored markdown reports.

> `ef` and `mongo` start disposable Docker containers through Testcontainers.

## Usage

```bash
dotnet run -c Release --framework net8.0 --project benchmarks/Deveel.Repository.Benchmarks/Deveel.Repository.Benchmarks.csproj -- --driver in-memory

dotnet run -c Release --framework net8.0 --project benchmarks/Deveel.Repository.Benchmarks/Deveel.Repository.Benchmarks.csproj -- --driver ef --filter '*FindAsync_ByKey*'

dotnet run -c Release --framework net8.0 --project benchmarks/Deveel.Repository.Benchmarks/Deveel.Repository.Benchmarks.csproj -- /driver:mongo --list flat

dotnet run -c Release --framework net8.0 --project benchmarks/Deveel.Repository.Benchmarks/Deveel.Repository.Benchmarks.csproj -- --driver all

dotnet run -c Release --framework net8.0 --project benchmarks/Deveel.Repository.Benchmarks/Deveel.Repository.Benchmarks.csproj -- --driver in-memory --export markdown --output docs/benchmarks

dotnet run -c Release --framework net8.0 --project benchmarks/Deveel.Repository.Benchmarks/Deveel.Repository.Benchmarks.csproj -- --driver in-memory --export markdown,csv --output docs/benchmarks
```

Any additional arguments are forwarded to BenchmarkDotNet.

The `--output` option sets the BenchmarkDotNet artifacts directory, so exported files will be written under that folder, typically in its `results/` subdirectory.


