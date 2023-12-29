using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph.Nodes
{
    [@NodeInfo("*", "a * b")]
    public sealed class MultiplyNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(1, true), A, "A");
            AddPort(Direction.Input, new PortType.Float(1, true), B, "B");
            AddPort(Direction.Output, new PortType.Float(1, true), OUT);
        }

        public override void Visit(StringBuilder sb)
        {
            var components = ImplicitTruncation(new[] { A, B }, OUT);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            AppendOutputLine(sb, OUT, "Multiply", $"{a} * {b}");
        }

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }
    }

    [@NodeInfo("+", "a + b")]
    public sealed class AddNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(1, true), A, "A");
            AddPort(Direction.Input, new PortType.Float(1, true), B, "B");
            AddPort(Direction.Output, new PortType.Float(1, true), OUT);
        }

        public override void Visit(StringBuilder sb)
        {
            var components = ImplicitTruncation(new[] { A, B }, OUT);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            AppendOutputLine(sb, OUT, "Add", $"{a} + {b}");
        }
    }

    [@NodeInfo("dot", "dot(a, b)")]
    public sealed class DotNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(1, true), A, "A");
            AddPort(Direction.Input, new PortType.Float(1, true), B, "B");
            AddPort(Direction.Output, new PortType.Float(1), OUT);
        }

        public override void Visit(StringBuilder sb)
        {
            var components = ImplicitTruncation(new[] {A, B} );
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            AppendOutputLine(sb, OUT, "Dot", $"dot({a}, {b})");
        }

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }
    }

    [@NodeInfo("swizzle")]
    public sealed class SwizzleNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;
        [SerializeField] string swizzle = "x";

        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(1, true), IN);
            AddPort(Direction.Output, new PortType.Float(1, true), OUT);
        }

        public override void AddVisualElements()
        {
            var f = new TextField { value = swizzle };
            f.RegisterValueChangedCallback((evt) => {
                swizzle = evt.newValue;
            });
            Node.extensionContainer.Add(f);
        }

        public override void Visit(StringBuilder sb)
        {
            int components = swizzle.Length;
            var a = GetInputString(IN);
            PortNames[OUT] = "(" + a + ")." + swizzle;
            PortsTypes[OUT] = new PortType.Float(components);
            PortsTypes[IN] = PortsTypes[OUT];
        }
    }

    [@NodeInfo("float4")]
    public sealed class Float4Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] Vector4 value;

        public override void Initialize()
        {
            AddPort(Direction.Output, new PortType.Float(4), OUT);
        }

        public override void AddVisualElements()
        {
            var f = new Vector4Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }
        public override void Visit(StringBuilder sb)
        {
            PortNames[0] = "float4" + value.ToString("R");
        }
    }
    [@NodeInfo("float3")]
    public sealed class Float3Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] Vector3 value;

        public override void Initialize()
        {
            AddPort(Direction.Output, new PortType.Float(3), OUT);
        }

        public override void AddVisualElements()
        {
            var f = new Vector3Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(StringBuilder sb)
        {
            PortNames[0] = "float3" + value.ToString("R");
        }
    }

    [@NodeInfo("float2")]
    public sealed class Float2Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] Vector2 value;

        public override void Initialize()
        {
            AddPort(Direction.Output, new PortType.Float(2), OUT);
        }

        public override void AddVisualElements()
        {
            var f = new Vector2Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(StringBuilder sb)
        {
            PortNames[0] = "float2" + value.ToString("R");
        }
    }

    [@NodeInfo("float")]
    public sealed class FloatNode : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] float value;

        public override void Initialize()
        {
            AddPort(Direction.Output, new PortType.Float(1), OUT);
        }

        public override void AddVisualElements()
        {
            var f = new FloatField { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(StringBuilder sb)
        {
            PortNames[0] = value.ToString("R");
        }
    }

    [@NodeInfo("Result")]
    public sealed class OutputNode : ShaderNode
    {
        const int IN = 0;

        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(4), IN);
        }
        public override void Visit(StringBuilder sb)
        {
            var col = GetCastInputString(IN, 4);
            sb.AppendLine($"col = {col};");
        }
    }
}