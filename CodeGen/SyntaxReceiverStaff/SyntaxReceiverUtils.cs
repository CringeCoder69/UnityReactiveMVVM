using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen.SyntaxReceiverStaff
{
    internal static class SyntaxReceiverUtils
    {
        private const string BaseView = nameof(BaseView);
        private const string BaseDynamicView = nameof(BaseDynamicView);

        public static void CollectViewModel(SyntaxNode syntaxNode,
            IDictionary<string, ViewModelInfo> viewModels)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceNode)
            {
                var interfaceName = interfaceNode.Identifier.ValueText;
                if (interfaceName.EndsWith("ViewModel"))
                {
                    var viewModelInfo = GetOrCreate(interfaceName, viewModels);
                    viewModelInfo.Declaration = interfaceNode;
                    viewModelInfo.Namespace = GetNamespace(interfaceNode);
                }
            }
        }

        public static void CollectView(SyntaxNode syntaxNode, IList<ViewInfo> views,
            IDictionary<string, ViewModelInfo> viewModels)
        {
            if (syntaxNode is ClassDeclarationSyntax classNode)
            {
                if (classNode.HasModifier(SyntaxKind.AbstractKeyword))
                    return;

                if (MatchGenericPattern(classNode, out var baseType, out var argumentType) &&
                    (baseType == BaseView || baseType == BaseDynamicView))
                {
                    var className = classNode.Identifier.ValueText;
                    var viewInfo = new ViewInfo
                    {
                        Name = className,
                        ViewModelName = argumentType,
                        Namespace = GetNamespace(classNode),
                        NeedGeneration = classNode.HasModifier(SyntaxKind.PartialKeyword),
                        IsDynamic = baseType == BaseDynamicView,
                    };

                    views.Add(viewInfo);

                    var viewModelInfo = GetOrCreate(argumentType, viewModels);
                    viewModelInfo.View = viewInfo;
                }
            }
        }

        private static ViewModelInfo GetOrCreate(string key, IDictionary<string, ViewModelInfo> viewModels)
        {
            if (!viewModels.TryGetValue(key, out var viewModel))
                viewModels[key] = viewModel = new ViewModelInfo();
            return viewModel;
        }

        private static bool MatchGenericPattern(ClassDeclarationSyntax classNode,
            out string targetBaseType, out string genericArgument)
        {
            targetBaseType = null;
            genericArgument = null;

            if (classNode.BaseList == null)
                return false;

            foreach (var baseType in classNode.BaseList.Types)
            {
                if (!(baseType.Type is GenericNameSyntax genericNameSyntax))
                    continue;

                if (genericNameSyntax.TypeArgumentList.Arguments.Count != 1)
                    continue;

                var argument = genericNameSyntax.TypeArgumentList.Arguments.First();
                if (!(argument is IdentifierNameSyntax i))
                    continue;

                targetBaseType = genericNameSyntax.Identifier.ValueText;
                genericArgument = i.ToString();
                return true;
            }

            return false;
        }

        public static bool HasModifier(this MemberDeclarationSyntax declaration, SyntaxKind syntaxKind)
        {
            return declaration.Modifiers.Any(m => m.IsKind(syntaxKind));
        }

        public static string GetNamespace(SyntaxNode syntaxNode)
        {
            return syntaxNode.Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault()
                ?.Name.ToString();
        }
    }
}
