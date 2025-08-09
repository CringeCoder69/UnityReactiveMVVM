namespace CodeGen.SyntaxReceiverStaff
{
    public class ViewInfo
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string ViewModelName { get; set; }
        public bool NeedGeneration { get; set; }
        public bool IsDynamic { get; set; }
    }
}
