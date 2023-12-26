using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Networking.UnityWebRequest;


namespace z3y.ShaderGraph.Nodes
{
    [@NodeInfo("*", "a * b")]
    public class MultiplyNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2);
        }
        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetVariableName(0);
            var b = GetVariableName(1);

            var result = GetVariableName(2, "multiply");
            int components = Mathf.Max(((PortType.DynamicFloat)portTypes[0]).components, ((PortType.DynamicFloat)portTypes[1]).components);
            portTypes[2] = new PortType.DynamicFloat(components);
            sb.AppendLine($"float{components} {result} = {a} * {b};");
        }
    }

    [@NodeInfo("+", "a + b")]
    public class AddNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetVariableName(0);
            var b = GetVariableName(1);

            var result = GetVariableName(2, "add");
            int components = Mathf.Max(((PortType.DynamicFloat)portTypes[0]).components, ((PortType.DynamicFloat)portTypes[1]).components);
            portTypes[2] = new PortType.DynamicFloat(components);
            sb.AppendLine($"float{components} {result} = {a} + {b};");
        }
    }

    [@NodeInfo("dot", "dot(a, b)")]
    public class DotNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2);
        }
        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetVariableName(0);
            var b = GetVariableName(1);

            var result = GetVariableName(2, "dot");
            int components = Mathf.Max(((PortType.DynamicFloat)portTypes[0]).components, ((PortType.DynamicFloat)portTypes[1]).components);
            portTypes[2] = new PortType.DynamicFloat(components);
            sb.AppendLine($"float{components} {result} = dot({a}, {b});");
        }
    }

    [@NodeInfo("swizzle")]
    public class SwizzleNode : ShaderNode
    {
        [UnityEngine.SerializeField] string swizzle = "x";
        public override void AddElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0);
            AddOutput(typeof(PortType.DynamicFloat), 1);

            var f = new TextField { value = swizzle };
            f.RegisterValueChangedCallback((evt) => {
                swizzle = evt.newValue;
            });
            Node.extensionContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetVariableName(0);
            if (a.EndsWith(")"))
            {
                varibleNames[1] = a + "." + swizzle;
            }
            else
            {
                varibleNames[1] = "(" + a + ")." + swizzle;
            }
            portTypes[1] = new PortType.DynamicFloat(swizzle.Length);
        }
    }

    [@NodeInfo("float3")]
    public class Float3Node : ShaderNode
    {
        [SerializeField] Vector3 value;

        public override void AddElements()
        {
            AddOutput(typeof(PortType.DynamicFloat), 0);

            var f = new Vector3Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.extensionContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] =  "float3" + value.ToString("R");
            portTypes[0] = new PortType.DynamicFloat(3);
        }
    }

    [@NodeInfo("float")]
    public class FloatNode : ShaderNode
    {
        [SerializeField] float value;

        public override void AddElements()
        {
            AddOutput(typeof(PortType.DynamicFloat), 0);

            var f = new FloatField { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.extensionContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] = value.ToString("R");
            portTypes[0] = new PortType.DynamicFloat(1);
        }
    }

    [@NodeInfo("Result")]
    public class OutputNode : ShaderNode
    {
        public override void AddElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "Result");
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            sb.AppendLine($"col = {GetVariableName(0)};");
        }
    }
}