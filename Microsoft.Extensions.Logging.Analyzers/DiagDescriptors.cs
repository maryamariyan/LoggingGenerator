// © Microsoft Corporation. All rights reserved.

using Microsoft.CodeAnalysis;

namespace Microsoft.Extensions.Logging.Analyzers
{
    static class DiagDescriptors
    {
        public static DiagnosticDescriptor UsingLegacyLoggingMethod { get; } = new (
            id: "LA0000",
            title: Resources.UsingLegacyMethodTitle,
            messageFormat: Resources.UsingLegacyMethodMessage,
            category: "Performance",
            description: Resources.UsingLegacyMethodDescription,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
