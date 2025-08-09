namespace CodeGen.SyntaxReceiverStaff
{
    public class ViewModelClassInfo
    {
        public string Name { get; set; }
        public string TypeParameters { get; set; }
        public string Namespace { get; set; }
        public string Ancestor { get; set; }
        public bool NeedGeneration { get; set; }
    }
}
