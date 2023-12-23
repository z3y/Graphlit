using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph.Nodes
{ 
    public class MultiplyNode : ShaderNode
    {
        public override string Title => "Multiply";
        public override string Tooltip => "C = A * B";

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