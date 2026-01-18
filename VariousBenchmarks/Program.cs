using BenchmarkDotNet.Running;

using VariousBenchmarks.Parsing;
using VariousBenchmarks.Streams;
using VariousBenchmarks.Streams.Peak;

// BenchmarkRunner.Run<Base64StreamEncodingBenchmarks>();

// return;
double avg;
avg = PeakMemory.Run(10, 1024).Average();
Console.WriteLine(avg);

avg = PeakMemory.Run(10, 64 * 1024).Average();
Console.WriteLine(avg);

avg = PeakMemory.Run(10, 1024 * 1024).Average();
Console.WriteLine(avg);
