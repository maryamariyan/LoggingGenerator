// � Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Extensions.Logging.Generators
{
    public partial class LoggingGenerator
    {
        internal class Emitter
        {
            // The maximum arity of the LogStateHolder-family of types. Beyond this number, parameters are just kepts in an array (which implies an allocation
            // for the array and boxing of all logging method arguments.
            const int MaxStateHolderArity = 6;

            private readonly Stack<StringBuilder> _builders = new();
            private readonly bool _pascalCaseArguments;

            public Emitter(bool pascalCaseArguments)
            {
                _pascalCaseArguments = pascalCaseArguments;
            }

            public string Emit(IReadOnlyList<LoggerClass> logClasses, CancellationToken cancellationToken)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var lc in logClasses)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // stop any additional work
                            break;
                        }

                        sb.Append(GenType(lc));
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenType(LoggerClass lc)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var lm in lc.Methods)
                    {
                        sb.Append(GenFormatFunc(lm));
                        sb.Append(GenNameArray(lm));
                        sb.Append(GenLogMethod(lm));
                    }

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

            private string GenFormatFunc(LoggerMethod lm)
            {
                if (!lm.MessageHasTemplates)
                {
                    return string.Empty;
                }

                var sb = GetStringBuilder();
                try
                {
                    string typeName;
                    if (lm.Parameters.Count == 0)
                    {
                        typeName = $"global::Microsoft.Extensions.Logging.LogStateHolder";
                    }
                    else if (lm.Parameters.Count == 1)
                    {
                        typeName = $"global::Microsoft.Extensions.Logging.LogStateHolder<{lm.Parameters[0].Type}>";
                        sb.Append($"var {lm.Parameters[0].Name} = __holder.Value;\n");
                    }
                    else if (lm.Parameters.Count > MaxStateHolderArity)
                    {
                        typeName = "global::Microsoft.Extensions.Logging.LogStateHolderN";

                        var index = 0;
                        foreach (var p in lm.Parameters)
                        {
                            sb.Append($"var {p.Name} = __holder[{index++}].Value;\n");
                        }
                    }
                    else
                    {
                        sb.Append("global::Microsoft.Extensions.Logging.LogStateHolder<");

                        foreach (var p in lm.Parameters)
                        {
                            if (p != lm.Parameters[0])
                            {
                                sb.Append(", ");
                            }

                            sb.Append(p.Type);
                        }
                        sb.Append('>');
                        typeName = sb.ToString();

                        sb.Clear();
                        var index = 1;
                        foreach (var p in lm.Parameters)
                        {
                            sb.Append($"var {p.Name} = __holder.Value{index++};\n");
                        }
                    }

                    return $@"private static readonly global::System.Func<{typeName}, global::System.Exception?, string> __{lm.Name}FormatFunc = (__holder, _) =>
                        {{
                            {sb}
                            return $""{EscapeMessageString(lm.Message)}"";
                        }};
                ";
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenNameArray(LoggerMethod lm)
            {
                if (lm.Parameters.Count is < 2 or > MaxStateHolderArity)
                {
                    return string.Empty;
                }

                var sb = GetStringBuilder();
                try
                {
                    sb.Append($"private static readonly string[] __{lm.Name}Names = new []{{");
                    foreach (var p in lm.Parameters)
                    {
                        sb.Append($"\"{NormalizeArgumentName(p.Name)}\", ");
                    }

                    sb.Append("};");
                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenLogMethod(LoggerMethod lm)
            {
                string level = lm.Level switch
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
                foreach (var p in lm.Parameters)
                {
                    if (p.IsExceptionType)
                    {
                        exceptionArg = p.Name;
                        break;
                    }
                }

                string formatFunc;
                if (lm.MessageHasTemplates)
                {
                    formatFunc = $"__{lm.Name}FormatFunc";
                }
                else
                {
                    formatFunc = $"(_, _) => \"{EscapeMessageString(lm.Message)}\"";
                }

                return $@"
                    {lm.Modifiers} void {lm.Name}({(lm.IsExtensionMethod ? "this " : string.Empty)}{lm.LoggerType} __logger{(lm.Parameters.Count > 0 ? ", " : string.Empty)}{GenParameters(lm)})
                    {{
                        if (__logger.IsEnabled({level}))
                        {{
                            __logger.Log(
                                {level},
                                new global::Microsoft.Extensions.Logging.EventId({lm.EventId}, {eventName}),
                                {GenHolder(lm, formatFunc)},
                                {exceptionArg},
                                {formatFunc});
                        }}
                    }}
                ";
            }

            private string GenParameters(LoggerMethod lm)
            {
                var sb = GetStringBuilder();
                try
                {
                    foreach (var p in lm.Parameters)
                    {
                        if (p != lm.Parameters[0])
                        {
                            sb.Append(", ");
                        }

                        sb.Append($"{p.Type} {p.Name}");
                    }

                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string GenHolder(LoggerMethod lm, string formatFunc)
            {
                if (lm.Parameters.Count == 0)
                {
                    return "new global::Microsoft.Extensions.Logging.LogStateHolder()";
                }

                if (lm.Parameters.Count == 1)
                {
                    return $"new global::Microsoft.Extensions.Logging.LogStateHolder<{lm.Parameters[0].Type}>({formatFunc}, \"{NormalizeArgumentName(lm.Parameters[0].Name)}\", {lm.Parameters[0].Name})";
                }

                var sb = GetStringBuilder();
                try
                {
                    if (lm.Parameters.Count > MaxStateHolderArity)
                    {
                        sb.Append($"new global::Microsoft.Extensions.Logging.LogStateHolderN({formatFunc}, new global::System.Collections.Generic.KeyValuePair<string, object?>[]{{");
                        foreach (var p in lm.Parameters)
                        {
                            sb.Append($"new(\"{NormalizeArgumentName(p.Name)}\", {p.Name}), ");
                        }

                        sb.Append("})");
                    }
                    else
                    {
                        foreach (var p in lm.Parameters)
                        {
                            if (p != lm.Parameters[0])
                            {
                                sb.Append(", ");
                            }

                            sb.Append(p.Type);
                        }
                        var tp = sb.ToString();

                        sb.Clear();
                        sb.Append($"new global::Microsoft.Extensions.Logging.LogStateHolder<{tp}>({formatFunc}, __{lm.Name}Names, ");
                        foreach (var p in lm.Parameters)
                        {
                            if (p != lm.Parameters[0])
                            {
                                sb.Append(", ");
                            }

                            sb.Append(p.Name);
                        }

                        sb.Append(')');
                    }
                    return sb.ToString();
                }
                finally
                {
                    ReturnStringBuilder(sb);
                }
            }

            private string NormalizeArgumentName(string name)
            {
                if (!_pascalCaseArguments || char.IsUpper(name, 0))
                {
                    return name;
                }

                var sb = GetStringBuilder();
                try
                {
                    sb.Append(char.ToUpperInvariant(name[0]));
                    sb.Append(name, 1, name.Length - 1);
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

            // our own cheezy object pool since we can't use the .NET core version
            private StringBuilder GetStringBuilder()
            {
                if (_builders.Count == 0)
                {
                    return new StringBuilder(1024);
                }

                var sb = _builders.Pop();
                sb.Clear();
                return sb;
            }

            private void ReturnStringBuilder(StringBuilder sb)
            {
                _builders.Push(sb);
            }
        }
    }
}
