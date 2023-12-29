using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph.Nodes
{
    /*[@NodeInfo("*", "a * b")]
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

            var result = SetOutputVariable(2, "Multiply");
            var type = InheritDynamicFloatMax(2, 0, 1);
            CastVariableName(ref a, 0, type.components);
            CastVariableName(ref b, 1, type.components);
            sb.AppendLine($"{type} {result} = {a} * {b};");
        }

        public override void DefaultInputValue(int portID)
        {
            portNames[portID] = "1";
            portTypes[portID] = new PortType.DynamicFloat(1);
        }
    }*/

    [@NodeInfo("*", "a * b")]
    public class MultiplyNode : ShaderNode
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

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            SetOutputString(OUT, "Multiply");
            var type = InheritFloatComponentsMax(OUT, new []{ A, B });

            var a = GetCastInputString(A, type.components);
            var b = GetCastInputString(B, type.components);

            AppendOutputLine(OUT, sb, $"{a} * {b}");
        }

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }

    }

    /*[@NodeInfo("+", "a + b")]
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

            var result = SetOutputVariable(2, "Add");
            var type = InheritDynamicFloatMax(2, 0, 1);
            CastVariableName(ref a, 0, type.components);
            CastVariableName(ref b, 1, type.components);
            sb.AppendLine($"{type} {result} = {a} + {b};");
        }
    }*/

    [@NodeInfo("dot", "dot(a, b)")]
    public class DotNode : ShaderNode
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

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var typaA = (PortType.Float)PortsTypes[A];
            var typaB = (PortType.Float)PortsTypes[B];

            int components = Mathf.Max(typaA.components, typaB.components);
            var a = GetCastInputString(A, components);
            var b = GetCastInputString(B, components);
            SetOutputString(OUT, "Dot");
            AppendOutputLine(OUT, sb, $"dot({a}, {b})");
        }

        public override string SetDefaultInputString(int portID)
        {
            return "1";
        }
    }

    /*[@NodeInfo("swizzle")]
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
            portNames[1] = "(" + a + ")." + swizzle;
            portTypes[1] = new PortType.DynamicFloat(swizzle.Length);
        }
    }*/

    [@NodeInfo("float4")]
    public class Float4Node : ShaderNode
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
        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            PortNames[0] = "float4" + value.ToString("R");
        }
    }
    [@NodeInfo("float3")]
    public class Float3Node : ShaderNode
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

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            PortNames[0] = "float3" + value.ToString("R");
        }
    }

    [@NodeInfo("float2")]
    public class Float2Node : ShaderNode
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

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            PortNames[0] = "float2" + value.ToString("R");
        }
    }

    [@NodeInfo("float")]
    public class FloatNode : ShaderNode
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

        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            PortNames[0] = value.ToString("R");
        }
    }

    [@NodeInfo("Result")]
    public class OutputNode : ShaderNode
    {
        const int IN = 0;
        //const int IN2 = 1;


        public override void Initialize()
        {
            AddPort(Direction.Input, new PortType.Float(4), IN);
            //AddPort(Direction.Input, new PortType.Float(2), IN2);

        }
        public override void Visit(System.Text.StringBuilder sb, int outID)
        {
            var col = GetCastInputString(IN, 4);
            //var col2 = GetCastInputString(IN, 2);
            //sb.AppendLine($"float2 col2 = {col2};");
            sb.AppendLine($"col = {col};");
        }
    }
}