internal sealed record DriverSelection(
	BenchmarkDriver Driver,
	string[] BenchmarkArgs,
	IReadOnlyList<BenchmarkExportFormat> ExportFormats,
	string? OutputPath,
	bool ShowUsage = false) {
	private static readonly Type[] InMemoryBenchmarks = [typeof(InMemoryRepositoryBenchmarks)];
	private static readonly Type[] EntityFrameworkBenchmarks = [typeof(EfRepositoryBenchmarks)];
	private static readonly Type[] MongoBenchmarks = [typeof(MongoRepositoryBenchmarks)];
	private static readonly Type[] AllBenchmarks =
	[
		typeof(InMemoryRepositoryBenchmarks),
		typeof(EfRepositoryBenchmarks),
		typeof(MongoRepositoryBenchmarks)
	];

	public Type[] BenchmarkTypes => Driver switch {
		BenchmarkDriver.InMemory => InMemoryBenchmarks,
		BenchmarkDriver.EntityFramework => EntityFrameworkBenchmarks,
		BenchmarkDriver.Mongo => MongoBenchmarks,
		_ => AllBenchmarks
	};

	public static DriverSelection Parse(string[] args) {
		var benchmarkArgs = new List<string>(args.Length);
		var exportFormats = new List<BenchmarkExportFormat>();
		BenchmarkDriver driver = BenchmarkDriver.InMemory;
		string? outputPath = null;
		var showUsage = false;

		for (var i = 0; i < args.Length; i++) {
			var argument = args[i];

			if (TryReadOption(argument, "driver", out var driverValue, out var consumesNextArgument)) {
				if (string.IsNullOrWhiteSpace(driverValue)) {
					if (!consumesNextArgument || i + 1 >= args.Length)
						throw new ArgumentException("The driver option requires a value.");

					driverValue = args[++i];
				}

				if (driverValue.Equals("help", StringComparison.OrdinalIgnoreCase) ||
					driverValue.Equals("?", StringComparison.OrdinalIgnoreCase)) {
					showUsage = true;
					continue;
				}

				driver = ParseDriver(driverValue);
				continue;
			}

			if (TryReadOption(argument, "export", out var exportValue, out consumesNextArgument)) {
				if (string.IsNullOrWhiteSpace(exportValue)) {
					if (!consumesNextArgument || i + 1 >= args.Length)
						throw new ArgumentException("The export option requires a value.");

					exportValue = args[++i];
				}

				foreach (var exportFormat in ParseExportFormats(exportValue)) {
					exportFormats.Add(exportFormat);
				}

				continue;
			}

			if (TryReadOption(argument, "output", out var outputValue, out consumesNextArgument)) {
				if (string.IsNullOrWhiteSpace(outputValue)) {
					if (!consumesNextArgument || i + 1 >= args.Length)
						throw new ArgumentException("The output option requires a value.");

					outputValue = args[++i];
				}

				outputPath = outputValue;
				continue;
			}

			benchmarkArgs.Add(argument);
		}

		return new DriverSelection(driver, benchmarkArgs.ToArray(), exportFormats, outputPath, showUsage);
	}

	public static void WriteUsage(TextWriter? writer = null) {
		writer ??= Console.Out;
		writer.WriteLine("Unified repository benchmarks");
		writer.WriteLine();
		writer.WriteLine("Driver options:");
		writer.WriteLine("  --driver in-memory");
		writer.WriteLine("  --driver ef");
		writer.WriteLine("  --driver mongo");
		writer.WriteLine("  --driver all");
		writer.WriteLine("  /driver:in-memory");
		writer.WriteLine("  --export markdown,csv,html,plain");
		writer.WriteLine("  --output docs/benchmarks");
		writer.WriteLine();
		writer.WriteLine("If no driver is specified, the default is 'in-memory'.");
		writer.WriteLine("If no export is specified, the runner generates plain text and GitHub-flavored markdown reports.");
		writer.WriteLine("Entity Framework and Mongo benchmarks require Docker because they bootstrap containers via Testcontainers.");
		writer.WriteLine();
		writer.WriteLine("Examples:");
		writer.WriteLine("  dotnet run -c Release --framework net8.0 --project benchmarks/repobench/repobench.csproj -- --driver in-memory");
		writer.WriteLine("  dotnet run -c Release --framework net8.0 --project benchmarks/repobench/repobench.csproj -- --driver ef --filter '*FindAsync_ByKey*'");
		writer.WriteLine("  dotnet run -c Release --framework net8.0 --project benchmarks/repobench/repobench.csproj -- /driver:mongo --list flat");
		writer.WriteLine("  dotnet run -c Release --framework net8.0 --project benchmarks/repobench/repobench.csproj -- --driver in-memory --export markdown --output docs/benchmarks");
	}

	private static bool TryReadOption(string argument, string optionName, out string? value, out bool consumesNextArgument) {
		value = null;
		consumesNextArgument = false;

		if (argument.Equals($"--{optionName}", StringComparison.OrdinalIgnoreCase) ||
			argument.Equals($"/{optionName}", StringComparison.OrdinalIgnoreCase)) {
			consumesNextArgument = true;
			return true;
		}

		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var prefixes = new[] { $"--{optionName}=", $"--{optionName}:", $"/{optionName}=", $"/{optionName}:" };
		foreach (var prefix in prefixes) {
			if (argument.StartsWith(prefix, comparison)) {
				value = argument[prefix.Length..];
				return true;
			}
		}

		return false;
	}

	private static IEnumerable<BenchmarkExportFormat> ParseExportFormats(string value) {
		foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
			var normalized = token.Trim().ToLowerInvariant();

			if (normalized is "all" or "*") {
				yield return BenchmarkExportFormat.Markdown;
				yield return BenchmarkExportFormat.Csv;
				yield return BenchmarkExportFormat.Html;
				yield return BenchmarkExportFormat.Plain;
				continue;
			}

			yield return normalized switch {
				"md" or "markdown" or "github-markdown" => BenchmarkExportFormat.Markdown,
				"csv" => BenchmarkExportFormat.Csv,
				"html" => BenchmarkExportFormat.Html,
				"plain" or "text" or "txt" => BenchmarkExportFormat.Plain,
				_ => throw new ArgumentException($"Unsupported export format '{token}'.")
			};
		}
	}

	private static BenchmarkDriver ParseDriver(string value) {
		return value.Trim().ToLowerInvariant() switch {
			"in-memory" or "inmemory" or "memory" => BenchmarkDriver.InMemory,
			"ef" or "entityframework" or "entity-framework" => BenchmarkDriver.EntityFramework,
			"mongo" or "mongodb" => BenchmarkDriver.Mongo,
			"all" => BenchmarkDriver.All,
			_ => throw new ArgumentException($"Unsupported benchmark driver '{value}'.")
		};
	}
}

