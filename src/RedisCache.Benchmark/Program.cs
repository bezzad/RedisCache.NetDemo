// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using RedisCache.Benchmark;
using System;
using System.Reflection;
using System.Threading.Tasks;

Console.WriteLine("Redis vs. Memory cache performance benchmark");

#if DEBUG

var sw = new System.Diagnostics.Stopwatch();
var manager = new BenchmarkManager();
manager.GlobalSetup();
manager.RepeatCount = 10000;
Console.WriteLine($"Repeating each test {manager.RepeatCount} times");
Console.WriteLine("\n");
Console.WriteLine(new string('-', 91));
var headerDesc = "|".PadRight(18) + "Test Method Name".PadRight(32) + "|  Duration (milisecond)".PadRight(24) + "  |  Is Async?  |";
Console.WriteLine(headerDesc);

var methods = typeof(BenchmarkManager).GetMethods(BindingFlags.Public | BindingFlags.Instance);
foreach (var method in methods)
{
    if (method.GetCustomAttribute(typeof(BenchmarkAttribute)) != null)
    {
        Console.WriteLine(new string('-', 91));
        var isAsync = false;
        sw.Restart();
        if (method.ReturnType == typeof(Task))
        {
            isAsync = true;
            var task = (Task)method.Invoke(manager, null);
            await task;
        }
        else
        {
            method.Invoke(manager, null);
        }
        sw.Stop();

        var leftDesc = $"| {method.Name}() tested. ".PadRight(50);
        var rightDesc = $"|   {sw.Elapsed.TotalMilliseconds:N0}ms ".PadRight(25);
        Console.WriteLine(leftDesc + rightDesc + $" |    {isAsync}".PadRight(15) + "|");
    }
}
Console.WriteLine(new string('-', 91));

#else

BenchmarkDotNet.Running.BenchmarkRunner.Run<BenchmarkManager>();

#endif