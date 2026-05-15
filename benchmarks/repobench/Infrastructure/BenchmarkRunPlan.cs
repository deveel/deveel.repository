using BenchmarkDotNet.Configs;

using Deveel.Repository.Options;

namespace Deveel.Repository.Benchmarks.Infrastructure;

internal sealed record BenchmarkRunPlan(
	IConfig Config,
	IReadOnlyList<BenchmarkExportFormat> ExportFormats,
	string? ArtifactsPath);

