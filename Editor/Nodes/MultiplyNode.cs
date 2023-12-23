using UnityEditor.Experimental.GraphView;


namespace z3y.ShaderGraph.Nodes
{
    [@DisplayName("Multiply"), @Tooltip("C = A * B")]
    public class MultiplyNode : ShaderNode
    {
        public override void AddElements()
        {
            var portA = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            portA.portName = "A";
            Node.inputContainer.Add(portA);

            var portB = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            portB.portName = "B";
            Node.inputContainer.Add(portB);

            var portC = Node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            portC.portName = "C";
            Node.outputContainer.Add(portC);
        }
    }

    [@DisplayName("Add"), @Tooltip("C = A + B")]
    public class AddNode : ShaderNode
    {
        public override void AddElements()
        {
            var portA = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            portA.portName = "A";
            Node.inputContainer.Add(portA);

            var portB = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            portB.portName = "B";
            Node.inputContainer.Add(portB);

            var portC = Node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            portC.portName = "C";
            Node.outputContainer.Add(portC);
        }
    }
}