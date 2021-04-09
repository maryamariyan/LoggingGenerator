# LoggingGenerator

This is an example showing how we can arrange to have strongly typed logging APIs for modern .NET apps.

The point of this exercise is to create a logging model which:

* Is delightful for service developers
* Eliminates string formatting
* Eliminates memory allocations
* Enables output in a dense binary format
* Enables more effective auditing of the logs produced by a component

Use is pretty simple. A service developer creates a class which lists all of the log messages that the assembly can produce.
Once this is done, new methods are generated automatically which the developer uses to interact with an ILogger instance. 

The `Microsoft.Extensions.Logging.Generators` project uses C# 9.0 source generators. This is magic voodoo invoked at compile time. This code is
responsible for finding methods annotated with the `[LoggerMessage]` attribute and automatically generating the strongly-typed
logging methods.

## Examples

Here is an example class written by a developer, followed by the code being auto-generated.

```csharp
static partial class Log
{
    [LoggerMessage(0, LogLevel.Critical, "Could not open socket to `{hostName}`")]
    public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
}
```

And the resulting generated code:


```csharp
ststic partial class Log
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Extensions.Logging.Generators", "1.0.0.0")]
    private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, string, global::System.Exception?> __CouldNotOpenSocketCallback =
        global::Microsoft.Extensions.Logging.LoggerMessage.Define<string>(global::Microsoft.Extensions.Logging.LogLevel.Critical, new global::Microsoft.Extensions.Logging.EventId(0, nameof(CouldNotOpenSocket)), "Could not open socket to `{hostName}`"); 

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Extensions.Logging.Generators", "1.0.0.0")]
    public static partial void CouldNotOpenSocket(Microsoft.Extensions.Logging.ILogger logger, string hostName)
    {
        if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Critical))
        {
            __CouldNotOpenSocketCallback(logger, hostName, null);
        }
    }
}
```

## Modes and Options

The example above shows the canonical use of the model, where the logging method is static and the log level is specified in the attribute definition.
There are other forms possible too. For example, in the following example the logging method is declared as an instance method. In this
form, the logging method gets the logger by accessing the `_logger` field in the containing class.

```csharp
class MyLogWrapper
{
    private readonly ILogger _logger;
    
    public MyLogWrapper(ILogger logger)
    {
        _logger = logger;
    }
    
    [LoggerMessage(0, LogLevel.Critical, "Could not open socket to `{hostName}`")]
    public partial void CouldNotOpenSocket(string hostName);
}
```

Sometimes, the logging level needs to be a dynamic property rather than being statically built into the code. You can do this by omitting the logging level
from the attribute and instead supplying it as an argument to the logging method:

```csharp
static partial class Log
{
    [LoggerMessage(0, "Could not open socket to `{hostName}`")]
    public static partial void CouldNotOpenSocket(ILogger logger, LogLevel level, string hostName);
}
```

You can omit the logging message and a default one will be provided for you that formats the arguments into a JSON-encoded string:

```csharp
static partial class Log
{
    [LoggerMessage(0, LogLevel.Critical)]
    public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
}
```

## Options

The generator supports 2 global options that it recognizes:

* `PascalCaseArguments` : YES/NO

    This will convert argument names to pascal case (from `hostName` to `HostName`) within the generated code such that when the ILogger enumerates
    the state, the argumenta will be in pascal case, which can make the logs nicer to consume. This defaults to YES.

* `EmitDefaultMessage` : YES/NO

    This controls whether to generate a default message when none is supplied in the attribute. This defaults to YES.

## Analyzer and Fixer

The `Microsoft.Extensions.Logging.Analyzers` project contains an analyzer that produces a warning
for any uses of the legacy `LoggerExtensions.Log<Debug|Information|Warning|Error|Critical|Trace>` 
methods.

For most of these uses, fixer logic is available that makes it trivial for a user to highlight
a call to a legacy log method and have a shiny new strongly-typed logging method signature 
generated automatically. This makes it a snap to convert existing log uses to the new more
efficient form.

## Benchmark

Here are the current benchmark results:

```plain
|                  Method |       Mean |      Error |     StdDev |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------ |-----------:|-----------:|-----------:|---------:|------:|------:|----------:|
|         ClassicLogging1 | 927.357 us | 18.5083 us | 39.4428 us | 173.8281 |     - |     - |  728001 B |
|         ClassicLogging2 | 586.637 us |  9.5880 us |  8.9686 us |  92.7734 |     - |     - |  392000 B |
|          LoggerMessage1 | 844.514 us | 16.5958 us | 14.7117 us | 188.4766 |     - |     - |  792000 B |
|          LoggerMessage2 | 528.927 us | 10.0096 us |  8.8732 us | 118.1641 |     - |     - |  496000 B |
|                 LogGen1 | 733.230 us | 12.0292 us | 15.6413 us | 188.4766 |     - |     - |  792000 B |
|                 LogGen2 | 513.936 us | 10.0079 us | 16.7210 us | 122.0703 |     - |     - |  512000 B |
| ClassicLogging1Disabled | 995.989 us | 11.5435 us | 10.2330 us | 173.8281 |     - |     - |  728001 B |
| ClassicLogging2Disabled | 649.658 us |  9.2190 us |  8.6234 us |  92.7734 |     - |     - |  392000 B |
|  LoggerMessage1Disabled |   9.109 us |  0.0881 us |  0.0781 us |        - |     - |     - |         - |
|  LoggerMessage2Disabled |   6.215 us |  0.0529 us |  0.0469 us |        - |     - |     - |         - |
|         LogGen1Disabled |   6.643 us |  0.0602 us |  0.0534 us |        - |     - |     - |         - |
|         LogGen2Disabled |   5.440 us |  0.0809 us |  0.0717 us |        - |     - |     - |         - |
```

## Current State

This is a general proposal for how this functionality can be integrated into the main .NET distribution. This is why
assemblies have the Microsoft.* prefix. If this doesn't get integrated into .NET, I'll change the assemblies to use
a different naming scheme for general consumption.

The fact this uses types generated dynamically at compile-time means
that symbols aren't available at edit-time. Smart IDEs like VS 2019+
handle this automatically. But editors which aren't tightly integrated
with Roslyn will show red squiggles to the developer, which is sad.

Anyway, feel free to use this as-is but there's no compatibility guarantee. Use at your own risks.
