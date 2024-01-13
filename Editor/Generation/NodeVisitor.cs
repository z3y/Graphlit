using System.Collections.Generic;

namespace ZSG
{
    public enum GenerationMode
    {
        Preview,
        Final
    }

    public enum ShaderStage
    {
        Vertex,
        Fragment
    }

    public class NodeVisitor
    {
        public NodeVisitor(ShaderBuilder shaderBuilder, ShaderStage stage, int passIndex, string outputStruct)
        {
            _shaderBuilder = shaderBuilder;
            _function = shaderBuilder.passBuilders[passIndex].functions;
            _props = shaderBuilder.passBuilders[passIndex].properties;

            if (stage == ShaderStage.Vertex)
            {
                _expression = _shaderBuilder.passBuilders[passIndex].vertexDescription;
            }
            else if (stage == ShaderStage.Fragment)
            {
                _expression = _shaderBuilder.passBuilders[passIndex].surfaceDescription;
            }

            _expression.Add($"{outputStruct} output = ({outputStruct})0;");

            Pass = passIndex;
            Stage = stage;
        }

        internal ShaderBuilder _shaderBuilder;
        public GenerationMode GenerationMode => _shaderBuilder.GenerationMode;
        public ShaderStage Stage { get; }
        public int Pass {  get; }

        private List<string> _expression;
        private HashSet<string> _function;
        private HashSet<PropertyDescriptor> _props;

        public void AppendLine(string value)
        {
            _expression.Add(value);
        }
        public void AddFunction(string function)
        {
            _function.Add(function);
        }
        public void AddProperty(PropertyDescriptor property)
        {
            _props.Add(property);
        }
    }
}
