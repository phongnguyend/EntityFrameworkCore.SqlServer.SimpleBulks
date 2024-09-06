using BenchmarkDotNet.Running;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;


//_ = BenchmarkRunner.Run<BulkInsertSingleTableBenchmarks>();
//_ = BenchmarkRunner.Run<BulkInsertMultipleTablesBenchmarks>();
//_ = BenchmarkRunner.Run<BulkUpdateBenchmarks>();
//_ = BenchmarkRunner.Run<BulkDeleteBenchmarks>();
_ = BenchmarkRunner.Run<BulkMergeBenchmarks>();
//_ = BenchmarkRunner.Run<BulkMergeReturnDbGeneratedIdBenchmarks>();
//_ = BenchmarkRunner.Run<BulkMatchSingleColumnBenchmarks>();
//_ = BenchmarkRunner.Run<BulkMatchMultipleColumnsBenchmarks>();