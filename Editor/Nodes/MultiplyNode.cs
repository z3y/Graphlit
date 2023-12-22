using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph.Nodes
{ 
    public class MultiplyNode : ShaderNode
    {
        public override void Initialize()
        {
        }

        public override void Draw()
        {
            var textField = new Label { text = "Multiply" };
            titleContainer.Insert(0, textField);

            var portA = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            portA.portName = "A";
            inputContainer.Add(portA);

            var portB = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            portB.portName = "B";
            inputContainer.Add(portB);

            var portC = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            portC.portName = "C";
            outputContainer.Add(portC);
        }
    }
}