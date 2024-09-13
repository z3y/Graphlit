using System;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using System.ComponentModel;

namespace Graphlit
{
    [NodeInfo("Hidden/Subgraph Input"), Serializable]
    public class SubgraphInputNode : ShaderNode
    {
        public void SetReference(int id)
        {
            _ref = id;
        }

        protected const int OUT = 0;
        [SerializeField] internal int _ref;
        public override Color Accent => Color.magenta;
        //[NonSerialized] public PropertyDescriptor propertyDescriptor;

        public override bool DisablePreview => true;

        public override void Initialize()
        {
            var output = GraphView.graphData.subgraphInputs.Where(x => x.id == _ref).FirstOrDefault();

            if (output is null)
            {
                return;
            }

            PortDescriptor desc;
            if (output.type == "Float")
            {
                desc = new PortDescriptor(PortDirection.Output, new Float(output.dimension), output.id, output.name);
                portDescriptors.Add(output.id, desc);

                if (output.binding != PortBinding.None)
                {
                    Bind(output.id, output.binding);
                }
                else
                {
                    DefaultValues[output.id] = output.ValueToString();
                }
            }
            else
            {
                var type = Type.GetType("Graphlit.Nodes.PortType." + output.type);
                var instance = (IPortType)Activator.CreateInstance(type);
                desc = new PortDescriptor(PortDirection.Output, instance, output.id, output.name);
                portDescriptors.Add(output.id, desc);
            }


            TitleLabel.text = desc.Name;

            ResetPorts();

            /*propertyDescriptor.onValueChange += () =>
            {
                if (TitleLabel is null || propertyDescriptor is null)
                {
                    return;
                }
                TitleLabel.text = propertyDescriptor.displayName;
            };
            TitleLabel.text = propertyDescriptor.displayName;*/
        }

        public override void AdditionalElements(VisualElement root)
        {
            /*var imgui = new IMGUIContainer();
            imgui.onGUIHandler = () =>
            {
                propertyDescriptor.PropertyEditorGUI();
            };
            root.Add(imgui);*/
        }

        protected override void Generate(NodeVisitor visitor)
        {
            //visitor.AddProperty(propertyDescriptor);
            //PortData[OUT] = new GeneratedPortData(portDescriptors[OUT].Type, propertyDescriptor.GetReferenceName(visitor.GenerationMode));
        }
    }
}