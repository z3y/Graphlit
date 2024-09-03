using System;
using UnityEngine.UIElements;
using UnityEngine;

namespace Graphlit
{
    public abstract class PropertyNode : ShaderNode
    {
        public void SetReference(string guid)
        {
            _ref = guid;
        }

        protected abstract PropertyType propertyType { get; }

        protected const int OUT = 0;
        [SerializeField] internal string _ref;
        public override Color Accent => new Color(0.3f, 0.7f, 0.3f);
        [NonSerialized] public PropertyDescriptor propertyDescriptor;

        public override bool DisablePreview => true;
        public override void Initialize()
        {


            var graphData = GraphView.graphData;
            propertyDescriptor = graphData.properties.Find(x => x.guid == _ref);
            if (string.IsNullOrEmpty(_ref) || propertyDescriptor is null)
            {
                propertyDescriptor = new PropertyDescriptor(propertyType);
                graphData.properties.Add(propertyDescriptor);
                _ref = propertyDescriptor.guid;
            }
            else
            {
                _ref = propertyDescriptor.guid;
            }

            propertyDescriptor.graphView = GraphView;
            propertyDescriptor.UpdatePreviewMaterial();

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
            visitor.AddProperty(propertyDescriptor);
            PortData[OUT] = new GeneratedPortData(portDescriptors[OUT].Type, propertyDescriptor.GetReferenceName(visitor.GenerationMode));
        }
    }
}