``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                    Method |     Mean |    Error |   StdDev |      Gen 0 |    Gen 1 | Allocated |
|-------------------------- |---------:|---------:|---------:|-----------:|---------:|----------:|
| RunOnIDisposableAnalyzers | 957.9 ms | 19.00 ms | 36.16 ms | 11375.0000 | 312.5000 |  68.48 MB |
