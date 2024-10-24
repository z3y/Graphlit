using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/UV"), Serializable]
    public class UVNode : ShaderNode
    {
        const int OUT = 0;

        [SerializeField] Channel _uv = Channel.UV0;
        [SerializeField] int _channels = 2;
        public override Precision DefaultPrecisionOverride => Precision.Float;
        enum Channel
        {
            UV0, UV1, UV2, UV3
        }

        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(2), OUT));
            Bind(OUT, ChannelToBinding());

            var dropdown = new EnumField(_uv);

            dropdown.RegisterValueChangedCallback((evt) =>
            {
                _uv = (Channel)evt.newValue;
                Bind(OUT, ChannelToBinding());
                GeneratePreviewForAffectedNodes();
            });
            inputContainer.Add(dropdown);
        }

        public override void AdditionalElements(VisualElement root)
        {
            var channelsSelector = new Toggle("float4");
            channelsSelector.RegisterValueChangedCallback((evt) =>
            {
                _channels = evt.newValue ? 4 : 2;
                portDescriptors[OUT].Type = new Float(_channels);
                GeneratePreviewForAffectedNodes();
            });
            root.Add(channelsSelector);
        }

        private PortBinding ChannelToBinding()
        {
            return _uv switch
            {
                Channel.UV0 => PortBinding.UV0,
                Channel.UV1 => PortBinding.UV1,
                Channel.UV2 => PortBinding.UV2,
                Channel.UV3 => PortBinding.UV3,
                _ => throw new NotImplementedException(),
            };
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}