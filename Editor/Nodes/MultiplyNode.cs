using System;
using System.Runtime.Serialization;
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

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }

        public override void Visit(NodeVisitor visitor)
        {
            var components = ImplicitTruncation(OUT, A, B);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            visitor.AppendLine(FormatOutput(OUT, "Multiply", $"{a} * {b}"));
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

        public override void Visit(NodeVisitor visitor)
        {
            var components = ImplicitTruncation(OUT, A, B);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            visitor.AppendLine(FormatOutput(OUT, "Add", $"{a} + {b}"));
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

        public override void Visit(NodeVisitor visitor)
        {
            var components = ImplicitTruncation(null, A, B);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            visitor.AppendLine(FormatOutput(OUT, "Dot", $"dot({a}, {b})"));
        }

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }
    }

    [@NodeInfo("swizzle"), Serializable]
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
            f.RegisterValueChangedCallback((evt) =>
            {
                swizzle = Swizzle.ValidateSwizzle(evt, f);
            });
            Node.extensionContainer.Add(f);
        }

        public override void Visit(NodeVisitor visitor)
        {
            int components = swizzle.Length;
            var a = GetInputString(IN);
            PortNames[OUT] = "(" + a + ")." + swizzle;
            PortsTypes[OUT] = new PortType.Float(components);
            PortsTypes[IN] = PortsTypes[OUT];
        }
    }

    [@NodeInfo("float4"), Serializable]
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
        public override void Visit(NodeVisitor visitor)
        {
            PortNames[0] = "float4" + value.ToString("R");
        }
    }
    [@NodeInfo("float3"), Serializable]
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

        public override void Visit(NodeVisitor visitor)
        {
            PortNames[0] = "float3" + value.ToString("R");
        }
    }

    [@NodeInfo("float2"), Serializable]
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

        public override void Visit(NodeVisitor visitor)
        {
            PortNames[0] = "float2" + value.ToString("R");
        }
    }

    [@NodeInfo("float"), Serializable]
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

        public override void Visit(NodeVisitor visitor)
        {
            PortNames[0] = value.ToString("R");
        }
    }

    [@NodeInfo("Result")]
    public sealed class OutputNode : ShaderNode
    {
        const int ALBEDO = 0;
        const int ALPHA = 1;
        const int ROUGHNESS = 2;
        const int METALLIC = 3;

        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(3), ALBEDO, "Albedo");
            AddPort(Direction.Input, new PortType.Float(1), ALPHA, "Alpha");
            AddPort(Direction.Input, new PortType.Float(1), ROUGHNESS, "Roughness");
            AddPort(Direction.Input, new PortType.Float(1), METALLIC, "Metallic");
        }
        public override void Visit(NodeVisitor visitor)
        {
            visitor.AppendLine($"float3 albedo = {GetInputString(ALBEDO)};");
            visitor.AppendLine($"float alpha = {GetInputString(ALPHA)};");
            visitor.AppendLine($"float roughness = {GetInputString(ROUGHNESS)};");
            visitor.AppendLine($"float metallic = {GetInputString(METALLIC)};");
        }
    }
}