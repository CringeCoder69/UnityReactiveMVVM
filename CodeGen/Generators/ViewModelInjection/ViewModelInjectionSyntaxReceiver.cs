using System.Collections.Generic;
using System.Linq;
using CodeGen.SyntaxReceiverStaff;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen.Generators.ViewModelInjection
{
    internal class ViewModelInjectionSyntaxReceiver : ISyntaxReceiver
    {
        public Dictionary<string, ViewModelInfo> ViewModels { get; } = new Dictionary<string, ViewModelInfo>();
        public List<ViewInfo> Views { get; } = new List<ViewInfo>();
        public List<ViewModelClassInfo> ViewModelClasses { get; } = new List<ViewModelClassInfo>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            SyntaxReceiverUtils.CollectViewModel(syntaxNode, ViewModels);
            SyntaxReceiverUtils.CollectView(syntaxNode, Views, ViewModels);

            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if (!classDeclarationSyntax.Identifier.ValueText.EndsWith("ViewModel"))
                    return;

                if (classDeclarationSyntax.BaseList == null)
                    return;

                ViewModelClasses.Add(new ViewModelClassInfo
                {
                    Name = classDeclarationSyntax.Identifier.ValueText,
                    TypeParameters = classDeclarationSyntax.TypeParameterList?.ToString(),
                    Namespace = SyntaxReceiverUtils.GetNamespace(classDeclarationSyntax),
                    Ancestors = classDeclarationSyntax.BaseList.Types.Select(x => x.ToString()),
                    NeedGeneration = classDeclarationSyntax.HasModifier(SyntaxKind.PartialKeyword)
                });
            }
        }
    }
}
