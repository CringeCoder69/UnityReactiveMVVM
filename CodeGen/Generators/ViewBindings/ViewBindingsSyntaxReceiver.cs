using System.Collections.Generic;
using CodeGen.SyntaxReceiverStaff;
using Microsoft.CodeAnalysis;

namespace CodeGen.Generators.ViewBindings
{
    public class ViewBindingsSyntaxReceiver : ISyntaxReceiver
    {
        public Dictionary<string, ViewModelInfo> ViewModels { get; } = new Dictionary<string, ViewModelInfo>();
        public List<ViewInfo> Views { get; } = new List<ViewInfo>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            SyntaxReceiverUtils.CollectViewModel(syntaxNode, ViewModels);
            SyntaxReceiverUtils.CollectView(syntaxNode, Views, ViewModels);
        }
    }
}
