using System.Collections.Generic;

namespace CodeGen.GeneratorStaff
{
    public class GenerationData
    {
        public string Namespace { get; set; }

        public string ClassName { get; set; }
        public string InterfaceName { get; set; }

        public List<Property> NestedViewModels { get; } = new List<Property>();
        public List<Property> NestedViewModelCollections { get; } = new List<Property>();
        public List<Property> ReactiveCommands { get; } = new List<Property>();
        public List<Property> Observables { get; } = new List<Property>();

        public HashSet<string> Usings { get; } = new HashSet<string>();
    }
}