namespace z3y.ShaderGraph.Nodes
{
    using System;
    using UnityEngine.UIElements;
    using UnityEngine;
    using z3y.ShaderGraph.Nodes.PortType;
    using System.Collections.Generic;

    [NodeInfo("*", "a * b")]
    public sealed class MultiplyNode : ShaderNode, IRequireDescriptionVisitor
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Input, new Float(1, true), A, "A"),
            new(PortDirection.Input, new Float(1, true), B, "B"),
            new(PortDirection.Output, new Float(1, true), OUT),
        };

        public void VisitDescription(DescriptionVisitor visitor)
        {
            var components = ImplicitTruncation(OUT, A, B);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            visitor.AppendLine(FormatOutput(OUT, "Multiply", $"{a} * {b}"));
        }
    }

    [NodeInfo("float"), Serializable]
    public class FloatNode : ShaderNode, IRequireDescriptionVisitor, IMayRequirePropertyVisitor, IRequireFunctionVisitor
    {
        const int OUT = 0;
        [SerializeField] float _value;
        [SerializeField] string _propertyName;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Output, new Float(1, true), OUT),
        };

        private bool _isProperty;
        public bool IsProperty {
            get => _isProperty || _propertyName != string.Empty;
            set => _isProperty = value;
        }

        public override void AddElements(ShaderNodeVisualElement node)
        {
            var f = new FloatField { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;

                node.UpdatePreview((mat) => {
                    mat.SetFloat(_propertyName, _value);
                });

            });

            node.inputContainer.Add(f);

            var p = new TextField { value = _propertyName };
            p.RegisterValueChangedCallback((evt) =>
            {
                _propertyName = evt.newValue;
            });

            node.inputContainer.Add(p);
        }

        public void VisitDescription(DescriptionVisitor visitor)
        {
            VariableNames[OUT] = IsProperty ? _propertyName : _value.ToString("R");
        }

        public void VisitProperty(PropertyVisitor visitor)
        {
            visitor.AddProperty(_propertyName);
        }

        public void VisitFunction(FunctionVisitor visitor)
        {
            visitor.AddFunction("cool func",
                @"
float TotallyCoolFunction(float a, float b)
{
    return a * b;
}
");
        }
    }

    [@NodeInfo("swizzle"), Serializable]
    public sealed class SwizzleNode : ShaderNode, IRequireDescriptionVisitor
    {
        const int IN = 0;
        const int OUT = 1;
        [SerializeField] string swizzle = "x";

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Input, new Float(1, true), IN),
            new(PortDirection.Output, new Float(1, true), OUT),

        };

        public override void AddElements(ShaderNodeVisualElement node)
        {
            var f = new TextField { value = swizzle };
            f.RegisterValueChangedCallback((evt) =>
            {
                swizzle = Swizzle.ValidateSwizzle(evt, f);
            });
            node.extensionContainer.Add(f);
        }

        public void VisitDescription(DescriptionVisitor visitor)
        {
            int components = swizzle.Length;
            var a = GetInputString(IN);
            VariableNames[OUT] = "(" + a + ")." + swizzle;
            Ports[OUT].Type = new Float(components);
            Ports[IN].Type = Ports[OUT].Type;
        }
    }

    /*
     * [@NodeInfo("*", "a * b")]
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

        public override void AddVisualElements(ShaderNodeVisualElement node)
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
    }*/

    
}