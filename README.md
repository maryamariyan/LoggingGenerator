﻿# LoggingGenerator

This is an example showing how we can arrange to have strongly typed logging APIs for modern .NET apps.

The point of this exercise is to create a logging model which:

* Is delightful for service developers
* Eliminates string formatting
* Eliminates memory allocations
* Enables output in a dense binary format
* Enables more effective auditing of log data

Use is pretty simple. A service developer creates a class which lists all of the log messages that the assembly can produce.
Once this is done, new methods are generated automatically which the developer uses to interact with an ILogger instance. 

The Microsoft.Extensions.Logging.Generators project uses C# 9.0 source generators. This is magic voodoo invoked at compile time. This code is
responsible for finding methods annotated with the [LoggerMessage] attribute and automatically generating the strongly-typed
logging methods.

## Current State

This is a general proposal for how this functionality can be integrated into the main .NET distribution. This is why
assemblies have the Microsoft.* prefix. If this doesn't get integrated into .NET, I'll change the assemblies to use
a different naming scheme for general consumption.

Anyway, feel free to use this as-is but there's no compatibility guarantee. Use at your own risks.

## Analyzer and Fixer

The Microsoft.Extensions.Logging.Analyzers project contains an analyzer that produces a warning
for any uses of the legacy LoggerExtensions.Log<Debug|Information|Warning|Error|Critical|Trace>() 
methods.

For most of these uses, fixer logic is available that makes it trivial for a user to highlight
a call to a legacy log method and have a shiny new strongly-typed logging method signature 
generated automatically. This makes it a snap to convert existing log uses to the new more
efficient form.

## Design Issues

The fact this uses types generated dynamically at compile-time means
that symbols aren't available at edit-time. Smart IDEs like VS 2019+
handle this automatically. But editors which aren't tightly integrated
with Roslyn will show red squiggles to the developer, which is sad.

## Implementation Todos

General

* Add nuget packaging voodoo

Generator

* The Microsoft.Extensions.Logging.Extras assembly is only temporary. The types in here should go to the
Microsoft.Extensions.Logging.Abstractions assembly

Analyzer

* Nothing to do.

Fixer

* BUG: If the target class is in the same file as the legacy call site, then things get confused

## Example

Here is an example interface written by a developer, followed by the code being auto-generated.

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
    private static readonly global::System.Func<global::Microsoft.Extensions.Logging.LogStateHolder<string>, global::System.Exception?, string> __CouldNotOpenSocketFormatFunc = (__holder, _) =>
    {
        var hostName = __holder.Value;
        return $"Could not open socket to `{hostName}`";
    };

    internal static partial void CouldNotOpenSocket(Microsoft.Extensions.Logging.ILogger __logger, string hostName)
    {
        if (__logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Critical))
        {
            __logger.Log(
                global::Microsoft.Extensions.Logging.LogLevel.Critical,
                new global::Microsoft.Extensions.Logging.EventId(0, nameof(CouldNotOpenSocket)),
                new global::Microsoft.Extensions.Logging.LogStateHolder<string>(nameof(hostName), hostName),
                null,
                __CouldNotOpenSocketFormatFunc);
        }
    }
}
```
