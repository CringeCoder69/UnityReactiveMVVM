using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGen.SyntaxReceiverStaff;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen.GeneratorStaff
{
    internal static class Common
    {
        public static void PrintUsings(GenerationData data, StringBuilder sb)
        {
            data.Usings.Remove(data.Namespace);
            foreach (var u in data.Usings.Where(s => !string.IsNullOrEmpty(s)).OrderBy(s => s))
                sb.AppendLine($"using {u};");
        }

        public static void CollectProperties(InterfaceDeclarationSyntax viewModel, GenerationData generationData,
            IDictionary<string, ViewModelInfo> viewModels, GeneratorExecutionContext context)
        {
            foreach (var member in viewModel.Members)
            {
                try
                {
                    if (!(member is PropertyDeclarationSyntax property))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnsupportedMemberType,
                            member.GetLocation()));
                        continue;
                    }

                    var prop = new Property
                    {
                        Name = property.Identifier.ValueText,
                        Node = property
                    };

                    if (property.Type is GenericNameSyntax genericNameSyntax)
                    {
                        var genericTypeName = genericNameSyntax.Identifier.ValueText;
                        if (genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
                        {
                            var argumentType = genericNameSyntax.TypeArgumentList.Arguments.First();
                            if (!(argumentType is GenericNameSyntax))
                            {
                                prop.Type = argumentType.ToString();
                                if (genericTypeName == "IObservableCollection") // view models collection
                                {
                                    if (viewModels.ContainsKey(prop.Type))
                                    {
                                        generationData.NestedViewModelCollections.Add(prop);
                                    }
                                    else
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InvalidGenericItemType,
                                            argumentType.GetLocation()));
                                    }
                                    continue;
                                }
                                else if (genericTypeName == "Observable")
                                {
                                    if (viewModels.TryGetValue(prop.Type, out var info))
                                    {
                                        if (info.View.IsDynamic)
                                        {
                                            generationData.NestedViewModels.Add(prop); // dynamic view model
                                        }
                                        else
                                        {
                                            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.OnlyDynamicViewModel,
                                                argumentType.GetLocation()));
                                        }
                                    }
                                    else
                                    {
                                        generationData.Observables.Add(prop); // common property
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    else if (property.Type is IdentifierNameSyntax identifierNameSyntax)
                    {
                        var propertyType = identifierNameSyntax.Identifier.ValueText;
                        if (viewModels.ContainsKey(propertyType)) // view model
                        {
                            prop.Type = propertyType;
                            generationData.NestedViewModels.Add(prop);
                            continue;
                        }
                        else if (propertyType == "ReactiveCommand") // button
                        {
                            generationData.ReactiveCommands.Add(prop);
                            continue;
                        }
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnsupportedPropertyType,
                        property.Type.GetLocation()));

                }
                catch (Exception ex)
                {
                    throw new Exception($"Processing '{member}' failed.", ex);
                }
            }
        }
    }
}
