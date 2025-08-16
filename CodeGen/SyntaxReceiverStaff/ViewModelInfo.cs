using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen.SyntaxReceiverStaff
{
    public class ViewModelInfo
    {
        public string Namespace { get; set; }
        public InterfaceDeclarationSyntax Declaration { get; set; }
        public ViewInfo View { get; set; }
    }
}
