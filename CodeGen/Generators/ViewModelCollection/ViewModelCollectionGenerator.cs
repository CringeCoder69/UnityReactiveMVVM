using System;
using System.Linq;
using System.Text;
using CodeGen.GeneratorStaff;
using Microsoft.CodeAnalysis;

namespace CodeGen.Generators.ViewModelCollection
{
    [Generator]
    internal class ViewModelCollectionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ViewModelCollectionSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is ViewModelCollectionSyntaxReceiver receiver))
                return;

            try
            {
                foreach (var kvp in receiver.ViewModels)
                {
                    var viewModelName = kvp.Key;
                    var generationData = new GenerationData
                    {
                        ClassName = viewModelName + "Collection",
                        InterfaceName = viewModelName,
                        Namespace = $"{kvp.Value.Namespace}.Collections",
                    };

                    generationData.Usings.Add("ObservableCollections");
                    context.AddSource($"{generationData.ClassName}.cs", Build(generationData));
                }
            }
            catch (Exception ex)
            {
                context.AddSource("Exception", ex.ToString());
            }
        }

        private string Build(GenerationData data)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// Auto-generated code");

            Common.PrintUsings(data, sb);

            if (!string.IsNullOrEmpty(data.Namespace))
            {
                sb.AppendLine($"namespace {data.Namespace}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public interface {data.ClassName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        void Initialize(IObservableCollection<{data.InterfaceName}> collection);");
            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(data.Namespace))
                sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
