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
                new global::Microsoft.Extensions.Logging.Internal.LogValues<string>(nameof(hostName), hostName),
                null,
                __CouldNotOpenSocketFormatFunc);
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

Sometime, the logging level needs to be a dynamic property rather than being statically built into the code. You can do this by omiting the logging level 
from the attribute and instead supplying it as an argument to the logging method

```csharp
static partial class Log
{
    [LoggerMessage(0, "Could not open socket to `{hostName}`")]
    public static partial void CouldNotOpenSocket(ILogger logger, LogLevel level, string hostName);
}
```

You can omit the logging message and a default one will be provided for you that formats the arguments into a JSON-encoded string.

```csharp
static partial class Log
{
    [LoggerMessage(0, LogLevel.Critical)]
    public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
}
```

## Options

The generator supports 3 global options that it recognizes:

* `PascalCaseArguments` : YES/NO

    This will convert argument names to pascal case (from `hostName` to `HostName`) within the generated code such that when the ILogger enumerates
    the state, the argumenta will be in pascal case, which can make the logs nicer to consume. This defaults to NO.

* `EmitDefaultMessage` : YES/NO

    This controls whether to generate a default message when none is supplied in the attribute. This defaults to YES.

* `FieldName` : &lt;field name&gt;

    This controls the field name used when the logging methods are declared as instance methods. This defaults to `_logger`.

## Analyzer and Fixer

The `Microsoft.Extensions.Logging.Analyzers` project contains an analyzer that produces a warning
for any uses of the legacy `LoggerExtensions.Log<Debug|Information|Warning|Error|Critical|Trace>` 
methods.

For most of these uses, fixer logic is available that makes it trivial for a user to highlight
a call to a legacy log method and have a shiny new strongly-typed logging method signature 
generated automatically. This makes it a snap to convert existing log uses to the new more
efficient form.

## Current State

This is a general proposal for how this functionality can be integrated into the main .NET distribution. This is why
assemblies have the Microsoft.* prefix. If this doesn't get integrated into .NET, I'll change the assemblies to use
a different naming scheme for general consumption.

The fact this uses types generated dynamically at compile-time means
that symbols aren't available at edit-time. Smart IDEs like VS 2019+
handle this automatically. But editors which aren't tightly integrated
with Roslyn will show red squiggles to the developer, which is sad.

Anyway, feel free to use this as-is but there's no compatibility guarantee. Use at your own risks.
