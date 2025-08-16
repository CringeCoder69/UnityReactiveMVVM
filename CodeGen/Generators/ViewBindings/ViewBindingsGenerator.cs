using System;
using System.Linq;
using System.Text;
using CodeGen.GeneratorStaff;
using Microsoft.CodeAnalysis;

namespace CodeGen.Generators.ViewBindings
{
    [Generator]
    internal partial class ViewBindingsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ViewBindingsSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is ViewBindingsSyntaxReceiver receiver))
                return;
            try
            {
                foreach (var viewInfo in receiver.Views)
                {
                    if (viewInfo.IsDynamic || !viewInfo.NeedGeneration)
                        continue;

                    if (!receiver.ViewModels.TryGetValue(viewInfo.ViewModelName, out var viewModelInfo))
                        continue;

                    var generationData = new GenerationData
                    {
                        Namespace = viewInfo.Namespace,

                        ClassName = viewInfo.Name,
                        InterfaceName = viewInfo.ViewModelName,
                    };

                    generationData.Usings.Add("R3");

                    Common.CollectProperties(viewModelInfo.Declaration, generationData, receiver.ViewModels, context);
                    HandleProperties(generationData, receiver, context);

                    context.AddSource($"{viewInfo.Name}.bindings.cs", Build(generationData));
                }
            }
            catch (Exception ex)
            {
                context.AddSource("Exception", ex.ToString());
            }
        }

        private void HandleProperties(GenerationData generationData,
            ViewBindingsSyntaxReceiver syntaxReceiver, GeneratorExecutionContext context)
        {
            generationData.Usings.Add("UnityEngine");

            foreach (var property in generationData.ReactiveCommands)
            {
                property.Type = "BaseButtonHolder";
                generationData.Usings.Add("R3");
                generationData.Usings.Add("UnityReactiveMVVM");
            }

            foreach (var property in generationData.Observables)
            {
                property.Type = $"UnityEvent<{property.Type}>";

                var semanticModel = context.Compilation.GetSemanticModel(property.Node.SyntaxTree);
                var propertySymbol = semanticModel.GetDeclaredSymbol(property.Node) as IPropertySymbol;

                string argumentNamespace = ((INamedTypeSymbol)propertySymbol.Type)
                    .TypeArguments[0].ContainingNamespace.ToDisplayString();
                generationData.Usings.Add(argumentNamespace);

                generationData.Usings.Add("R3");
                generationData.Usings.Add("UnityEngine.Events");
            }

            foreach (var property in generationData.NestedViewModelCollections)
            {
                if (syntaxReceiver.ViewModels.TryGetValue(property.Type, out var viewModelInfo))
                {
                    property.Type += "Collection";
                    generationData.Usings.Add($"{viewModelInfo.Namespace}.Collections");
                    generationData.Usings.Add("UnityEngine");
                    generationData.Usings.Add("UnityReactiveMVVM.Utils");
                }
            }

            foreach (var property in generationData.NestedViewModels)
            {
                var viewModelName = property.Type;
                if (syntaxReceiver.ViewModels.TryGetValue(viewModelName, out var viewModelInfo))
                {
                    property.Type = viewModelInfo.View.Name;
                    generationData.Usings.Add(viewModelInfo.View.Namespace);
                }
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

            sb.AppendLine($"    public partial class {data.ClassName}");
            sb.AppendLine("    {");

            var fields = Enumerable.Empty<Property>()
                .Concat(data.NestedViewModels)
                .Concat(data.ReactiveCommands)
                .Concat(data.Observables);

            foreach (var collection in data.NestedViewModelCollections)
                sb.AppendLine($"        [SerializeField] private GameObject {collection.Name};");

            foreach (var field in fields)
                sb.AppendLine($"        [SerializeField] private {field.Type} {field.Name};");

            sb.AppendLine($"        protected override void InitializeNested({data.InterfaceName} viewModel)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.InitializeNested(viewModel);");

            foreach (var viewModel in data.NestedViewModels)
                sb.AppendLine($"            {viewModel.Name}.Initialize(viewModel.{viewModel.Name});");

            foreach (var viewModel in data.NestedViewModelCollections)
                sb.AppendLine($"            {viewModel.Name}.GetComponent<{viewModel.Type}>().Initialize(viewModel.{viewModel.Name});");

            sb.AppendLine("        }");
            sb.AppendLine($"        protected override void BindSelf({data.InterfaceName} viewModel, ref DisposableBag disposableBag)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.BindSelf(viewModel, ref disposableBag);");

            foreach (var button in data.ReactiveCommands)
                sb.AppendLine($"            {button.Name}.Clicked.Subscribe(viewModel.{button.Name}.Execute).AddTo(ref disposableBag);");

            foreach (var observable in data.Observables)
                sb.AppendLine($"            viewModel.{observable.Name}.Subscribe({observable.Name}.Invoke).AddTo(ref disposableBag);");

            sb.AppendLine("        }");
            sb.AppendLine($"        private void OnValidate()");
            sb.AppendLine("        {");

            foreach (var collection in data.NestedViewModelCollections)
                sb.AppendLine($"            ValidationUtils.ValidateComponent<{collection.Type}>(ref {collection.Name});");


            sb.AppendLine("        }");
            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(data.Namespace))
                sb.AppendLine("}");

            return sb.ToString();
        }
    }
}