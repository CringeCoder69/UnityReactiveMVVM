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

                    var generationData = new GenerationData
                    {
                        Namespace = viewModelClassInfo.Namespace,
                        ClassName = viewModelClassInfo.Name + viewModelClassInfo.TypeParameters,
                    };

                    foreach (var ancestor in viewModelClassInfo.Ancestors)
                    {
                        if (!receiver.ViewModels.TryGetValue(ancestor, out var viewModelInfo))
                            continue;

                        Common.CollectProperties(viewModelInfo.Declaration, generationData, receiver.ViewModels, context);
                    }

                    if (!HandleProperties(generationData, receiver.ViewModels))
                        continue;

                    context.AddSource($"{viewModelClassInfo.Name}.injection.cs", Build(generationData));
                }
            }
            catch (Exception ex)
            {
                context.AddSource("Exception", ex.ToString());
            }
        }

        private bool HandleProperties(GenerationData generationData, IDictionary<string, ViewModelInfo> viewModels)
        {
            var allViewModels = Enumerable.Empty<Property>()
                .Concat(generationData.NestedViewModelCollections)
                .Concat(generationData.NestedViewModels)
                .GetDistinctByKeyFirst(p => p.Name);

            if (!allViewModels.Any())
                return false;

            foreach (var property in allViewModels)
                if (viewModels.TryGetValue(property.Type, out var viewModelInfo))
                    generationData.Usings.Add(viewModelInfo.Namespace);

            if (generationData.NestedViewModelCollections.Any())
                generationData.Usings.Add("ObservableCollections");

            foreach (var property in generationData.NestedViewModels)
                if (viewModels.TryGetValue(property.Type, out var viewModelInfo) && viewModelInfo.View.IsDynamic)
                {
                    property.Type = $"Observable<{property.Type}>";
                    generationData.Usings.Add("R3");
                }

            return true;
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

    public static class CollectionUtils
    {
        public static IEnumerable<T> GetDistinctByKeyFirst<T, TKey>(
            this IEnumerable<T> items, Func<T, TKey> keySelector)
        {
            return items
                .GroupBy(keySelector)
                .Select(group => group.First());
        }
    }
}
