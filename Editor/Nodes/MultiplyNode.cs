using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


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
        public override string Visit(int outID)
        {
            var a = GetVariableName(0);
            var b = GetVariableName(1);
            var c = GetVariableName(2);

            return $"float {c} = {a} * {b};";
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

        public override string Visit(int outID)
        {
            var a = GetVariableName(0);
            var b = GetVariableName(1);
            var c = GetVariableName(2);

            return $"float {c} = {a} + {b};";
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
        public override string Visit(int outID)
        {
            var a = GetVariableName(0);
            var b = GetVariableName(1);
            var c = GetVariableName(2);

            return $"float {c} = dot({a}, {b});";
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
        [UnityEngine.SerializeField] string persistentField = "asdfg";
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "adsfgs");
            AddInput(typeof(float), 1, "adsfgs");
            AddOutput(typeof(float), 2);
            AddOutput(typeof(float), 3, "cool");

            var f = new TextField { value = persistentField };
            f.RegisterValueChangedCallback((evt) => {
                persistentField = evt.newValue;
            });
            Node.extensionContainer.Add(f);
        }
    }

    [@NodeInfo("float3")]
    public class Float3Node : ShaderNode
    {
        [SerializeField] Vector3 data;

        public override void AddElements()
        {
            AddOutput(typeof(float), 0);

            var f = new Vector3Field { value = data };
            f.RegisterValueChangedCallback((evt) => {
                data = evt.newValue;
            });
            Node.extensionContainer.Add(f);
        }
    }

    [@NodeInfo("float")]
    public class FloatNode : ShaderNode
    {
        [SerializeField] float data;

        public override void AddElements()
        {
            AddOutput(typeof(float), 0);

            var f = new FloatField { value = data };
            f.RegisterValueChangedCallback((evt) => {
                data = evt.newValue;
            });
            Node.extensionContainer.Add(f);

            /*var b = new Button();
            b.clicked += () => { Debug.Log(EditorJsonUtility.ToJson((FloatNode)this)); };
            Node.extensionContainer.Add(b);*/
        }

        public override string Visit(int outID)
        {
            var c = GetVariableName(0);

            return $"float {c} = {data};";
        }
    }

    [@NodeInfo("Result")]
    public class OutputNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(float), 0, "Result");
        }
    }
}