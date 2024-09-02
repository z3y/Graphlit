using System;
using UnityEngine.UIElements;
using UnityEngine;
using Enlit.Nodes;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditor;
using System.IO;

namespace Enlit
{

    [NodeInfo("Input/Custom Function"), Serializable]
    public class CustomFunctionNode : ShaderNode
    {
        public static readonly string[] Tag = new [] { "EnlitFunction", "ZSGFunction" };
        [MenuItem("Assets/Create/Shader/Enlit/Shader Include")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent("CustomFunction.hlsl", DefaultFunction);
            var include = new ShaderInclude();
            AssetDatabase.SetLabels(include, new[] { Tag[0] });
        }
        const string DefaultFunction = "void CustomFunction(float3 In, out float3 Out)\n{\n    Out = In;\n}";
        [SerializeField] string _code = DefaultFunction;
        private string _fileName;
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
                _fileName = include.name;
                _path = AssetDatabase.GetAssetPath(include);
                return File.ReadAllText(_path);
            }
        }

        [SerializeField] string _file;
        [SerializeField] bool _useFile = false;
        public void UseFile(ShaderInclude include)
        {
            _file = Helpers.AssetSerializableReference(include);
            _useFile = true;
        }
        private FunctionParser _functionParser = new();
        public override bool DisablePreview => true;

        public override Color Accent => new Color(0.2f, 0.4f, 0.8f);

        public override void Initialize()
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
            _portBindings.Clear();
            foreach (var item in _functionParser.bindings)
            {
                Bind(item.Key, item.Value);
            }
            TitleLabel.text = _useFile ? _fileName : _functionParser.methodName;
        }

        public override void AdditionalElements(VisualElement root)
        {
            var useFile = new Toggle("Use External File")
            {
                value = _useFile
            };
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

            var updateButton = new Button
            {
                text = "Update"
            };
            updateButton.clicked += ParseAndUpdate;

            var code = new TextField()
            {
                value = _code,
                multiline = true
            };
            code.RegisterValueChangedCallback((evt) =>
            {
                _code = evt.newValue;
            });

            var saveToFile = new Button()
            {
                text = "Save To File"
            };
            saveToFile.clicked += () =>
            {
                if (!_functionParser.TryParse(Code))
                {
                    return;
                }
                var path = EditorUtility.SaveFilePanel(
                    "Save text as hlsl",
                    "Assets/",
                    _functionParser.methodName + ".hlsl",
                    "hlsl");

                if (path.Length != 0)
                {
                    File.WriteAllText(path, _code);
                    string unityPath = "Assets" + path.Substring(Application.dataPath.Length);
                    AssetDatabase.ImportAsset(unityPath);
                    var asset = AssetDatabase.LoadAssetAtPath<ShaderInclude>(unityPath);
                    AssetDatabase.SetLabels(asset, new[] { Tag[0] });
                    UseFile(asset);
                    file.value = asset;
                    RefreshState();
                    ParseAndUpdate();
                }
            };

            root.Add(useFile);
            root.Add(file);
            root.Add(updateButton);
            root.Add(code);
            root.Add(saveToFile);

            void RefreshState()
            {
                file.style.display = !_useFile ? DisplayStyle.None : DisplayStyle.Flex;
                code.style.display = _useFile ? DisplayStyle.None : DisplayStyle.Flex;
                saveToFile.style.display = code.style.display;
            }

            useFile.RegisterValueChangedCallback(x =>
            {
                _useFile = x.newValue;
                RefreshState();
            });
            RefreshState();
        }

        public void ParseAndUpdate()
        {
            if (!_functionParser.TryParse(Code)) return;
            portDescriptors.Clear();
            foreach (var descriptor in _functionParser.descriptors)
            {
                portDescriptors.Add(descriptor.ID, descriptor);
            }
            ResetPorts();
            Update();

            GeneratePreviewForAffectedNodes();
        }

        bool IsVoidType()
        {
            return !portDescriptors.Values.Any(x => x.ID == 99);
        }

        string MethodParams()
        {
            string param = "";

            PortDescriptor[] array = portDescriptors.Values.ToArray();
            int lastParam = array.Length - 1;
            if (!IsVoidType())
            {
                lastParam--;
            }
            for (int i = 0; i < array.Length; i++)
            {
                PortDescriptor port = array[i];
                if (port.ID == 99)
                {
                    continue;
                }

                var data = PortData[port.ID];
                if (port.Direction == PortDirection.Input && _functionParser.defaultValues.ContainsKey(port.ID))
                {
                    var portElement = PortElements.Where(x => x.GetPortID() == port.ID).First();
                    if (!portElement.connected)
                    {
                        data.Name = _functionParser.defaultValues[port.ID];
                    }
                }

                param += data.Name;

                if (i != lastParam) param += ", ";
            }

            return param;
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string uniqueID = UniqueVariableID;

            string methodName = _functionParser.methodName;

            foreach (PortDescriptor port in portDescriptors.Values)
            {
                if (port.Direction == PortDirection.Input) continue;

                string outName = $"{methodName}_{port.ID}_{uniqueID}";
                visitor.AppendLine($"{port.Type} {outName};");
                SetVariable(port.ID, outName);
            }

            if (IsVoidType())
            {
                visitor.AppendLine($"{methodName}({MethodParams()});");
            }
            else
            {
                visitor.AppendLine($"{PortData[99].Name} = {methodName}({MethodParams()});");
            }
            visitor.AddFunction(_useFile ? "#include \"" + _path + "\"" : Code);

        }
    }
}