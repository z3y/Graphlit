using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Globalization;
using Graphlit.Nodes.PortType;
using System.Reflection;
using System.Linq;

namespace Graphlit
{

    [NodeInfo("Targets/Subgraph"), Serializable]
    public class SubgraphOutputNode : ShaderNode
    {
        [Serializable]
        public class SerializablePortDescriptor
        {
            public string name = "Port";
            public Vector4 value = new Vector4();
            [Range(1, 4)] public int dimension = 1;
            public int id = 0;

            public string type;
            public PortBinding binding = PortBinding.None;

            public Type ToSystemType() => Type.GetType("Graphlit.Nodes.PortType." + type);

            public string ValueToString()
            {
                string x, y, z, w;
                x = value.x.ToString(CultureInfo.InvariantCulture);
                y = value.y.ToString(CultureInfo.InvariantCulture);
                z = value.z.ToString(CultureInfo.InvariantCulture);
                w = value.w.ToString(CultureInfo.InvariantCulture);

                return dimension switch
                {
                    1 => $"float({x})",
                    2 => $"float2({x}, {y})",
                    3 => $"float3({x}, {y}, {z})",
                    4 or _ => $"float4({x}, {y}, {z}, {w})",
                };
            }

            public void AddPropertyDescriptor(ShaderNode node, PortDirection direction)
            {
                if (type == "Float")
                {
                    var desc = new PortDescriptor(direction, new Float(dimension), id, name);
                    node.portDescriptors.Add(id, desc);

                    if (binding != PortBinding.None)
                    {
                        node.Bind(id, binding);
                    }
                    else
                    {
                        node.DefaultValues[id] = ValueToString();
                    }
                }
                else
                {
                    var type = Type.GetType("Graphlit.Nodes.PortType." + this.type);
                    var instance = (IPortType)Activator.CreateInstance(type);
                    var desc = new PortDescriptor(direction, instance, id, name);
                    node.portDescriptors.Add(id, desc);
                }
            }
        }

        public override bool DisablePreview => true;
        public override Color Accent => Color.magenta;


        public override void Initialize()
        {
            inputContainer.Add(new VisualElement());
            var data = GraphView.graphData;
            foreach (var output in data.subgraphOutputs)
            {
                output.AddPropertyDescriptor(this, PortDirection.Input);
            }
            ResetPorts();
        }

        public override void AdditionalElements(VisualElement root)
        {
            var graphData = GraphView.graphData;
            var defaultPreviewState = new EnumField("Default Preview", graphData.defaultPreviewState);
            defaultPreviewState.RegisterValueChangedCallback(x => graphData.defaultPreviewState = (GraphData.DefaultPreviewState)x.newValue);
            root.Add(defaultPreviewState);

            root.Add(PropertyDescriptor.CreateReordableListElement(graphData.properties, GraphView));

            root.Add(CreateReordableListElement(graphData.subgraphInputs, false));
            root.Add(CreateReordableListElement(graphData.subgraphOutputs, true));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            var data = GraphView.graphData;
            foreach (var output in data.subgraphOutputs)
            {
                visitor.AppendLine($"{output.name} = {PortData[output.id].Name};");

            };
        }

        public VisualElement CreateReordableListElement(List<SerializablePortDescriptor> ports, bool isOutput)
        {
            var e = new IMGUIContainer();
            var list = CreateReordableList(ports, isOutput);

            e.onGUIHandler += () =>
            {
                list.DoLayoutList();
            };

            return e;
        }

        public ReorderableList CreateReordableList(List<SerializablePortDescriptor> ports, bool isOutput)
        {
            var reorderableList = new ReorderableList(ports, typeof(SerializablePortDescriptor), true, true, true, true);

            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, isOutput ? "Output" : "Input");
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var p = ports[index];
                var style = new GUIStyle(GUI.skin.label) { richText = true };

                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(rect, $"<b>{p.name}</b>", style);
                rect.x += 120;
                rect.width -= 120;
                if (p.type == "Float" && p.dimension > 1)
                {
                    EditorGUI.LabelField(rect, $"Float{p.dimension}", style);
                }
                else
                {
                    EditorGUI.LabelField(rect, $"{p.type}", style);
                }

                if (isActive)
                {
                    EditorGUILayout.BeginVertical(new GUIStyle("GroupBox"));
                    p.name = EditorGUILayout.TextField(new GUIContent("Name"), p.name);

                    if (p.type == "Float")
                    {
                        p.dimension = EditorGUILayout.IntSlider(new GUIContent("Dimension"), p.dimension, 1, 4);
                        if (!isOutput)
                        {
                            p.binding = (PortBinding)EditorGUILayout.EnumPopup(new GUIContent("Binding"), p.binding);
                            if (p.binding == PortBinding.None)
                            {
                                p.value = EditorGUILayout.Vector4Field(new GUIContent("Default Value"), p.value);
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Update();
                }
            };

            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                void OnTypeSelected(object data)
                {
                    var type = (Type)data;
                    ports.Add(new SerializablePortDescriptor() {
                        id = GraphView.graphData.subgraphOutputIdCounter++, type = type.Name
                    });
                    list.Select(ports.Count - 1);

                    Update();
                }

                var menu = new GenericMenu();
                var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IPortType).IsAssignableFrom(t) && !t.IsClass)
                    .ToArray()[1..];

                foreach (var t in types)
                {
                    menu.AddItem(new GUIContent(t.Name), false, OnTypeSelected, t);
                }

                menu.ShowAsContext();
            };

            reorderableList.onRemoveCallback += (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);

                Update();
            };

            reorderableList.onReorderCallback = (ReorderableList list) =>
            {
                Update();
            };

            return reorderableList;
        }

        void Update()
        {
            _portBindings.Clear();
            portDescriptors.Clear();

            Initialize();

            CleanLooseEdges();
        }
    }
}