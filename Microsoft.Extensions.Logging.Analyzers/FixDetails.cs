// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.Extensions.Logging.Analyzers
{
    /// <summary>
    /// Tracks a bunch of metadata about a potential fix to apply
    /// </summary>
    class FixDetails
    {
        public readonly int MessageParamIndex;
        public readonly int ExceptionParamIndex;
        public readonly int EventIdParamIndex;
        public readonly int LogLevelParamIndex;
        public readonly int ArgsParamIndex;
        public readonly string Message = string.Empty;
        public readonly string Level = string.Empty;
        public readonly string TargetFilename;
        public readonly string TargetNamespace;
        public readonly string TargetClassName;
        public readonly string TargetMethodName;
        public readonly IReadOnlyList<string> MessageArgs;

        public FixDetails(
            IMethodSymbol method,
            IInvocationOperation invocationOp,
            string? defaultNamespace,
            IEnumerable<Document> docs)
        {
            (MessageParamIndex, ExceptionParamIndex, EventIdParamIndex, LogLevelParamIndex, ArgsParamIndex) = IdentifyParameters(method);

            if (MessageParamIndex >= 0)
            {
                var op = invocationOp.Arguments[MessageParamIndex].Descendants().SingleOrDefault(x => x.Kind == OperationKind.Literal || x.Kind == OperationKind.FieldReference);
                switch (op)
                {
                    case ILiteralOperation lit:
                        Message = lit.ConstantValue.Value as string ?? string.Empty;
                        break;

                    case IFieldReferenceOperation fieldRef:
                        Message = fieldRef.ConstantValue.Value as string ?? string.Empty;
                        break;
                }
            }

            if (LogLevelParamIndex > 0)
            {
                object? value = null;

                var op = invocationOp.Arguments[LogLevelParamIndex].Descendants().SingleOrDefault(x => x.Kind == OperationKind.Literal || x.Kind == OperationKind.FieldReference);
                switch (op)
                {
                    case ILiteralOperation lit:
                        value = lit.ConstantValue.Value;
                        break;

                    case IFieldReferenceOperation fieldRef:
                        value = fieldRef.ConstantValue.Value;
                        break;
                }

                if (value is int)
                {
                    Level = (int)value switch
                    {
                        0 => "Trace",
                        1 => "Debug",
                        2 => "Information",
                        3 => "Warning",
                        4 => "Error",
                        5 => "Critical",
                        _ => string.Empty,
                    };
                }
            }
            else
            {
                Level = method.Name.Substring(3);
            };

            TargetFilename = FindUniqueFilename(docs);
            TargetNamespace = defaultNamespace ?? string.Empty;
            TargetClassName = "Log";
            TargetMethodName = DeriveName(Message);
            MessageArgs = ExtractTemplateArgs(Message);
        }

        public string FullTargetClassName
        {
            get
            {
                if (string.IsNullOrEmpty(TargetNamespace))
                {
                    return TargetClassName;
                }

                return $"{TargetNamespace}.{TargetClassName}";
            }
        }

        private static string FindUniqueFilename(IEnumerable<Document> docs)
        {
            var targetName = "Log.cs";
            int count = 2;
            bool duplicate;
            do
            {
                duplicate = false;
                foreach (var doc in docs)
                {
                    if (string.Equals(doc.Name, targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        duplicate = true;
                        targetName = $"Log{count}.cs";
                        count++;
                        break;
                    }
                }
            } while (duplicate);

            return targetName;
        }

        /// <summary>
        /// Finds the position of the well-known parameters of legacy logging methods. 
        /// </summary>
        /// <returns>-1 for any parameter not present in the given overload</returns>
        private static (int message, int exception, int eventId, int logLevel, int args) IdentifyParameters(IMethodSymbol method)
        {
            var message = -1;
            var exception = -1;
            var eventId = -1;
            var logLevel = -1;
            var args = -1;

            int index = 0;
            foreach (var p in method.Parameters)
            {
                switch (p.Name)
                {
                    case "message": message = index; break;
                    case "exception": exception = index; break;
                    case "eventId": eventId = index; break;
                    case "logLevel": logLevel = index; break;
                    case "args": args = index; break;
                }
                index++;
            }

            return (message, exception, eventId, logLevel, args);
        }

        /// <summary>
        /// Given a logging message with template args, generate a reasonable logging method name
        /// </summary>
        private static string DeriveName(string message)
        {
            var sb = new StringBuilder();
            bool capitalizeNext = true;
            foreach (var ch in message)
            {
                if (char.IsLetter(ch) || (char.IsLetterOrDigit(ch) && sb.Length > 1))
                {
                    if (capitalizeNext)
                    {
                        sb.Append(char.ToUpperInvariant(ch));
                        capitalizeNext = false;
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    capitalizeNext = true;
                }
            }

            return sb.ToString();
        }

        static readonly char[] _formatDelimiters = { ',', ':' };

        /// <summary>
        /// Finds the template arguments contained in the message string
        /// </summary>
        private static List<string> ExtractTemplateArgs(string message)
        {
            var args = new List<string>();
            var scanIndex = 0;
            var endIndex = message.Length;

            while (scanIndex < endIndex)
            {
                var openBraceIndex = FindBraceIndex(message, '{', scanIndex, endIndex);
                var closeBraceIndex = FindBraceIndex(message, '}', openBraceIndex, endIndex);

                if (closeBraceIndex == endIndex)
                {
                    scanIndex = endIndex;
                }
                else
                {
                    // Format item syntax : { index[,alignment][ :formatString] }.
                    var formatDelimiterIndex = FindIndexOfAny(message, _formatDelimiters, openBraceIndex, closeBraceIndex);

                    args.Add(message.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
                    scanIndex = closeBraceIndex + 1;
                }
            }

            return args;
        }

        private static int FindBraceIndex(string message, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            var braceIndex = endIndex;
            var scanIndex = startIndex;
            var braceOccurrenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurrenceCount > 0 && message[scanIndex] != brace)
                {
                    if (braceOccurrenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
                        braceOccurrenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (message[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurrenceCount == 0)
                        {
                            // For '}' pick the first occurrence.
                            braceIndex = scanIndex;
                        }
                    }
                    else
                    {
                        // For '{' pick the last occurrence.
                        braceIndex = scanIndex;
                    }

                    braceOccurrenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static int FindIndexOfAny(string message, char[] chars, int startIndex, int endIndex)
        {
            var findIndex = message.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return findIndex == -1 ? endIndex : findIndex;
        }
    }
}
