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

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }
    }

    [NodeInfo("+", "a + b")]
    public sealed class AddNode : ShaderNode, IRequireDescriptionVisitor
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

            visitor.AppendLine(FormatOutput(OUT, "Add", $"{a} + {b}"));
        }
    }

    [NodeInfo("dot", "dot(a, b)")]
    public sealed class DotNode : ShaderNode, IRequireDescriptionVisitor
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Input, new Float(1, true), A, "A"),
            new(PortDirection.Input, new Float(1, true), B, "B"),
            new(PortDirection.Output, new Float(1, false), OUT),
        };

        public void VisitDescription(DescriptionVisitor visitor)
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
            Ports.GetByID(OUT).Type = new Float(components);
            Ports.GetByID(IN).Type = Ports[OUT].Type;
        }
    }

    [NodeInfo("float"), Serializable]
    public class FloatNode : ShaderNode, IRequireDescriptionVisitor, IRequirePropertyVisitor
    {
        const int OUT = 0;
        [SerializeField] float _value;
        [SerializeField] string _propertyName;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Output, new Float(1, false), OUT),
        };

        private bool _isProperty => _propertyName != string.Empty;
        public PropertyDescriptor PropertyDescriptor => new PropertyDescriptor(PropertyType.Float, _propertyName);

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
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : _value.ToString("R");
        }

        public void VisitProperty(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("float2"), Serializable]
    public class Float2Node : ShaderNode, IRequireDescriptionVisitor, IRequirePropertyVisitor
    {
        const int OUT = 0;
        [SerializeField] Vector2 _value;
        [SerializeField] string _propertyName;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Output, new Float(2, false), OUT),
        };

        private bool _isProperty => _propertyName != string.Empty;
        public PropertyDescriptor PropertyDescriptor => new PropertyDescriptor(PropertyType.Float2, _propertyName);

        public override void AddElements(ShaderNodeVisualElement node)
        {
            var f = new Vector2Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;

                node.UpdatePreview((mat) => {
                    mat.SetVector(_propertyName, _value);
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
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : "float2" + _value.ToString("R");
        }

        public void VisitProperty(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("float3"), Serializable]
    public class Float3Node : ShaderNode, IRequireDescriptionVisitor, IRequirePropertyVisitor
    {
        const int OUT = 0;
        [SerializeField] Vector3 _value;
        [SerializeField] string _propertyName;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Output, new Float(3, false), OUT),
        };

        private bool _isProperty => _propertyName != string.Empty;
        public PropertyDescriptor PropertyDescriptor => new PropertyDescriptor(PropertyType.Float3, _propertyName);

        public override void AddElements(ShaderNodeVisualElement node)
        {
            var f = new Vector3Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;

                node.UpdatePreview((mat) => {
                    mat.SetVector(_propertyName, _value);
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
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : "float3" + _value.ToString("R");
        }

        public void VisitProperty(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("float4"), Serializable]
    public class Float4Node : ShaderNode, IRequireDescriptionVisitor, IRequirePropertyVisitor
    {
        const int OUT = 0;
        [SerializeField] Vector4 _value;
        [SerializeField] string _propertyName;

        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Output, new Float(4, false), OUT),
        };

        private bool _isProperty => _propertyName != string.Empty;
        public PropertyDescriptor PropertyDescriptor => new PropertyDescriptor(PropertyType.Float4, _propertyName);

        public override void AddElements(ShaderNodeVisualElement node)
        {
            var f = new Vector4Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;

                node.UpdatePreview((mat) => {
                    mat.SetVector(_propertyName, _value);
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
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : "float4" + _value.ToString("R");
        }

        public void VisitProperty(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }
}