using System.Collections.Generic;
using CodeGen.SyntaxReceiverStaff;
using Microsoft.CodeAnalysis;

namespace CodeGen.Generators.ViewModelCollection
{
    internal class ViewModelCollectionSyntaxReceiver : ISyntaxReceiver
    {
        public Dictionary<string, ViewModelInfo> ViewModels { get; } = new Dictionary<string, ViewModelInfo>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            SyntaxReceiverUtils.CollectViewModel(syntaxNode, ViewModels);
        }
    }
}
