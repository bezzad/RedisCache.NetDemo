// See https://aka.ms/new-console-template for more information
using RedisCache.Benchmark;
using System;
using System.Diagnostics;

Console.WriteLine("Redis vs. Memory cache performance benchmark");

#if DEBUG

var sw = new Stopwatch();
var x = new BenchmarkManager();
x.GlobalSetup();
x.RepeatCount = 1000;
Console.WriteLine($"Repeating each test {x.RepeatCount} times");
Console.WriteLine("\n\n");

sw.Start();
x.AddGet_Memory();
sw.Stop();
Console.WriteLine($"{nameof(x.AddGet_Memory)}() tested. \t\t\t{sw.Elapsed.TotalNanosecond():N0}ns");

sw.Restart();
await x.AddGet_Memory_Async();
sw.Stop();
Console.WriteLine($"{nameof(x.AddGet_Memory_Async)}() tested. \t\t\t{sw.Elapsed.TotalNanosecond():N0}ns");

sw.Restart();
x.AddGet_Redis();
sw.Stop();
Console.WriteLine($"{nameof(x.AddGet_Redis)}() tested. \t\t\t\t{sw.Elapsed.TotalNanosecond():N0}ns");

sw.Restart();
await x.AddGet_Redis_Async();
sw.Stop();
Console.WriteLine($"{nameof(x.AddGet_Redis_Async)}() tested. \t\t\t{sw.Elapsed.TotalNanosecond():N0}ns");

sw.Restart();
x.AddGet_FireAndForget_Redis();
sw.Stop();
Console.WriteLine($"{nameof(x.AddGet_FireAndForget_Redis)}() tested. \t\t{sw.Elapsed.TotalNanosecond():N0}ns");

sw.Restart();
await x.AddGet_FireAndForget_Redis_Async();
sw.Stop();
Console.WriteLine($"{nameof(x.AddGet_FireAndForget_Redis_Async)}() tested. \t{sw.Elapsed.TotalNanosecond():N0}ns");

#else

BenchmarkRunner.Run<BenchmarkManager>();

#endif