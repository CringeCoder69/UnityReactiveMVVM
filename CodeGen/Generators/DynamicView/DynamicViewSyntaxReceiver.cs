using System.Collections.Generic;
using CodeGen.SyntaxReceiverStaff;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen.Generators.DynamicView
{
    internal class DynamicViewSyntaxReceiver : ISyntaxReceiver
    {
        public Dictionary<string, ViewModelInfo> ViewModels { get; } = new Dictionary<string, ViewModelInfo>();
        public List<ViewInfo> Views { get; } = new List<ViewInfo>();

        public Dictionary<string /*Ancestor*/, HashSet<string>> ViewModelInheritance { get; } =
            new Dictionary<string, HashSet<string>>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            SyntaxReceiverUtils.CollectViewModel(syntaxNode, ViewModels);
            SyntaxReceiverUtils.CollectView(syntaxNode, Views, ViewModels);

            CheckViewModelInheritance(syntaxNode);
        }

        private void CheckViewModelInheritance(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax))
                return;

            var currentInterfaceName = interfaceDeclarationSyntax.Identifier.ValueText;
            if (!ViewModelInheritance.ContainsKey(currentInterfaceName))
                ViewModelInheritance.Add(currentInterfaceName, new HashSet<string>());

            if (interfaceDeclarationSyntax.BaseList == null)
                return;

            foreach (var baseTypeSyntax in interfaceDeclarationSyntax.BaseList.Types)
            {
                var ancestorName = baseTypeSyntax.Type.ToString();
                if (!ViewModelInheritance.TryGetValue(ancestorName, out var inheritors))
                    ViewModelInheritance[ancestorName] = inheritors = new HashSet<string>();

                inheritors.Add(currentInterfaceName);
            }
        }
    }
}
