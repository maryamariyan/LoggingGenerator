// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Extensions.Logging.Generators
{
    public partial class LoggerMessageGenerator
    {
        internal class Emitter
        {
            // The maximum arity of LoggerMessage.Define.
            private const int MaxLoggerMessageDefineArguments = 6;

            private readonly string _generatedCodeAttribute =
                $"global::System.CodeDom.Compiler.GeneratedCodeAttribute(" +
                $"\"{typeof(Emitter).Assembly.GetName().Name}\", " +
                $"\"{typeof(Emitter).Assembly.GetName().Version}\")";
            private readonly Stack<StringBuilder> _builders = new ();

            public string Emit(IReadOnlyList<LoggerClass> logClasses, CancellationToken cancellationToken)
            {
                var sb = GetStringBuilder();
                try
                {
                    _ = sb.Append("// <auto-generated/>\n");
                    _ = sb.Append("#nullable enable\n");

                    foreach (var lc in logClasses)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _ = sb.Append(GenType(lc));
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private static string EscapeMessageString(string message)
            {
                return message
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\"", "\\\"");
            }

            private static bool UseLoggerMessageDefine(LoggerMethod lm)
            {
                var result =
                    (lm.RegularParameters.Count <= MaxLoggerMessageDefineArguments) && // more args than LoggerMessage.Define can handle
                    (lm.Level != null) &&                                              // dynamic log level, which LoggerMessage.Define can't handle
                    (lm.TemplateList.Count == lm.RegularParameters.Count);             // mismatch in template to args, which LoggerMessage.Define can't handle

                if (result)
                {
                    // make sure the order of the templates matches the order of the logging method parameter
                    int count = 0;
                    foreach (var t in lm.TemplateList)
                    {
                        if (!t.Equals(lm.RegularParameters[count].Name, StringComparison.OrdinalIgnoreCase))
                        {
                            // order doesn't match, can't use LoggerMessage.Define
                            return false;
                        }
                    }
                }

                return result;
            }

            private string GenType(LoggerClass lc)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var lm in lc.Methods)
                    {
                        if (!UseLoggerMessageDefine(lm))
                        {
                            _ = sb.Append(GenStruct(lm));
                        }

                        _ = sb.Append(GenLogMethod(lm));
                    }

                    _ = sb.Append(GenEnumerationHelper(lc));

                    if (string.IsNullOrWhiteSpace(lc.Namespace))
                    {
                        return $@"
                        partial class {lc.Name} {lc.Constraints}
                        {{
                            {sb}
                        }}
                        ";
                    }

                    return $@"
                    namespace {lc.Namespace}
                    {{
                        partial class {lc.Name} {lc.Constraints}
                        {{
                            {sb}
                        }}
                    }}
                    ";
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenStruct(LoggerMethod lm)
            {
                var constructor = string.Empty;
                if (lm.RegularParameters.Count > 0)
                {
                    constructor = $@"
                                public __{lm.Name}Struct({GenArguments(lm)})
                                {{
{GenFieldAssignments(lm)}
                                }}
";
                }

                var toString = $@"
                                public override string ToString()
                                {{
{GenVariableAssignments(lm)}
                                    return $""{lm.Message}"";
                                }}
";

                return $@"
                            [{_generatedCodeAttribute}]
                            private readonly struct __{lm.Name}Struct : global::System.Collections.Generic.IReadOnlyList<global::System.Collections.Generic.KeyValuePair<string, object?>>
                            {{
{GenFields(lm)}
{constructor}
{toString}
                                public static string Format(__{lm.Name}Struct state, global::System.Exception? ex) => state.ToString();

                                public int Count => {lm.RegularParameters.Count + 1};

                                public global::System.Collections.Generic.KeyValuePair<string, object?> this[int index]
                                {{
                                    get => index switch
                                    {{
{GenCases(lm)}
                                        _ => throw new global::System.IndexOutOfRangeException(nameof(index)),  // return the same exception LoggerMessage.Define returns in this case
                                    }};
                                }}

                                public global::System.Collections.Generic.IEnumerator<global::System.Collections.Generic.KeyValuePair<string, object?>> GetEnumerator()
                                {{
                                    for (int i = 0; i < {lm.RegularParameters.Count + 1}; i++)
                                    {{
                                        yield return this[i];
                                    }}
                                }}

                                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
                            }}
";
            }

            private string GenFields(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.RegularParameters)
                    {
                        _ = sb.Append($"                                private readonly {p.Type} _{p.Name};\n");
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenFieldAssignments(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.RegularParameters)
                    {
                        _ = sb.Append($"                                    this._{p.Name} = {p.Name};\n");
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenVariableAssignments(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var t in lm.TemplateMap)
                    {
                        int index = 0;
                        foreach (var p in lm.RegularParameters)
                        {
                            if (t.Key.Equals(p.Name, System.StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }

                            index++;
                        }

                        // check for an index that's too big, this can happen in some cases of malformed input
                        if (index < lm.RegularParameters.Count)
                        {
                            if (lm.RegularParameters[index].IsEnumerable)
                            {
                                _ = sb.Append($"                                    var {t.Key} = "
                                    + $"__Enumerate((global::System.Collections.IEnumerable ?)this._{lm.RegularParameters[index].Name});\n");
                            }
                            else
                            {
                                _ = sb.Append($"                                    var {t.Key} = this._{lm.RegularParameters[index].Name};\n");
                            }
                        }
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenCases(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    var index = 0;
                    foreach (var p in lm.RegularParameters)
                    {
                        var name = p.Name;
                        if (lm.TemplateMap.ContainsKey(name))
                        {
                            // take the letter casing from the template
                            name = lm.TemplateMap[name];
                        }

                        _ = sb.Append($"                                        {index++} => new global::System.Collections.Generic.KeyValuePair<string, object?>(\"{name}\", this._{p.Name}),\n");
                    }

                    _ = sb.Append($"                                        {index++} => new global::System.Collections.Generic.KeyValuePair<string, object?>(\"{{OriginalFormat}}\", \"{EscapeMessageString(lm.Message)}\"),\n");
                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenCallbackArguments(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.RegularParameters)
                    {
                        _ = sb.Append($"{p.Name}, ");
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenDefineTypes(LoggerMethod lm, bool brackets)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.RegularParameters)
                    {
                        if (sb.Length > 0)
                        {
                            _ = sb.Append(", ");
                        }

                        _ = sb.Append($"{p.Type}");
                    }

                    var result = sb.ToString();
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (brackets)
                        {
                            result = "<" + result + ">";
                        }
                        else
                        {
                            result += ", ";
                        }
                    }

                    return result;
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenParameters(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.AllParameters)
                    {
                        if (sb.Length > 0)
                        {
                            _ = sb.Append(", ");
                        }

                        _ = sb.Append($"{p.Type} {p.Name}");
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenArguments(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.RegularParameters)
                    {
                        if (sb.Length > 0)
                        {
                            _ = sb.Append(", ");
                        }

                        _ = sb.Append($"{p.Type} {p.Name}");
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenHolder(LoggerMethod lm)
            {
                var typeName = $"__{lm.Name}Struct";

                var sb = GetStringBuilder();
                try
                {
                    _ = sb.Append($"new {typeName}(");
                    foreach (var p in lm.RegularParameters)
                    {
                        if (p != lm.RegularParameters[0])
                        {
                            _ = sb.Append(", ");
                        }

                        _ = sb.Append(p.Name);
                    }

                    _ = sb.Append(')');

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenLogMethod(LoggerMethod lm)
            {
                string level = string.Empty;

                if (lm.Level == null)
                {
                    foreach (var p in lm.AllParameters)
                    {
                        if (p.IsLogLevel)
                        {
                            level = p.Name;
                            break;
                        }
                    }
                }
                else
                {
#pragma warning disable S109 // Magic numbers should not be used
                    level = lm.Level switch
                    {
                        0 => "global::Microsoft.Extensions.Logging.LogLevel.Trace",
                        1 => "global::Microsoft.Extensions.Logging.LogLevel.Debug",
                        2 => "global::Microsoft.Extensions.Logging.LogLevel.Information",
                        3 => "global::Microsoft.Extensions.Logging.LogLevel.Warning",
                        4 => "global::Microsoft.Extensions.Logging.LogLevel.Error",
                        5 => "global::Microsoft.Extensions.Logging.LogLevel.Critical",
                        6 => "global::Microsoft.Extensions.Logging.LogLevel.None",
                        _ => $"(global::Microsoft.Extensions.Logging.LogLevel){lm.Level}",
                    };
#pragma warning restore S109 // Magic numbers should not be used
                }

                string eventName;
                if (string.IsNullOrWhiteSpace(lm.EventName))
                {
                    eventName = $"nameof({lm.Name})";
                }
                else
                {
                    eventName = $"\"{lm.EventName}\"";
                }

                string exceptionArg = "null";
                foreach (var p in lm.AllParameters)
                {
                    if (p.IsException)
                    {
                        exceptionArg = p.Name;
                        break;
                    }
                }

                string logger = lm.LoggerField;
                foreach (var p in lm.AllParameters)
                {
                    if (p.IsLogger)
                    {
                        logger = p.Name;
                        break;
                    }
                }

                var extension = (lm.IsExtensionMethod ? "this " : string.Empty);

                if (UseLoggerMessageDefine(lm))
                {
                    return $@"
                            [{_generatedCodeAttribute}]
                            private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, {GenDefineTypes(lm, false)}global::System.Exception?> __{lm.Name}Callback =
                                global::Microsoft.Extensions.Logging.LoggerMessage.Define{GenDefineTypes(lm, true)}({level}, new global::Microsoft.Extensions.Logging.EventId({lm.EventId}, {eventName}), ""{EscapeMessageString(lm.Message)}""); 

                            [{_generatedCodeAttribute}]
                            {lm.Modifiers} void {lm.Name}({extension}{GenParameters(lm)})
                            {{
                                if ({logger}.IsEnabled({level}))
                                {{
                                    __{lm.Name}Callback({logger}, {GenCallbackArguments(lm)}{exceptionArg});
                                }}
                            }}
                        ";
                }
                else
                {
                    return $@"
                            [{_generatedCodeAttribute}]
                            {lm.Modifiers} void {lm.Name}({extension}{GenParameters(lm)})
                            {{
                                if ({logger}.IsEnabled({level}))
                                {{
                                    {logger}.Log(
                                        {level},
                                        new global::Microsoft.Extensions.Logging.EventId({lm.EventId}, {eventName}),
                                        {GenHolder(lm)},
                                        {exceptionArg},
                                        __{lm.Name}Struct.Format);
                                }}
                            }}
                        ";
                }
            }

            private string GenEnumerationHelper(LoggerClass lc)
            {
                foreach (var lm in lc.Methods)
                {
                    if (UseLoggerMessageDefine(lm))
                    {
                        foreach (var p in lm.RegularParameters)
                        {
                            if (p.IsEnumerable)
                            {
                                return $@"
                            [{_generatedCodeAttribute}]
                            private static string __Enumerate(global::System.Collections.IEnumerable? enumerable)
                            {{
                                if (enumerable == null)
                                {{
                                    return ""(null)"";
                                }}

                                var sb = new global::System.Text.StringBuilder();
                                _ = sb.Append('[');

                                bool first = true;
                                foreach (object e in enumerable)
                                {{
                                    if (!first)
                                    {{
                                        _ = sb.Append("", "");
                                    }}

                                    if (e == null)
                                    {{
                                        _ = sb.Append(""(null)"");
                                    }}
                                    else
                                    {{
                                        if (e is global::System.IFormattable fmt)
                                        {{
                                            _ = sb.Append(fmt.ToString(null, global::System.Globalization.CultureInfo.InvariantCulture));
                                        }}
                                        else
                                        {{
                                            _ = sb.Append(e);
                                        }}
                                    }}

                                    first = false;
                                }}

                                _ = sb.Append(']');

                                return sb.ToString();
                            }}
";
                            }
                        }
                    }
                }

                return string.Empty;
            }

            // our own cheezy object pool since we can't use the .NET core version (since this code runs in legacy .NET framework)
            private StringBuilder GetStringBuilder()
            {
                const int DefaultStringBuilderCapacity = 1024;

                if (_builders.Count == 0)
                {
                    return new StringBuilder(DefaultStringBuilderCapacity);
                }

                var sb = _builders.Pop();
                _ = sb.Clear();
                return sb;
            }

            private void ReturnStringBuilder(StringBuilder sb)
            {
                _builders.Push(sb);
            }
        }
    }
}
