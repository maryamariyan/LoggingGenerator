# Using Crank to run benchmarks:

Install crank tool:
```
> dotnet tool install -g Microsoft.Crank.Controller --version "0.2.0-*"
```

## Run benchmarks remotely:

```
> crank --config Benchmark\benchmarks.yml --profile aspnet-perf-win
```

## Run benchmarks locally: 

Install and run crank agent if you want to be able to run locally:
```
 > dotnet tool install -g Microsoft.Crank.Agent --version "0.2.0-*" 
 > crank-agent
```

On another window/terminal, run crank command below:
```
> crank --config Benchmark\benchmarks.yml --profile local
```

Or:
```
> crank --config Benchmark\benchmarks.yml --profile local --scenario LogGen
```

This uses a filter to only run LogGen benchmarks, defined as a scenario, in the yml file.

## Run benchmarks using a remote source code:

Make sure the yml file uses `repository` and `branchOrCommit`:
```yml
      repository: https://github.com/geeknoid/LoggingGenerator
      branchOrCommit: master
```

## Run benchmarks using a local source code:
Make sure `localFolder` is set on the yml file in order to use a local copy of your source project:
```yml
     localFolder: .\.. # uncomment this to use your local copy of the source code
```

## Reference

For more information on using Crank tool for BenchmarkDotNet apps visit: https://github.com/dotnet/crank/blob/main/docs/microbenchmarks.md