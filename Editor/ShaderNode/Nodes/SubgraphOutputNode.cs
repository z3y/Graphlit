/*using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using System.Globalization;

namespace Graphlit
{

    [NodeInfo("Target/Subgraph Output"), Serializable]
    public class SubgraphOutputNode : ShaderNode
    {
        [Serializable]
        public class CustomPort
        {
            public string name = "Port ";
            public Vector4 value;
            [Range(1, 4)] public int dimension = 1;
            public int id = 0;
            public PortBinding binding = PortBinding.None;

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
        }

        public override bool DisablePreview => true;
        public override Color Accent => new Color(0.2f, 0.4f, 0.8f);

        [SerializeField] public List<CustomPort> outputs;
        [SerializeField] public List<CustomPort> inputs;

        public override void Initialize()
        {
        }

        public override void AdditionalElements(VisualElement root)
        {
            var graphData = GraphView.graphData;
            root.Add(PropertyDescriptor.CreateReordableListElement(graphData.properties, GraphView));

            root.Add(CreateReordableListElement(outputs, GraphView, "Outputs"));
            root.Add(CreateReordableListElement(inputs, GraphView, "Inputs"));
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }

        public static VisualElement CreateReordableListElement(List<CustomPort> ports, ShaderGraphView graphView, string label)
        {
            var e = new IMGUIContainer();
            var list = CreateReordableList(ports, graphView, label);

            e.onGUIHandler += () =>
            {
                list.DoLayoutList();
            };

            return e;
        }

        public static ReorderableList CreateReordableList(List<CustomPort> ports, ShaderGraphView graphView, string label)
        {
            var reorderableList = new ReorderableList(ports, typeof(CustomPort), true, true, true, true);

            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, label);
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var p = ports[index];
                var style = new GUIStyle(GUI.skin.label) { richText = true };

                EditorGUI.LabelField(rect, $"<b>{p.name}</b>", style);
                //rect.x += 120;
                //rect.width -= 120;
                //EditorGUI.LabelField(rect, $"{p.displayName}", style);

                if (isActive)
                {
                    EditorGUILayout.BeginVertical(new GUIStyle("GroupBox"));
                    //ports[index].PropertyEditorGUI();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            };

            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                void OnTypeSelected(object data)
                {
                    var type = (PropertyType)data;
                    ports.Add(new CustomPort() { });
                    list.Select(ports.Count - 1);
                }

                var menu = new GenericMenu();
                foreach (PropertyType value in Enum.GetValues(typeof(PropertyType)))
                {
                    menu.AddItem(new GUIContent(Enum.GetName(typeof(PropertyType), value)), false, OnTypeSelected, value);
                }

                menu.ShowAsContext();
            };

            reorderableList.onRemoveCallback += (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);

                if (graphView == null)
                {
                    return;
                }

                var propertyNodes = graphView.graphElements.OfType<PropertyNode>();
                foreach (var node in propertyNodes)
                {
                    if (ports.Any(x => x.guid == node.propertyDescriptor.guid))
                    {
                        continue;
                    }

                    // remove for now, convert later when avaliable
                    foreach (var port in node.PortElements)
                    {
                        node.Disconnect(port);
                    }
                    graphView.RemoveElement(node);
                }
            };

            return reorderableList;
        }
    }
}*/