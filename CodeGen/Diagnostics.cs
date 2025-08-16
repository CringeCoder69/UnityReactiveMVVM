using Microsoft.CodeAnalysis;

namespace CodeGen
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor UnsupportedMemberType = new DiagnosticDescriptor(
            "CG001",
            "Unsupported member type",
            "Only properties are supported for code generation.",
            "CodeGeneration",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor UnsupportedPropertyType = new DiagnosticDescriptor(
            "CG002",
            "Unsupported property type",
            "Unsupported property type for code generation",
            "CodeGeneration",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor InvalidGenericItemType = new DiagnosticDescriptor(
            "CG003",
            "Invalid collection item type",
            "Collection contains items which is not a registered view model.",
            "CodeGeneration",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor OnlyDynamicViewModel = new DiagnosticDescriptor(
            "CG004",
            "Require dynamic view model.",
            "Only dynamic view model allowed here.",
            "CodeGeneration",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
