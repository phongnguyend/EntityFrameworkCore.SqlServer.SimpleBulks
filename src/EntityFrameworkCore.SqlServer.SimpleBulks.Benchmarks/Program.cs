using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

// BenchmarkDotNet 0.15.8's SDK validator does not recognize .NET 11 yet.
// Use its standard out-of-process build/run components with an explicit TFM.
// The SDK still validates framework support when building the generated project.
var toolchain = new Toolchain(".NET 11",
    new CsProjGenerator("net11.0", null!, null!, null!),
    new DotNetCliBuilder("net11.0"),
    new DotNetCliExecutor(null!));
var config = DefaultConfig.Instance.AddJob(Job.Default
    .WithToolchain(toolchain)
    .AsMutator())
    // Parallel-build selection also requires a recognized runtime moniker.
    .WithOptions(ConfigOptions.DisableParallelBuild);

var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

var failed = false;
foreach (var summary in summaries)
{
    foreach (var error in summary.ValidationErrors.Where(error => error.IsCritical))
    {
        Console.Error.WriteLine($"Benchmark validation failed: {error.Message}");
        failed = true;
    }

    foreach (var report in summary.Reports.Where(report => !report.Success))
    {
        Console.Error.WriteLine($"Benchmark failed: {report.BenchmarkCase.DisplayInfo}");
        failed = true;
    }
}

if (failed)
    Console.Error.WriteLine("One or more benchmarks failed. See the benchmark logs in the uploaded artifacts for details.");

return failed ? 1 : 0;
