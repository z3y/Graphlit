using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Modulo")]
    public class FmodNode : SimpleExpressionNode
    {
        protected override string Expression => $"fmod({PortData[A].Name}, {PortData[B].Name})";
    }
}