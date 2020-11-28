// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Operations;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// Tracks a bunch of metadata about a potential fix to apply
    /// </summary>
    class FixDetails
    {
        public readonly int MessageParamIndex;
        public readonly int ExceptionParamIndex;
        public readonly int EventIdParamIndex;
        public readonly int LogLevelParamIndex;
        public readonly int ArgsIndex;
        public readonly string Message = string.Empty;
        public readonly string Level = string.Empty;
        public readonly string TargetFilename;
        public readonly string? TargetNamespace;
        public readonly string TargetClassName;
        public readonly string TargetMethodName;

        public FixDetails(IMethodSymbol method, IInvocationOperation invocation, string? defaultNamespace)
        {
            (MessageParamIndex, ExceptionParamIndex, EventIdParamIndex, LogLevelParamIndex, ArgsIndex) = IdentifyParameters(method);

            var lit = invocation.Arguments[MessageParamIndex].Descendants().First() as ILiteralOperation;
            if (lit != null)
            {
                Message = (lit.ConstantValue.Value as string)!;
            }

            if (method.Name == "Log")
            {
                lit = invocation.Arguments[LogLevelParamIndex].Descendants().First() as ILiteralOperation;
                if (lit != null)
                {
                    Level = ((int)(lit.ConstantValue.Value!)) switch
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

            TargetFilename = "Log.cs";
            TargetNamespace = defaultNamespace;
            TargetClassName = "Log";
            TargetMethodName = DeriveName(Message);
        }

        public string FullTargetClassName
        {
            get
            {
                if (TargetNamespace != null)
                {
                    return $"{TargetNamespace}.{TargetClassName}";
                }

                return TargetClassName;
            }
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
        /// Given a logging template string, generate a reasonable logging method name
        /// </summary>
        private static string DeriveName(string message)
        {
            var sb = new StringBuilder();
            bool capitalizeNext = true;
            foreach (var ch in message)
            {
                if (char.IsLetter(ch) || (sb.Length > 1 && char.IsLetterOrDigit(ch)))
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
    }
}
