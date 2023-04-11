// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using RedisCache.Benchmark;
using System;

Console.WriteLine("Start test Redis client methods performance");
var x = new BenchmarkManager();
x.GlobalSetup();
x.RepeatCount = 1;
x.Add_Redis();

BenchmarkRunner.Run<BenchmarkManager>();