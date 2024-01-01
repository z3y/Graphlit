namespace z3y.ShaderGraph
{
    public enum GenerationMode
    {
        Preview,
        Final
    }

    public enum GenerationStage
    {
        Vertex,
        Fragment
    }
    public class NodeVisitor
    {
        public NodeVisitor(ShaderBuilder sb)
        {
            _sb = sb;
        }
        private ShaderBuilder _sb;
        public GenerationMode GenerationMode { get; set; }
        public GenerationStage GenerationStage { get; set; }
        public int PassIndex { get; private set; }

        public void AddProperty(string property)
        {
            _sb.properties.Add(property);
        }

        public void AppendLine(string value)
        {
            if (GenerationStage == GenerationStage.Vertex)
            {
                _sb.passBuilders[PassIndex].vertexDescription.Add(value);
            }
            else if (GenerationStage == GenerationStage.Fragment)
            {
                _sb.passBuilders[PassIndex].surfaceDescription.Add(value);
            }
        }
    }
}
