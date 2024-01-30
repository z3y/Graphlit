using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using System.Linq;

namespace ZSG
{

    [NodeInfo("Input/Custom Function"), Serializable]
    public class CustomFunctionNode : ShaderNode
    {
        [SerializeField] string _code;
        [SerializeField] string _name;
        [SerializeField] List<SerializablePort> _ports = new List<SerializablePort>();

        enum PortType
        {
            Float,
            Float2,
            Float3,
            Float4,
            SamplerState,
            Texture2D
        }
        public override bool DisablePreview => true;

        static IPortType CreateInstance(PortType type)
        {
            return type switch
            {
                PortType.Float2 => new Float(2),
                PortType.Float3 => new Float(3),
                PortType.Float4 => new Float(4),
                PortType.SamplerState => new SamplerState(),
                PortType.Texture2D => new Texture2DObject(),
                PortType.Float or _ => new Float(1),
            };
        }

        [Serializable]
        struct SerializablePort
        {
            public SerializablePort(PortDirection direction, PortType type, int id, string name)
            {
                this.direction = direction;
                this.type = type;
                this.id = id;
                this.name = name;
            }
            public PortDirection direction;
            public PortType type;
            public int id;
            public string name;
        }

        public override void AddElements()
        {
            _ports.Clear();
            _ports.Add(new SerializablePort(PortDirection.Input, PortType.Float3, 0, "Yes"));
            _ports.Add(new SerializablePort(PortDirection.Input, PortType.Float, 1, "No"));
            _ports.Add(new SerializablePort(PortDirection.Input, PortType.Float2, 2, "UV"));
            //_ports.Add(new SerializablePort(PortDirection.Input, PortType.Texture2D, 3, "tex"));
            _ports.Add(new SerializablePort(PortDirection.Output, PortType.Float3, 4, "Out"));
            _ports.Add(new SerializablePort(PortDirection.Output, PortType.Float, 5, "Out2"));


            foreach (var port in _ports)
            {
                var t = CreateInstance(port.type);
                AddPort(new(port.direction, t, port.id, port.name));
            }
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

        string MethodTemplate()
        {
            string method = $"void {_name}(";

            PortDescriptor[] array = portDescriptors.Values.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                PortDescriptor port = array[i];
                if (port.Direction == PortDirection.Output) method += "out ";
                method += port.Type.ToString() + " ";
                method += port.Name;

                if (i != array.Length - 1) method += ", ";
            }


            method += ')';

            return method;
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

            foreach (PortDescriptor port in portDescriptors.Values)
            {
                if (port.Direction == PortDirection.Input) continue;

                string outName = $"{_name}_{port.ID}_{uniqueID}";
                visitor.AppendLine($"{port.Type} {outName};");
                SetVariable(port.ID, outName);
            }

            visitor.AppendLine($"{_name}({MethodParams()});");

            visitor.AddFunction(MethodTemplate() + "\n{\n" + _code + "\n}");
        }
    }
}