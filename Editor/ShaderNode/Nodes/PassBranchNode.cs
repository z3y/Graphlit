using System;
using System.Text;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphlit
{
    [NodeInfo("Utility/Pass Branch"), Serializable]
    public class PassBranchNode : ShaderNode
    {
        const int TRUE = 1;
        const int FALSE = 2;
        const int OUT = 3;

        [Flags]
        public enum Pass {
            Forward = 1 << 0,
            ForwardAdd = 1 << 1,
            ShadowCaster = 1 << 2,
            Meta = 1 << 3
        }

        string FlagsToString(GenerationMode generationMode)
        {
            if (generationMode == GenerationMode.Preview)
            {
                return passFlags.HasFlag(Pass.Forward) ? "1" :"0";
            }
            var sb = new StringBuilder();
            bool or = false;
            if (passFlags.HasFlag(Pass.Forward))
            {
                or = true;
                sb.Append("defined(UNITY_PASS_FORWARDBASE)");
            }
            if (passFlags.HasFlag(Pass.ForwardAdd))
            {
                if (or) { sb.Append(" || "); };
                or = true;
                sb.Append("defined(UNITY_PASS_FORWARDADD)");
            }
            if (passFlags.HasFlag(Pass.ShadowCaster))
            {
                if (or) { sb.Append(" || "); };
                or = true;
                sb.Append("defined(UNITY_PASS_SHADOWCASTER)");
            }
            if (passFlags.HasFlag(Pass.Meta))
            {
                if (or) { sb.Append(" || "); };
                or = true;
                sb.Append("defined(UNITY_PASS_META)");
            }

            if (!or)
            {
                return "0";
            }

            return sb.ToString();
        }

        [SerializeField] public Pass passFlags;

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));

            var e = new EnumFlagsField("", passFlags);
            e.RegisterValueChangedCallback((e) =>
            {
                passFlags = (Pass)e.newValue;
                GeneratePreviewForAffectedNodes();
            });
            extensionContainer.Add(e);
        }


        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(TRUE, FALSE).dimensions);
            SetVariable(OUT, UniqueVariable);

            var data = PortData[OUT];
            var type = (Float)data.Type;
            visitor.AppendLine($"{PrecisionString(type.dimensions)} {data.Name};");
            visitor.AppendLine($"#if {FlagsToString(visitor.GenerationMode)}");
            visitor.AppendLine($"{data.Name} = {PortData[TRUE].Name};");
            visitor.AppendLine("#else");
            visitor.AppendLine($"{data.Name} = {PortData[FALSE].Name};");
            visitor.AppendLine("#endif");
        }
    }
}