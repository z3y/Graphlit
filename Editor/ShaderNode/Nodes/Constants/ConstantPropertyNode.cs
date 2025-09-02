
using System.Linq;
using System.Security.Cryptography;

namespace Graphlit
{
    public abstract class ConstantPropertyNode : ShaderNode
    {
        protected string GetSuggestedPropertyName()
        {
            var output = Outputs.First();

            if (output.connected)
            {
                var conn = output.connections.First();
                var connectedNode = conn.input.node as ShaderNode;
                var propName = connectedNode.TitleLabel.text;
                return propName + " " + conn.input.portName;
            }

            return string.Empty;
        }
    }
}