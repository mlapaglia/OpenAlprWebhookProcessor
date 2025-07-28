using NUnit.Framework;

// Enable parallel execution at assembly level - tests in different classes can run in parallel
[assembly: Parallelizable(ParallelScope.Fixtures)]

// Set number of parallel workers (adjust based on your system - typically CPU cores)
[assembly: LevelOfParallelism(4)] 