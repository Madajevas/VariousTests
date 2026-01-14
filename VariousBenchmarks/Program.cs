using BenchmarkDotNet.Running;

using VariousBenchmarks.Parsing;
using VariousBenchmarks.Streams;

BenchmarkRunner.Run<Base64StreamDecodingBenchmarks>();
