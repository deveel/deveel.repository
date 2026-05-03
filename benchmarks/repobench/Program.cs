using BenchmarkDotNet.Running;

var selection = DriverSelection.Parse(args);

if (selection.ShowUsage) {
	DriverSelection.WriteUsage();
	return;
}

var config = BenchmarkConfigFactory.Create(selection);

BenchmarkSwitcher
	.FromTypes(selection.BenchmarkTypes)
	.Run(selection.BenchmarkArgs, config);

