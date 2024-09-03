using System;
using UnityEngine;
using UnityEngine.UIElements;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Normal"), Serializable]
    public class NormalNode : ShaderNode
    {
        public override PreviewType DefaultPreviewOverride => PreviewType.Preview3D;
        [SerializeField] BindingSpaceObjectWorld _space = BindingSpaceObjectWorld.World;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(3), 0));
            Bind(0, PortBindings.NormalBindingFromSpace(_space));

            var dropdown = new EnumField(_space);

            dropdown.RegisterValueChangedCallback((evt) =>
            {
                _space = (BindingSpaceObjectWorld)evt.newValue;
                Bind(0, PortBindings.NormalBindingFromSpace(_space));
                GeneratePreviewForAffectedNodes();
            });
            inputContainer.Add(dropdown);
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}