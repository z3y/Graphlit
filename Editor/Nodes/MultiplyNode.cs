namespace z3y.ShaderGraph.Nodes
{
    using System;
    using UnityEngine.UIElements;
    using UnityEngine;
    using z3y.ShaderGraph.Nodes.PortType;
    using System.Collections.Generic;

    [NodeInfo("*", "a * b")]
    public sealed class MultiplyNode : ShaderNode, IRequireExpressionVisitor
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

        public void Visit(ExpressionVisitor visitor)
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
    public sealed class AddNode : ShaderNode, IRequireExpressionVisitor
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

        public void Visit(ExpressionVisitor visitor)
        {
            var components = ImplicitTruncation(OUT, A, B);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);

            visitor.AppendLine(FormatOutput(OUT, "Add", $"{a} + {b}"));
        }
    }

    [NodeInfo("dot", "dot(a, b)")]
    public sealed class DotNode : ShaderNode, IRequireExpressionVisitor
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

        public void Visit(ExpressionVisitor visitor)
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
    public sealed class SwizzleNode : ShaderNode, IRequireExpressionVisitor
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
                swizzle = Swizzle.Validate(evt, f);
            });
            node.extensionContainer.Add(f);
        }

        public void Visit(ExpressionVisitor visitor)
        {
            int components = swizzle.Length;
            var a = GetInputString(IN);
            VariableNames[OUT] = a + "." + swizzle;
            Ports.GetByID(OUT).Type = new Float(components);
            Ports.GetByID(IN).Type = Ports[OUT].Type;
        }
    }

    [NodeInfo("float"), Serializable]
    public class FloatNode : ShaderNode, IRequireExpressionVisitor, IRequirePropertyVisitor
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



            });

            node.inputContainer.Add(f);

            var p = new TextField { value = _propertyName };
            p.RegisterValueChangedCallback((evt) =>
            {
                _propertyName = evt.newValue;
            });

            node.inputContainer.Add(p);
        }

        public void Visit(ExpressionVisitor visitor)
        {
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : "float(" + _value.ToString("R") + ")";
        }

        public void Visit(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("float2"), Serializable]
    public class Float2Node : ShaderNode, IRequireExpressionVisitor, IRequirePropertyVisitor
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

            });

            node.inputContainer.Add(f);

            var p = new TextField { value = _propertyName };
            p.RegisterValueChangedCallback((evt) =>
            {
                _propertyName = evt.newValue;
            });

            node.inputContainer.Add(p);
        }

        public void Visit(ExpressionVisitor visitor)
        {
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : "float2" + _value.ToString("R");
        }

        public void Visit(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("float3"), Serializable]
    public class Float3Node : ShaderNode, IRequireExpressionVisitor, IRequirePropertyVisitor
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
            node.UpdateMaterial = (mat) => {
                mat.SetVector(PropertyDescriptor.Name, _value);
            };

            var f = new Vector3Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;
                node.UpdatePreview();
            });

            node.inputContainer.Add(f);

            var p = new TextField { value = _propertyName };
            p.RegisterValueChangedCallback((evt) =>
            {
                _propertyName = evt.newValue;
            });

            node.inputContainer.Add(p);
        }

        public void Visit(ExpressionVisitor visitor)
        {
            bool isProperty = _isProperty || visitor.GenerationMode == GenerationMode.Preview;
            VariableNames[OUT] = isProperty ? PropertyDescriptor.Name : "float3" + _value.ToString("R");
        }

        public void Visit(PropertyVisitor visitor)
        {
            bool isProperty = _isProperty || visitor.GenerationMode == GenerationMode.Preview;

            if (!isProperty) return;

            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("float4"), Serializable]
    public class Float4Node : ShaderNode, IRequireExpressionVisitor, IRequirePropertyVisitor
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
            node.UpdateMaterial = (mat) => {
                mat.SetVector(_propertyName, _value);
            };

            var f = new Vector4Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;
                node.UpdatePreview();
            });

            node.inputContainer.Add(f);

            var p = new TextField { value = _propertyName };
            p.RegisterValueChangedCallback((evt) =>
            {
                _propertyName = evt.newValue;
            });

            node.inputContainer.Add(p);
        }

        public void Visit(ExpressionVisitor visitor)
        {
            VariableNames[OUT] = _isProperty ? PropertyDescriptor.Name : "float4" + _value.ToString("R");
        }

        public void Visit(PropertyVisitor visitor)
        {
            if (!_isProperty) return;
            visitor.AddProperty(PropertyDescriptor);
        }
    }

    [NodeInfo("Custom Function"), Serializable]
    public class CustomFunctionode : ShaderNode, IRequireExpressionVisitor, IRequireFunctionVisitor
    {
        const int OUT = 0;

        [SerializeField] private string _code;
        [SerializeField] private string _functionName;


        public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
        {
            new(PortDirection.Output, new Float(4, false), OUT),
        };


        public override void AddElements(ShaderNodeVisualElement node)
        {
            var functionName = new TextField { value = _functionName };
            functionName.RegisterValueChangedCallback((evt) =>
            {
                _functionName = evt.newValue;
            });
            node.extensionContainer.Add(functionName);



            var code = new TextField { value = _code };
            code.RegisterValueChangedCallback((evt) =>
            {
                _code = evt.newValue;
            });
            code.multiline = true;

            node.extensionContainer.Add(code);
        }

        public void Visit(ExpressionVisitor visitor)
        {
            visitor.AppendLine(FormatOutput(OUT, "CustomFunction", $"{_functionName}()"));
        }

        public void Visit(FunctionVisitor visitor)
        {
            if (string.IsNullOrEmpty(_code))
            {
                return;
            }

            visitor.AddFunction($"float4 {_functionName}()\n" + '{' + _code + '}');
        }
    }
}