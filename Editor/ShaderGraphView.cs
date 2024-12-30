using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Graphlit.Nodes;

namespace Graphlit
{

    public class ShaderGraphView : GraphView
    {
        private ShaderNodeSearchWindow _searchWindow;
        internal ShaderGraphWindow _editorWindow;

        public GraphData graphData;
        public VisualElement additionalNodeElements;
        public int uniqueID = 0;

        public Vector2 lastMousePos;
        public Vector2 copyMousePos;
        public List<ShaderNode> cachedNodesForBuilder;
        public List<RegisterVariableNode> cachedRegisterVariablesForBuilder;

        public void UpdateCachedNodesForBuilder()
        {
            cachedNodesForBuilder = nodes.OfType<ShaderNode>().ToList();
            cachedRegisterVariablesForBuilder = cachedNodesForBuilder.OfType<RegisterVariableNode>().ToList();
            //Debug.Log($"UpdateCachedNodesForBuilder");
        }

        public Material PreviewMaterial = new(Shader.Find("Unlit/Color"))
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Material _importedMaterial;
        public Material ImportedMaterial
        {
            get
            {
                if (_importedMaterial == null)
                {
                    _importedMaterial = AssetDatabase.LoadAllAssetRepresentationsAtPath(_assetPath).OfType<Material>().FirstOrDefault();
                }
                return _importedMaterial;
            }
        }

        public bool IsSubgraph => _assetPath.EndsWith(".subgraphlit");

        string _assetPath;
        public ShaderGraphView(ShaderGraphWindow editorWindow, string assetPath)
        {
            _editorWindow = editorWindow;
            _assetPath = assetPath;
            // manipulators
            SetupZoom(0.1f, 10.0f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            this.AddManipulator(CreateGroupContextualMenu());

            style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 1);

            // search window
            if (!_searchWindow)
            {
                _searchWindow = ScriptableObject.CreateInstance<ShaderNodeSearchWindow>();
                _searchWindow.Initialize(this);
            }
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition, 350, 450), _searchWindow);

            RegisterCallback<KeyDownEvent>(UndoEvent);
            RegisterCallback<KeyDownEvent>(NodeHotkeyKeyDown);

            RegisterCallback<KeyUpEvent>(NodeHotkeyUpDown);

            RegisterCallback<ClickEvent>(NodeHotkey);

            graphViewChanged += OnGraphViewChanged;

            serializeGraphElements = SerializeGraphElementsImpl;
            unserializeAndPaste = UnserializeAndPasteImpl;

            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            UpdateCachedNodesForBuilder();

            LoopDetection(change);


            if (change.elementsToRemove is not null)
            {
                foreach (var graphElement in change.elementsToRemove)
                {
                    if (graphElement is Edge edge)
                    {
                        ShaderBuilder.GeneratePreviewFromEdge(this, edge, true);
                    }
                }
            }

            RecordUndo();
            _editorWindow.SetDirty();
            return change;
        }

        void LoopDetection(GraphViewChange change)
        {
            if (change.edgesToCreate is null)
            {
                return;
            }


            foreach (var edge in change.edgesToCreate.ToArray())
            {
                string startGUID = edge.output.node.viewDataKey;

                if (LoopDetection(startGUID, edge))
                {
                    change.edgesToCreate.Remove(edge);
                    Debug.LogError("Infinite loop detected");
                    break;
                }
            }
        }

        void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            lastMousePos = contentViewContainer.WorldToLocal(evt.mousePosition);
        }

        bool LoopDetection(string startGUID, Edge edge)
        {
            var connectedNode = (ShaderNode)edge.input.node;
            if (connectedNode.viewDataKey == startGUID)
            {
                return true;
            }
            foreach (var port in connectedNode.Outputs)
            {
                if (!port.connected)
                {
                    continue;
                }

                foreach (var edges in port.connections)
                {
                    if (LoopDetection(startGUID, edges))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SetDirty() => _editorWindow.SetDirty();

        public string SerializeGraphElementsImpl(IEnumerable<GraphElement> elements)
        {
            var data = new SerializableGraph
            {
                nodes = SerializableGraph.ElementsToSerializableNode(elements).ToList(),
            };

            foreach (var element in elements)
            {
                if (element is Group group)
                {
                    data.groups.Add(new SerializableGraph.SerializableGroup(group));
                }
                else if (element is PropertyNode prop)
                {
                    data.data.properties.Add(prop.propertyDescriptor);
                }
            }

            copyMousePos = lastMousePos;
            return EditorJsonUtility.ToJson(data, false);
        }

        public void UnserializeAndPasteImpl(string operationName, string jsonData)
        {
            var data = new SerializableGraph();
            EditorJsonUtility.FromJsonOverwrite(jsonData, data);

            data.Reposition(lastMousePos);
            var graphElements = data.PasteElementsAndOverwiteGuids(this);

            ClearSelection();

            foreach (var graphElement in graphElements)
            {
                AddToSelection(graphElement);
            }
            
        }
        
        private IManipulator CreateGroupContextualMenu()
        {
            return new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Group", actionEvent.eventInfo.localMousePosition)))
            );
        }

        private GraphElement CreateGroup(string title, Vector2 localMousePosition)
        {
            TransformMousePositionToLocalSpace(ref localMousePosition, false);
            var group = CreateGroup(title);

            group.SetPosition(new Rect(localMousePosition, Vector3.one));
            return group;
        }

        public Group CreateGroup(string title)
        {
            return new Group
            {
                title = title,
                style =
                {
                    backgroundColor = new Color(0.09f, 0.09f, 0.09f, 0.5f),
                }
            };
        }

        public void CreateNode(ShaderNode node, Vector2 position, bool transform = true)
        {
            if (_editorWindow)
            {
                _editorWindow.SetDirty();
            }
            if (transform) TransformMousePositionToLocalSpace(ref position, true);

            if (node is not PreviewNode)
            {
                node._previewDisabled = graphData.defaultPreviewState == GraphData.DefaultPreviewState.Disabled;
            }
            node.InitializeInternal(this, position);
            RecordUndo();
            AddElement(node);
            node.GeneratePreview();
        }

        public void CreateNode(Type type, Vector2 position, bool transform = true)
        {
            var node = (ShaderNode)Activator.CreateInstance(type);
            CreateNode(node, position, transform);
        }

        public ShaderNode AddNode(SerializableNode serializableNode)
        {
            if (serializableNode.TryDeserialize(this, out var shaderNode))
            {
                AddElement(shaderNode);
                return shaderNode;
            }

            return null;
        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var allPort = ports.ToArray();
            var compatiblePorts = new List<Port>(allPort.Length);
            var node = startPort.node;
            var direction = startPort.direction;
            var type = startPort.portType;

            foreach (var port in allPort)
            {
                if (startPort == port)
                {
                    continue;
                }

                if (node == port.node)
                {
                    continue;
                }

                if (direction == port.direction)
                {
                    continue;
                }

                if (type != port.portType)
                {
                    continue;
                }

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        public void TransformMousePositionToLocalSpace(ref Vector2 position, bool isSearchWindow)
        {
            if (isSearchWindow)
            {
                position -= _editorWindow.position.position;
            }
            position = contentViewContainer.WorldToLocal(position);
        }

        private KeyCode _lastKeyCode = KeyCode.None;
        private void NodeHotkeyKeyDown(KeyDownEvent e)
        {
            if (e.ctrlKey && e.keyCode == KeyCode.S)
            {
                _editorWindow.SaveChangesImpl(false);
                return;
            }
            var key = e.keyCode;
            if (key != KeyCode.None) _lastKeyCode = e.keyCode;
        }

        private void NodeHotkeyUpDown(KeyUpEvent evt)
        {
            _lastKeyCode = KeyCode.None;
        }

        List<string> _udonStates = new List<string>();
        int _undoCount = 0;
        public void RecordUndo()
        {
            Debug.Log("Record Undo");
            var graph = SerializableGraph.StoreGraph(this);
            var json = EditorJsonUtility.ToJson(graph, false);

            _udonStates.Add(json);
            _undoCount = _udonStates.Count;
        }


        private void UndoEvent(KeyDownEvent e)
        {
            if (e.target is not ShaderGraphView)
            {
                return;
            }


            if (!e.ctrlKey)
            {
                return;
            }

            var key = e.keyCode;

            if (key == KeyCode.Z)
            {
                e.StopPropagation();

                if (_undoCount <= 0)
                {
                    Debug.Log("Nothing to Undo");
                    return;
                }

                foreach (var element in graphElements)
                {
                    RemoveElement(element);
                }
                var graph = new SerializableGraph();
                _undoCount--;
                EditorJsonUtility.FromJsonOverwrite(_udonStates[_undoCount], graph);
                graph.PopulateGraph(this);
                ShaderBuilder.GenerateAllPreviews(this);

                Debug.Log("UNDO");
            }
            if (key == KeyCode.Y)
            {
                e.StopPropagation();
                if (_undoCount + 1 >= _udonStates.Count)
                {
                    Debug.Log("Nothing to Redo");
                    return;
                }
                foreach (var element in graphElements)
                {
                    RemoveElement(element);
                }
                var graph = new SerializableGraph();
                _undoCount++;
                EditorJsonUtility.FromJsonOverwrite(_udonStates[_undoCount], graph);
                graph.PopulateGraph(this);
                ShaderBuilder.GenerateAllPreviews(this);

                Debug.Log("REDO");
            }
        }


        private void NodeHotkey(ClickEvent e)
        {
            if (e.target is not ShaderGraphView || e.button != (int)MouseButton.LeftMouse)
            {
                return;
            }

            Vector2 localPosition = e.localPosition;
            Vector2 position = viewTransform.matrix.inverse.MultiplyPoint(localPosition);

            switch (_lastKeyCode)
            {
                case KeyCode.Alpha1: CreateNode(typeof(FloatNode), position, false); break;
                case KeyCode.Alpha2: CreateNode(typeof(Float2Node), position, false); break;
                case KeyCode.Alpha3: CreateNode(typeof(Float3Node), position, false); break;
                case KeyCode.Alpha4: CreateNode(typeof(Float4Node), position, false); break;
                case KeyCode.Alpha5: CreateNode(typeof(ColorNode), position, false); break;
                case KeyCode.Alpha6: CreateNode(typeof(Texture2DPropertyNode), position, false); break;
                case KeyCode.M: CreateNode(typeof(MultiplyNode), position, false); break;
                case KeyCode.A: CreateNode(typeof(AddNode), position, false); break;
                case KeyCode.Period: CreateNode(typeof(DotNode), position, false); break;
                case KeyCode.Z: CreateNode(typeof(SwizzleNode), position, false); break;
                case KeyCode.N: CreateNode(typeof(NormalizeNode), position, false); break;
                case KeyCode.O: CreateNode(typeof(OneMinusNode), position, false); break;
                case KeyCode.S: CreateNode(typeof(SubtractNode), position, false); break;
                case KeyCode.T: CreateNode(typeof(SampleTexture2DNode), position, false); break;
                case KeyCode.U: CreateNode(typeof(UVNode), position, false); break;
                case KeyCode.P: CreateNode(typeof(PreviewNode), position, false); break;
                case KeyCode.C: CreateNode(typeof(CustomFunctionNode), position, false); break;
                case KeyCode.B: CreateNode(typeof(SplitNode), position, false); break;
                case KeyCode.V: CreateNode(typeof(AppendNode), position, false); break;
                case KeyCode.L: CreateNode(typeof(LerpNode), position, false); break;
                case KeyCode.R: CreateNode(typeof(RegisterVariableNode), position, false); break;
                case KeyCode.G: CreateNode(typeof(FetchVariableNode), position, false); break;
                case KeyCode.Insert: CreateAll(); break;
            }

            //e.StopPropagation();
        }
        void CreateAll()
        {
            Vector2 p = new Vector2(0, 0);
            Type[] _existingNodeTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(type => typeof(ShaderNode).IsAssignableFrom(type))
                .Where(x =>
                {
                    var info = x.GetCustomAttribute<NodeInfo>();
                    return info is not null && !info.name.StartsWith("_");
                })
                .OrderBy(x => x.GetCustomAttribute<NodeInfo>().name)
                .ToArray();

            foreach (var type in _existingNodeTypes)
            {
                p = new Vector2(p.x + 200, p.y);
                if (p.x > 2000)
                {
                    p.x = 0;
                    p.y += 400;
                }
                CreateNode(type, p, false);
            }

            var customFunctions = AssetDatabase.FindAssets("l:GraphlitFunction");
            foreach (var guid in customFunctions)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                p = new Vector2(p.x + 200, p.y);
                if (p.x > 2000)
                {
                    p.x = 0;
                    p.y += 400;
                }
                var node = new CustomFunctionNode();
                node.UseFile(AssetDatabase.LoadAssetAtPath<ShaderInclude>(path));
                CreateNode(node, p, false);
            }
        }

        void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                if (DragAndDrop.objectReferences[0] is Texture2D
                    || DragAndDrop.objectReferences[0] is SubgraphObject)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.StopPropagation();
                }
            }
        }

        void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();


            var pos = evt.localMousePosition;
            TransformMousePositionToLocalSpace(ref pos, false);

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D texture)
                {
                    // Create a new node

                    var node = new Texture2DPropertyNode();
                    var sample = new SampleTexture2DNode();

                    var desc = new PropertyDescriptor(PropertyType.Texture2D, obj.name);
                    graphData.properties.Add(desc);
                    node.SetReference(desc.guid);
                    node.propertyDescriptor = desc;

                    desc.DefaultTextureValue = texture;

                    var asset = AssetDatabase.GetAssetPath(texture);
                    var importer = AssetImporter.GetAtPath(asset);

                    if (importer is TextureImporter textureImporter)
                    {
                        if (textureImporter.textureType == TextureImporterType.NormalMap)
                        {
                            desc.defaultAttributes |= MaterialPropertyAttribute.Normal;
                            desc.DefaultTextureEnum = DefaultTextureName.bump;
                        }
                        else if (!textureImporter.sRGBTexture)
                        {
                            desc.defaultAttributes |= MaterialPropertyAttribute.Linear;
                        }

                    }

                    CreateNode(node, pos, false);
                    CreateNode(sample, new Vector2(pos.x + 200, pos.y), false);

                    var texPort = node.Outputs.First(x => x.GetPortID() == 0);
                    var inPort = sample.Inputs.First(x => x.GetPortID() == 1);
                    var newEdge = texPort.ConnectTo(inPort);
                    AddElement(newEdge);
                    node.GeneratePreviewForAffectedNodes();
                    pos.y += 400;
                }
                else if (obj is SubgraphObject subgraphObj)
                {
                    var node = new SubgraphNode();
                    node.subgraph = subgraphObj;
                    CreateNode(node, pos, false);
                    node.ReinitializePorts();
                }
            }

            evt.StopPropagation();
        }

        public Dictionary<string, T> GetElementsGuidDictionary<T>() where T : GraphElement
        {
            var acceleratedGetNode = new Dictionary<string, T>();
            foreach (var item in graphElements.OfType<T>())
            {
                acceleratedGetNode[item.viewDataKey] = item;
            }
            return acceleratedGetNode;
        }
    }
}