using NUnit.Framework;

// Enable parallel execution at assembly level - tests in different classes can run in parallel
[assembly: Parallelizable(ParallelScope.All)]

// Set number of parallel workers (0 = auto-detect based on CPU cores, or set to specific number)
[assembly: LevelOfParallelism(0)] 