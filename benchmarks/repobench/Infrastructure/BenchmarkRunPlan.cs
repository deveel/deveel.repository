using BenchmarkDotNet.Configs;

internal sealed record BenchmarkRunPlan(
	IConfig Config,
	IReadOnlyList<BenchmarkExportFormat> ExportFormats,
	string? ArtifactsPath);

