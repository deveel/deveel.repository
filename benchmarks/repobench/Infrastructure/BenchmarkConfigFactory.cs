using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;

internal static class BenchmarkConfigFactory {
	private static readonly BenchmarkExportFormat[] DefaultExportFormats =
	[
		BenchmarkExportFormat.Plain,
		BenchmarkExportFormat.Markdown
	];

	public static IConfig Create(DriverSelection selection) {
		var config = ManualConfig.CreateMinimumViable();
		var outputPath = selection.OutputPath;

		if (!String.IsNullOrWhiteSpace(outputPath)) {
			var fullPath = Path.GetFullPath(outputPath);
			Directory.CreateDirectory(fullPath);
			config.ArtifactsPath = fullPath;
		}

		foreach (var exporter in CreateExporters(selection.ExportFormats)) {
			config.AddExporter(exporter);
		}

		return config;
	}

	private static IEnumerable<IExporter> CreateExporters(IReadOnlyCollection<BenchmarkExportFormat> exportFormats) {
		var requestedFormats = exportFormats.Count == 0
			? DefaultExportFormats
			: exportFormats.ToArray();

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var exportFormat in requestedFormats) {
			var exporter = exportFormat switch {
				BenchmarkExportFormat.Markdown => MarkdownExporter.GitHub,
				BenchmarkExportFormat.Csv => CsvExporter.Default,
				BenchmarkExportFormat.Html => HtmlExporter.Default,
				BenchmarkExportFormat.Plain => PlainExporter.Default,
				_ => throw new ArgumentOutOfRangeException(nameof(exportFormat), exportFormat, "Unsupported export format")
			};

			if (seen.Add(exporter.Name))
				yield return exporter;
		}
	}
}



