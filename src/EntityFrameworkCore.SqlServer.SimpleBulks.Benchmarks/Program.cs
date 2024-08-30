using BenchmarkDotNet.Running;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

//_ = BenchmarkRunner.Run<BulkInsertSingleTableBenchmarks>();
//_ = BenchmarkRunner.Run<BulkInsertMultipleTablesBenchmarks>();
//_ = BenchmarkRunner.Run<BulkUpdateBenchmarks>();
_ = BenchmarkRunner.Run<BulkDeleteBenchmarks>();