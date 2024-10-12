using BenchmarkDotNet.Running;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;


_ = BenchmarkRunner.Run<BulkInsertSingleTableBenchmarks>();
_ = BenchmarkRunner.Run<BulkInsertMultipleTablesBenchmarks>();
_ = BenchmarkRunner.Run<BulkUpdateBenchmarks1>();
_ = BenchmarkRunner.Run<BulkUpdateBenchmarks2>();
_ = BenchmarkRunner.Run<BulkDeleteBenchmarks1>();
_ = BenchmarkRunner.Run<BulkDeleteBenchmarks2>();
_ = BenchmarkRunner.Run<BulkMergeBenchmarks>();
_ = BenchmarkRunner.Run<BulkMergeReturnDbGeneratedIdBenchmarks1>();
_ = BenchmarkRunner.Run<BulkMergeReturnDbGeneratedIdBenchmarks2>();
_ = BenchmarkRunner.Run<BulkMatchSingleColumnBenchmarks1>();
_ = BenchmarkRunner.Run<BulkMatchSingleColumnBenchmarks2>();
_ = BenchmarkRunner.Run<BulkMatchMultipleColumnsBenchmarks1>();
_ = BenchmarkRunner.Run<BulkMatchMultipleColumnsBenchmarks2>();
_ = BenchmarkRunner.Run<TempTableBenchmarks>();