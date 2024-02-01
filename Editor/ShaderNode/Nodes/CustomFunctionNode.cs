using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditor;
using System.IO;

namespace ZSG
{

    [NodeInfo("Input/Custom Function"), Serializable]
    public class CustomFunctionNode : ShaderNode
    {
        [SerializeField] string _code = "void CustomFunction(float3 In, out float3 Out)\n{\n    Out = In;\n}";
        string _path;
        string Code
        {
            get
            {
                if (!_useFile)
                {
                    return _code;
                }
                var include = Helpers.SerializableReferenceToObject<ShaderInclude>(_file);
                if (include == null)
                {
                    return _code;
                }
                _path = AssetDatabase.GetAssetPath(include);
                return File.ReadAllText(_path);
            }
        }

        [SerializeField] string _file;
        [SerializeField] bool _useFile = false;
        private FunctionParser _functionParser = new ();
        public override bool DisablePreview => true;

        public override Color Accent => new Color(0.2f, 0.4f, 0.8f);

        public override void AddElements()
        {
            inputContainer.Add(new VisualElement());
            outputContainer.Add(new VisualElement());

            if (!_functionParser.TryParse(Code)) return;



            foreach (var descriptor in _functionParser.descriptors)
            {
                AddPort(descriptor);
            }
            Update();
        }
        void Update()
        {
            foreach (var item in _functionParser.bindings)
            {
                Bind(item.Key, item.Value);
            }
            TitleLabel.text = _functionParser.methodName;
        }

        public override void AdditionalElements(VisualElement root)
        {
            var useFile = new Toggle("Use External File");
            useFile.RegisterValueChangedCallback(x => _useFile = x.newValue);
            var file = new ObjectField("File")
            {
                objectType = typeof(ShaderInclude)
            };
            if (!string.IsNullOrEmpty(_file))
            {
                file.value = Helpers.SerializableReferenceToObject<ShaderInclude>(_file);
            }
            file.RegisterValueChangedCallback(x =>
            {
                _file = Helpers.AssetSerializableReference(x.newValue);
            });

            var code = new TextField()
            {
                value = _code,
                multiline = true
            };
            code.RegisterValueChangedCallback((evt) =>
            {
                _code = evt.newValue;
            });

            root.Add(useFile);
            root.Add(file);
            root.Add(code);



        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            if (!_functionParser.TryParse(Code)) return;
            portDescriptors.Clear();
            foreach(var descriptor in _functionParser.descriptors)
            {
                portDescriptors.Add(descriptor.ID, descriptor);
            }
            ResetPorts();
            Update();

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

            visitor.AddFunction(_useFile ? "#include \"" + _path + "\"": Code);
        }
    }
}