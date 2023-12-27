using System;
using UnityEngine;
using UnityEngine.UIElements;
using static z3y.ShaderGraph.Nodes.PortType;

namespace z3y.ShaderGraph.Nodes
{
    [@NodeInfo("*", "a * b")]
    public class MultiplyNode : ShaderNode
    {
        public override void AddVisualElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            string a = GetInputVariable(0);
            string b = GetInputVariable(1);

            var result = GetOutputVariable(2, "Multiply");
            var type = InheritDynamicFloatMax(2, 0, 1);
            CastVariableName(ref a, 0, type.components);
            CastVariableName(ref b, 1, type.components);
            sb.AppendLine($"{type} {result} = {a} * {b};");
        }

        public override void DefaultInputValue(int portID)
        {
            varibleNames[portID] = "1";
            portTypes[portID] = new PortType.DynamicFloat(1);
        }
    }

    [@NodeInfo("+", "a + b")]
    public class AddNode : ShaderNode
    {
        public override void AddVisualElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetInputVariable(0);
            var b = GetInputVariable(1);

            var result = GetOutputVariable(2, "Add");
            var type = InheritDynamicFloatMax(2, 0, 1);
            CastVariableName(ref a, 0, type.components);
            CastVariableName(ref b, 1, type.components);
            sb.AppendLine($"{type} {result} = {a} + {b};");
        }
    }

    [@NodeInfo("++", "a + b")]
    public class AddMoreNode : ShaderNode
    {
        public override void AddVisualElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2, "default");
            AddOutput(typeof(PortType.DynamicFloat), 3, "more");
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetInputVariable(0);
            var b = GetInputVariable(1);

            if (outID == 2)
            {
                var result = GetOutputVariable(2, "Default");
                var result2 = GetOutputVariable(3, "More");

                var type = InheritDynamicFloatMax(2, 0, 1);
                CastVariableName(ref a, 0, type.components);
                CastVariableName(ref b, 1, type.components);
                sb.AppendLine($"{type} {result} = {a} + {b};");
            }
            else if (outID == 3)
            {
                var result = GetOutputVariable(3, "More");

                var type = new PortType.DynamicFloat(4);
                portTypes[3] = type;
                CastVariableName(ref a, 0, type.components);
                CastVariableName(ref b, 1, type.components);
                sb.AppendLine($"{type} {result} = {a} + {b};");
            }
            
        }
    }

    [@NodeInfo("dot", "dot(a, b)")]
    public class DotNode : ShaderNode
    {
        public override void AddVisualElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "a");
            AddInput(typeof(PortType.DynamicFloat), 1, "b");
            AddOutput(typeof(PortType.DynamicFloat), 2);
        }
        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var a = GetInputVariable(0);
            var b = GetInputVariable(1);

            var result = GetOutputVariable(2, "Dot");
            var type = InheritDynamicFloatMax(2, 0, 1);
            type.components = 1;
            portTypes[2] = type;
            sb.AppendLine($"{type} {result} = dot({a}, {b});");
        }

        public override void DefaultInputValue(int portID)
        {
            varibleNames[portID] = "1";
            portTypes[portID] = new PortType.DynamicFloat(1);
        }
    }

    [@NodeInfo("swizzle")]
    public class SwizzleNode : ShaderNode
    {
        [UnityEngine.SerializeField] string swizzle = "x";
        public override void AddVisualElements()
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
            var a = GetInputVariable(0);
            varibleNames[1] = "(" + a + ")." + swizzle;
            portTypes[1] = new PortType.DynamicFloat(swizzle.Length);
        }
    }

    [@NodeInfo("float4")]
    public class Float4Node : ShaderNode
    {
        [SerializeField] Vector4 value;

        public override void AddVisualElements()
        {
            AddOutput(typeof(PortType.DynamicFloat), 0);

            var f = new Vector4Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] = "float4" + value.ToString("R");
            portTypes[0] = new PortType.DynamicFloat(4, false);
        }
    }

    [@NodeInfo("float3")]
    public class Float3Node : ShaderNode
    {
        [SerializeField] Vector3 value;

        public override void AddVisualElements()
        {
            AddOutput(typeof(PortType.DynamicFloat), 0);

            var f = new Vector3Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] = "float3" + value.ToString("R");
            portTypes[0] = new PortType.DynamicFloat(3, false);
        }
    }

    [@NodeInfo("float2")]
    public class Float2Node : ShaderNode
    {
        [SerializeField] Vector2 value;

        public override void AddVisualElements()
        {
            AddOutput(typeof(PortType.DynamicFloat), 0);

            var f = new Vector2Field { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] = "float2" + value.ToString("R");
            portTypes[0] = new PortType.DynamicFloat(2);
        }
    }

    [@NodeInfo("float")]
    public class FloatNode : ShaderNode
    {
        [SerializeField] float value;

        public override void AddVisualElements()
        {
            AddOutput(typeof(PortType.DynamicFloat), 0);

            var f = new FloatField { value = value };
            f.RegisterValueChangedCallback((evt) => {
                value = evt.newValue;
            });
            Node.inputContainer.Add(f);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] = value.ToString("R");
            portTypes[0] = new PortType.DynamicFloat(1);
        }
    }

    [@NodeInfo("Property")]
    public class PropertyNode : ShaderNode
    {
        [SerializeField] string displayName = string.Empty;
        [SerializeField] PropertyType propertyType = PropertyType.Float;

        public override void AddVisualElements()
        {
            var f = new TextField { value = displayName };
            f.RegisterValueChangedCallback((evt) => {
                displayName = evt.newValue;
            });
            Node.inputContainer.Add(f);

            var e = new EnumField("Type", propertyType);
            e.RegisterValueChangedCallback((ChangeEvent<Enum> evt) => {
                propertyType = (PropertyType)evt.newValue;
            });
            Node.extensionContainer.Add(e);

            AddOutput(typeof(PortType.DynamicFloat), 0);
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            varibleNames[0] = '_' + displayName;
            portTypes[0] = new PortType.DynamicFloat(4);
        }
    }

    [@NodeInfo("Result")]
    public class OutputNode : ShaderNode
    {
        public override void AddVisualElements()
        {
            AddInput(typeof(PortType.DynamicFloat), 0, "Result");
        }

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            sb.AppendLine($"col = {GetInputVariable(0)};");
        }
    }
}