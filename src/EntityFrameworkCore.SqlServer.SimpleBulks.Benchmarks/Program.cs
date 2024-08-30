using BenchmarkDotNet.Running;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

_ = BenchmarkRunner.Run<BulkInsertSingleTableBenchmarks>();