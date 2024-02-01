using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using System.Linq;

namespace ZSG
{

    [NodeInfo("Input/Custom Function"), Serializable]
    public class CustomFunctionNode : ShaderNode
    {
        [SerializeField] string _code = "void CustomFunction(float3 In, out float3 Out)\n{\n    Out = In;\n}";
        private FunctionParser _functionParser = new ();
        public override bool DisablePreview => true;

        public override void AddElements()
        {
            inputContainer.Add(new VisualElement());
            outputContainer.Add(new VisualElement());

            if (!_functionParser.TryParse(_code)) return;


            foreach (var descriptor in _functionParser.descriptors)
            {
                AddPort(descriptor);
            }
            BindAll();
        }
        void BindAll()
        {
            foreach (var item in _functionParser.bindings)
            {
                Bind(item.Key, item.Value);
            }
        }

        public override void AdditionalElements(VisualElement root)
        {
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
            if (!_functionParser.TryParse(_code)) return;
            portDescriptors.Clear();
            foreach(var descriptor in _functionParser.descriptors)
            {
                portDescriptors.Add(descriptor.ID, descriptor);
            }
            ResetPorts();
            BindAll();

            GeneratePreviewForAffectedNodes();
        }

        string MethodParams()
        {
            string param = "";

            PortDescriptor[] array = portDescriptors.Values.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                PortDescriptor port = array[i];
                var data = PortData[port.ID];

                //if (port.Direction == PortDirection.Output) param += "out ";
                param += data.Name;

                if (i != array.Length - 1) param += ", ";
            }

            return param;
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string uniqueID = UniqueVariableID++.ToString();

            string methodName = _functionParser.methodName;

            foreach (PortDescriptor port in portDescriptors.Values)
            {
                if (port.Direction == PortDirection.Input) continue;

                string outName = $"{methodName}_{port.ID}_{uniqueID}";
                visitor.AppendLine($"{port.Type} {outName};");
                SetVariable(port.ID, outName);
            }

            visitor.AppendLine($"{methodName}({MethodParams()});");

            visitor.AddFunction(_code);
        }
    }
}