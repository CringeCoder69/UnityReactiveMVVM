using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGen.GeneratorStaff;
using CodeGen.SyntaxReceiverStaff;
using Microsoft.CodeAnalysis;

namespace CodeGen.Generators.ViewModelInjection
{
    [Generator]
    internal class ViewModelInjectionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ViewModelInjectionSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is ViewModelInjectionSyntaxReceiver receiver))
                return;

            try
            {
                foreach (var viewModelClassInfo in receiver.ViewModelClasses)
                {
                    if (!viewModelClassInfo.NeedGeneration)
                        continue;

                    if (receiver.ViewModels.TryGetValue(viewModelClassInfo.Ancestor, out var viewModelInfo))
                    {
                        var generationData = new GenerationData
                        {
                            Namespace = viewModelClassInfo.Namespace,
                            ClassName = viewModelClassInfo.Name + viewModelClassInfo.TypeParameters,
                        };

                        Common.CollectProperties(viewModelInfo.Declaration, generationData, receiver.ViewModels, context);
                        HandleProperties(generationData, receiver.ViewModels);

                        context.AddSource($"{viewModelClassInfo.Name}.injection.cs", Build(generationData));
                    }
                }
            }
            catch (Exception ex)
            {
                context.AddSource("Exception", ex.ToString());
            }
        }

        private void HandleProperties(GenerationData generationData, IDictionary<string, ViewModelInfo> viewModels)
        {
            var allViewModels = Enumerable.Empty<Property>()
                .Concat(generationData.NestedViewModelCollections)
                .Concat(generationData.NestedViewModels);

            if (generationData.NestedViewModelCollections.Any())
                generationData.Usings.Add("ObservableCollections");

            for (int i = generationData.NestedViewModels.Count - 1; i >= 0; i--)
            {
                var property = generationData.NestedViewModels[i];
                if (viewModels.TryGetValue(property.Type, out var viewModelInfo))
                    if (viewModelInfo.View.IsDynamic)
                        generationData.NestedViewModels.RemoveAt(i);
            }

            foreach (var property in allViewModels)
                if (viewModels.TryGetValue(property.Type, out var viewModelInfo))
                    generationData.Usings.Add(viewModelInfo.Namespace);
        }

        private string Build(GenerationData data)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// Auto-generated code");
            sb.AppendLine("using VContainer;");

            Common.PrintUsings(data, sb);

            if (!string.IsNullOrEmpty(data.Namespace))
            {
                sb.AppendLine($"namespace {data.Namespace}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public partial class {data.ClassName}");
            sb.AppendLine("    {");

            foreach (var property in data.NestedViewModelCollections)
            {
                sb.AppendLine($"        [Inject] public IObservableCollection<{property.Type}> {property.Name} {{ get; set; }}");
            }

            foreach (var property in data.NestedViewModels)
            {
                sb.AppendLine($"        [Inject] public {property.Type} {property.Name} {{ get; set; }}");
            }

            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(data.Namespace))
                sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
