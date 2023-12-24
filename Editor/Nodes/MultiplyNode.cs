using UnityEditor.Experimental.GraphView;


namespace z3y.ShaderGraph.Nodes
{
    [@NodeInfo("*", "a * b")]
    public class MultiplyNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "a");
            AddInput(typeof(float), 1, "b");
            AddOutput(typeof(float), 2);
        }
    }

    [@NodeInfo("+", "a + b")]
    public class AddNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "a");
            AddInput(typeof(float), 1, "b");
            AddOutput(typeof(float), 2);
        }
    }

    [@NodeInfo("dot", "dot(a, b)")]
    public class DotNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "a");
            AddInput(typeof(float), 1, "b");
            AddOutput(typeof(float), 2);
        }

        public override void Visit(Port[] ports)
        {
            var a = ports[0];
            var b = ports[1];


            var result = ports[2];
        }
    }

    [@NodeInfo("mad", "mad(a, b, c)")]
    public class MadNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "a");
            AddInput(typeof(float), 1, "b");
            AddInput(typeof(float), 2, "c");
            AddOutput(typeof(float), 3);
        }
    }

    [@NodeInfo("some custom data/test")]
    public class TestNode : ShaderNode
    {
        [UnityEngine.SerializeField] string meow = "meowww";
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "adsfgs");
            AddOutput(typeof(float), 1);
            AddOutput(typeof(float), 2, "cool");
        }
    }
}