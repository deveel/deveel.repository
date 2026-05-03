using BenchmarkDotNet.Running;

BenchmarkRunPlan? runPlan = null;

try {
	var selection = DriverSelection.Parse(args);

	if (selection.ShowUsage) {
		DriverSelection.WriteUsage();
		return;
	}

	runPlan = BenchmarkConfigFactory.Create(selection);

	var summaries = BenchmarkSwitcher
		.FromTypes(selection.BenchmarkTypes)
		.Run(selection.BenchmarkArgs, runPlan.Config)
		.ToArray();

	BenchmarkResultFileExporter.WriteSingleOutputFileIfRequested(selection, runPlan, summaries);
} catch (ArgumentException ex) {
	Console.Error.WriteLine(ex.Message);
	Console.Error.WriteLine();
	DriverSelection.WriteUsage(Console.Error);
	Environment.ExitCode = 1;
} catch (InvalidOperationException ex) {
	Console.Error.WriteLine(ex.Message);
	Environment.ExitCode = 1;
} catch (FileNotFoundException ex) {
	Console.Error.WriteLine(ex.Message);
	Environment.ExitCode = 1;
} finally {
	BenchmarkResultFileExporter.CleanupTemporaryArtifacts(runPlan?.ArtifactsPath);
}

