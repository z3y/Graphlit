using System.Collections.Generic;
using System.Linq;

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
        public NodeVisitor(ShaderBuilder shaderBuilder, ShaderStage stage, int passIndex)
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

            Pass = passIndex;
            Stage = stage;
            _pragmas = shaderBuilder.passBuilders[passIndex].pragmas;
        }

        internal ShaderBuilder _shaderBuilder;

        public GenerationMode GenerationMode => _shaderBuilder.GenerationMode;
        public ShaderStage Stage { get; }
        public int Pass {  get; }

        private List<string> _expression;
        private List<string> _function;
        private List<PropertyDescriptor> _props;
        private List<string> _pragmas;

        /*public string GetInputVariable(int ID)
        {
            if (visitInfo.Name.TryGetValue(ID, out var name))
            {
                return name;
            }
            else
            {
                var newName = "Node" + _uniqueID++;
                visitInfo.Name[ID] = newName;
                return newName;
            }
        }

        public IPortType GetType(int ID) => visitInfo.PortType[ID];
        public Float GetTypeFloat(int ID) => (Float)visitInfo.PortType[ID];

        public void SetVariable(int ID, string name) => visitInfo.Name[ID] = name;
        public void SetOutputType(int ID, IPortType type)
        {
            if (type is Float @float)
            {
                //TODO: inherit
                @float.fullPrecision = true;
            }
            visitInfo.PortType[ID] = type;
        }

        public void OutputExpression(int outID, int lhs, string @operator, int rhs, string prefix = null)
        {
            var name = (prefix ?? "Node") + _uniqueID++;
            SetVariable(outID, name);
            AppendLine($"{GetTypeFloat(outID)} {name} = {GetVariable(lhs)} {@operator} {GetVariable(rhs)};");
        }
*/

        public void AppendLine(string value)
        {
             _expression.Add(value);
        }
        public void AddFunction(string function)
        {
            if (_function.Contains(function))
            {
                return;
            }

            _function.Add(function);
        }
        public void AddPragma(string pragma)
        {
            _pragmas.Add(pragma);
        }
        public void AddProperty(PropertyDescriptor property)
        {
            if (!_props.Any(x => x.GetReferenceName(GenerationMode) == property.GetReferenceName(GenerationMode)))
            {
                _props.Add(property);
            }
        }
    }
}
