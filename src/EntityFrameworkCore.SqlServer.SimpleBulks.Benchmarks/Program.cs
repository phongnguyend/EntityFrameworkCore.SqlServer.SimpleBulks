using BenchmarkDotNet.Running;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;


//var test = new BulkMatchSingleColumnBenchmarks();
//test.RowsCount = 10;
//test.GlobalSetup();
//test.Test();
//return;

//_ = BenchmarkRunner.Run<BulkInsertSingleTableBenchmarks>();
//_ = BenchmarkRunner.Run<BulkInsertMultipleTablesBenchmarks>();
//_ = BenchmarkRunner.Run<BulkUpdateBenchmarks>();
//_ = BenchmarkRunner.Run<BulkDeleteBenchmarks>();
//_ = BenchmarkRunner.Run<BulkMergeBenchmarks>();
_ = BenchmarkRunner.Run<BulkMatchSingleColumnBenchmarks>();