using System;
using UnityEngine;
using UnityEngine.UIElements;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Utility/Branch"), Serializable]
    public class BranchNode : ShaderNode
    {
        [SerializeField] bool _dynamicBranch = false; 
        const int BOOL = 0;
        const int TRUE = 1;
        const int FALSE = 2;
        const int OUT = 3;
        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Bool(), BOOL, "Bool"));
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        public override void AdditionalElements(VisualElement root)
        {
            var f = new Toggle("Dynamic Branch") { value = _dynamicBranch };
            f.RegisterValueChangedCallback((evt) =>
            {
                _dynamicBranch = evt.newValue;
                UpdatePreviewMaterial();
            });
            root.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(TRUE, FALSE).dimensions);

            if (_dynamicBranch)
            {
                SetVariable(OUT, UniqueVariable);

                var data = PortData[OUT];
                var type = (Float)data.Type;
                visitor.AppendLine($"{PrecisionString(type.dimensions)} {data.Name};");
                visitor.AppendLine($"UNITY_BRANCH if ({PortData[BOOL].Name})");
                visitor.AppendLine("{");
                visitor.AppendLine($"   {data.Name} = {PortData[TRUE].Name};");
                visitor.AppendLine("} else {");
                visitor.AppendLine($"   {data.Name} = {PortData[FALSE].Name};");
                visitor.AppendLine("}");
            }
            else
            {
                Output(visitor, OUT, $"{PortData[BOOL].Name} ? {PortData[TRUE].Name} : {PortData[FALSE].Name}");
            }
        }
    }
}