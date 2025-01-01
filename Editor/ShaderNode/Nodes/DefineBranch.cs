using System;
using System.Text;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphlit
{
    [NodeInfo("Utility/Define Branch"), Serializable]
    public class DefineBranchNode : ShaderNode
    {
        const int TRUE = 1;
        const int FALSE = 2;
        const int OUT = 3;
        [SerializeField] string _define = "defined(_KEYWORD)";
        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));

            var e = new TextField("") { value = _define };
            e.RegisterValueChangedCallback((e) =>
            {
                _define = e.newValue;
            });

            e.RegisterCallback<FocusOutEvent>((e) =>
            {
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
            visitor.AppendLine($"#if {_define}");
            visitor.AppendLine($"{data.Name} = {PortData[TRUE].Name};");
            visitor.AppendLine("#else");
            visitor.AppendLine($"{data.Name} = {PortData[FALSE].Name};");
            visitor.AppendLine("#endif");
        }
    }
}