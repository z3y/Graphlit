using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{

    [NodeInfo("Input/Function"), Serializable]
    public class CustomFunctionNode : ShaderNode
    {
        [SerializeField] string _code;
        [SerializeField] string _name;
        [SerializeField] List<PortDescriptor> _descriptors = new List<PortDescriptor>();
        const int OUT = 0;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(4), OUT));
            //string propertyName = GetVariableNameForPreview(OUT);
        }

        public override void AdditionalElements(VisualElement root)
        {
            var name = new TextField("Name")
            {
                value = _name
            };
            name.RegisterValueChangedCallback((evt) =>
            {
                _name = evt.newValue;
            });
            root.Add(name);

            var columns = new Columns();

            var portDir = new Column()
            {
                name = "dd",
                makeCell = () => new EnumField(PortDirection.Input),
                bindCell = (e, i) =>
                {
                    var field = e as EnumField;
                    field.value = _descriptors[i].Direction;

                    field.RegisterValueChangedCallback(evt =>
                    {
                        _descriptors[i].Direction = (PortDirection)evt.newValue;
                    });
                },
                width = 50
            };
            columns.Add(portDir);


            var ports = new MultiColumnListView(columns)
            {
                headerTitle = "Ports",
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                showFoldoutHeader = true,
                reorderable = true,
                itemsSource = _descriptors
            };
            root.Add(ports);

            var code = new TextField()
            {
                value = _code,
                multiline = true
            };
            code.RegisterValueChangedCallback((evt) =>
            {
                _code = evt.newValue;
            });
            root.Add(code);


        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            GeneratePreviewForAffectedNodes();
        }

        protected override void Generate(NodeVisitor visitor)
        {
            visitor.AddFunction(_code);

            PortData[OUT] = new GeneratedPortData(new Float(4, true), "Function" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = {_name}();");
        }
    }
}