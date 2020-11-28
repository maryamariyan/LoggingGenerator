# LoggingGenerator

This is an example showing how we can arrange to have strongly typed logging APIs.

The point of this exercise is to create a logging model which:

* Is delightful for service developers
* Eliminates string formatting
* Eliminates memory allocations
* Enables output in a dense binary format
* Enables more effective auditing of log data

Use is pretty simple. A service developer creates a class which lists all of the log messages that the assembly can produce.
Once this is done, new methods are generated automatically which the developer uses to interact with an ILogger instance. 

The Microsoft.Extensions.Logging.Generators project uses C# 9.0 source generators. This is magic voodoo invoked at compile time. This code is
responsible for finding types annotated with the [LoggerExtensions] attribute and automatically generating the strongly-typed
logging methods.

## Analyzer and Fixes

The Microsoft.Extensions.Logging.Analyzers project contains an analyzer that produces a warning
for any uses of the legacy LoggerExtensions.Log<Debug|Information|Warning|Error|Critical|Trace>() 
methods.

For most of these uses, fixer logic is available that makes it trivial for a user to highlight
call to a legacy log method and have a shiny new strongly-type logging method signature 
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

* Support nullable message parameter types
* Support extension method syntax for logging methods
* The Microsoft.Extensions.Logging.Extras assembly is only temporary. The types in here should go to the Microsoft.Extensions.Logging.Abstractions assembly
* Get improved SemanticModel workaround from Jared

Analyzer

* Add unit tests

Fixer

* Generate a comment above the logging method sig
* Handle cases where the message string is a const
* Pick a different filename for the generated log file if the project already contains a file by that name.
* Pick a different method name if the target class already contains a method by that name
* If the target class is in the same file as the legacy call site, then things get confused
* Add unit tests

## Example

Here is an example interface written by a developer, followed by the code being auto-generated.

```csharp
partial class Log
{
    [LoggerMessage(0, LogLevel.Critical, "Could not open socket to `{hostName}`")]
    public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
}
```

And the resulting generated code:


```csharp
partial class Log
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
