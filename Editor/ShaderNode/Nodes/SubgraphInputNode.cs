using System;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;

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
        [NonSerialized] public PropertyDescriptor propertyDescriptor;

        public override bool DisablePreview => true;

        [NonSerialized] SubgraphOutputNode _subgraphOut;

        public override void Initialize()
        {
            var output = GraphView.graphData.subgraphInputs.Where(x => x.id == _ref).FirstOrDefault();


            if (output is null)
            {
                return;
            }

            //PortDescriptor desc;
            output.AddPropertyDescriptor(this, PortDirection.Output);


            //TitleLabel.text = propertyDescriptor.Name;

            ResetPorts();

            propertyDescriptor.onValueChange += () =>
            {
                if (TitleLabel is null || propertyDescriptor is null)
                {
                    return;
                }
                TitleLabel.text = propertyDescriptor.displayName;
            };
            TitleLabel.text = propertyDescriptor.displayName;
        }

        public override void AdditionalElements(VisualElement root)
        {
            var imgui = new IMGUIContainer();
            imgui.onGUIHandler = () =>
            {
                propertyDescriptor.PropertyEditorGUI();
            };
            root.Add(imgui);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                return;
            }

            string uniqueID = UniqueVariableID;

            //var subOut = GraphView.graphData.subgraphInputs.Where(x => x.id == _ref).FirstOrDefault();

            var subOut = GraphView.graphElements.OfType<SubgraphOutputNode>().FirstOrDefault();

            foreach (PortDescriptor port in portDescriptors.Values)
            {
                string name = $"SubgraphInput_{port.ID}_{uniqueID}";

                int id = port.ID;
                SetVariable(id, name);

                PortData[id] = subOut.subgraphResults[id];
            }

            //var output = GraphView.graphData.subgraphInputs.Where(x => x.id == _ref).FirstOrDefault();
            //PortData[OUT] = new GeneratedPortData(portDescriptors[OUT].Type, output.name);
        }
    }
}