using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen.GeneratorStaff
{
    public class Property
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public PropertyDeclarationSyntax Node { get; set; }
    }
}