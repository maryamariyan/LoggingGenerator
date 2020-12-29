// © Microsoft Corporation. All rights reserved.

using Microsoft.CodeAnalysis;

namespace Microsoft.Extensions.Logging.Analyzers
{
    internal static class DiagDescriptors
    {
        public static DiagnosticDescriptor UsingLegacyLoggingMethod { get; } = new (
            id: "LA0000",
            messageFormat: Resources.UsingLegacyMethodMessage,
            title: Resources.UsingLegacyMethodTitle,
            category: "Performance",
            description: Resources.UsingLegacyMethodDescription,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
