using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using static UnityEditor.Experimental.GraphView.Port;

namespace ZSG
{
    public struct GeneratedPortData
    {
        public GeneratedPortData(IPortType type, string name)
        {
            Type = type;
            Name = name;
        }

        public IPortType Type;
        public string Name;
    }

    public enum Precision
    {
        Inherit = 0,
        Float = 1,
        Half = 2
    }

    public abstract class ShaderNode : Node
    {
        public void Initialize(ShaderGraphView graphView, Vector2 position, string guid = null)
        {
            base.SetPosition(new Rect(position, Vector3.one));
            if (guid is not null) viewDataKey = guid;
            GraphView = graphView;
            AddDefaultElements();
        }

        public ShaderGraphView GraphView { get; private set; }

        public NodeInfo Info => GetType().GetCustomAttribute<NodeInfo>(false);

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (!DisablePreview)
            {
                evt.menu.AppendAction("Preview/Generate", GeneratePreview);
                evt.menu.AppendAction("Preview/Enable", SetPreviewStateEnabled);
                evt.menu.AppendAction("Preview/Disable", SetPreviewStateDisabled);
            }
        }

        public void GeneratePreview(DropdownMenuAction action)
        {
            InvokeOnSelection(x => ShaderBuilder.GeneratePreview(GraphView, x, action != null));
        }
        public void GeneratePreview()
        {
            ShaderBuilder.GeneratePreview(GraphView, this, false);
        }

        void InvokeOnSelection(Action<ShaderNode> action)
        {
            foreach (var item in GraphView.selection)
            {
                if (item is ShaderNode node)
                {
                    action.Invoke(node);
                }
            }
        }

        public IEnumerable<Port> PortElements => inputContainer.Children().Concat(outputContainer.Children()).Where(x => x is Port).Cast<Port>();
        public IEnumerable<Port> Inputs => inputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Input);
        public IEnumerable<Port> Outputs => outputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Output);

        protected abstract void Generate(NodeVisitor visitor);

        public Dictionary<int, PortDescriptor> portDescriptors = new();
        public Port AddPort(PortDescriptor portDescriptor, bool addDescriptors = true)
        {
            if (addDescriptors)
            {
                portDescriptors.Add(portDescriptor.ID, portDescriptor);
            }

            var container = portDescriptor.Direction == PortDirection.Input ? inputContainer : outputContainer;

            var type = portDescriptor.Type.GetType();
            var capacity = portDescriptor.Direction == PortDirection.Input ? Capacity.Single : Capacity.Multi;

            var port = Port.Create<Edge>(Orientation.Horizontal, (Direction)portDescriptor.Direction, capacity, type);


            port.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
            port.portName = portDescriptor.Name;
            port.userData = portDescriptor.ID;
            if (portDescriptor.Type is Float @float)
            {
                var color = @float.GetPortColor();
                port.portColor = color;
            }
            else
            {
                port.portColor = portDescriptor.Type.GetPortColor();
            }

            container.Add(port);

            return port;
        }
        void Disconnect(Port port)
        {
            if (port.connected)
            {
                foreach (var edge in port.connections.ToArray())
                {
                    var input = edge.input;
                    var output = edge.output;

                    input.Disconnect(edge);
                    output.Disconnect(edge);
                }
            }
        }
        public void ResetPorts()
        {
            foreach ( var port in PortElements.ToArray())
            {
                var descriptors = portDescriptors.Where(x => x.Key == port.GetPortID()).ToArray();
                if (descriptors.Count() > 0)
                {
                    var descriptor = descriptors.First().Value;
                    var newType = descriptor.Type.GetType();
                    if (port.portType != newType)
                    {
                        port.portType = newType;
                        Disconnect(port);
                    }
                    port.portName = descriptor.Name;

                    if (descriptor.Type is Float @float)
                    {
                        var color = @float.GetPortColor();
                        port.portColor = color;
                    }
                    else
                    {
                        port.portColor = descriptor.Type.GetPortColor();
                    }
                    continue;
                }

                Disconnect(port);
                port.parent.Remove(port);
            }

            foreach (var desc in portDescriptors)
            {
                if (PortElements.Any(x => x.GetPortID() == desc.Key))
                {
                    continue;
                }
                AddPort(desc.Value, false);
            }

            //UpdateGraphView();
            //RefreshPorts();
        }

        public void GeneratePreviewForAffectedNodes()
        {
            ShaderBuilder.GeneratePreview(GraphView, this);

            foreach (var output in Outputs)
            {
                if (output.connected)
                {
                    foreach (var edge in output.connections)
                    {
                        ShaderBuilder.GeneratePreviewFromEdge(GraphView, edge, false);
                    }
                }
            }
        }

        public abstract void AddElements();
        public virtual int PreviewResolution => 96;
        protected internal PreviewType? _defaultPreview = null;
        public PreviewType DefaultPreview
        {
            get
            {
                if (_defaultPreview is PreviewType preview)
                {
                    return preview;
                }
                return DefaultPreviewOverride;
            }
            internal set
            {
                _defaultPreview = value;
            }
        }
        public virtual PreviewType DefaultPreviewOverride => PreviewType.Inherit;
        protected internal PreviewType _inheritedPreview;

        protected internal bool _previewDisabled = false;
        public virtual bool DisablePreview => false;

        protected internal Precision? _defaultPrecision = null;
        public Precision DefaultPrecision
        {
            get
            {
                if (_defaultPrecision is Precision precision)
                {
                    return precision;
                }
                return DefaultPrecisionOverride;
            }
            internal set
            {
                _defaultPrecision = value;
            }
        }
        public virtual Precision DefaultPrecisionOverride => Precision.Inherit;
        protected internal bool _inheritedPrecision;

        public virtual void InheritPreviewAndPrecision()
        {
            int is3D = 0;
            int is2D = 0;

            int connectedCount = 0;

            bool inheritedPrecisionIsFloat = false;

            foreach (var port in Inputs)
            {
                if (!port.connected)
                {
                    continue;
                }

                foreach(var edge in port.connections)
                {
                    var node = (ShaderNode)edge.output.node;
                    if (node is null)
                    {
                        continue;
                    }
                    is3D += node._inheritedPreview == PreviewType.Preview3D ? 1 : 0;
                    is2D += node._inheritedPreview == PreviewType.Preview2D ? 1 : 0;

                    //Debug.Log(node.inheritedPreview);
                    connectedCount++;

                    inheritedPrecisionIsFloat |= node._inheritedPrecision;
                    break;
                }
            }


            if (connectedCount == 0)
            {
                _inheritedPreview = DefaultPreview;
                inheritedPrecisionIsFloat = GraphView.graphData.precision == GraphData.GraphPrecision.Float;
            }
            else if (is2D == 0 && is3D == 0)
            {
                _inheritedPreview = PreviewType.Preview2D;
            }
            else if (is3D > 0)
            {
                _inheritedPreview = PreviewType.Preview3D;
            }
            else
            {
                _inheritedPreview = PreviewType.Preview2D;
            }

            if (DefaultPrecision == Precision.Float)
            {
                _inheritedPrecision = true;
            }
            else if (DefaultPrecision == Precision.Half)
            {
                _inheritedPrecision = false;
            }
            else
            {
                _inheritedPrecision = inheritedPrecisionIsFloat;
            }

            if (DefaultPreview != PreviewType.Inherit)
            {
                _inheritedPreview = DefaultPreview;
            }

            //Debug.Log(inheritedPreview + Info.name);
        }

        private void AddDefaultElements()
        {

            AddStyles();
            AddTitleElement();
            AddElements();
            if (!DisablePreview)
            {
                AddPreview();
            }


            RefreshExpandedState();
            RefreshPorts();
        }
        public virtual Color Accent => Color.gray;
        void AddStyles()
        {
            var color = new Color(0.07f, 0.07f, 0.07f, 1);
            extensionContainer.style.backgroundColor = color;
            inputContainer.style.backgroundColor = color;
            outputContainer.style.backgroundColor = color;

            var accentLine = new VisualElement();
            {
                var s = accentLine.style;
                accentLine.StretchToParentSize();
                s.height = 2;
                s.backgroundColor = Accent;
                s.top = 22;
            }
            titleContainer.Add(accentLine);
        }
        public Label TitleLabel;
        void AddTitleElement()
        {
            /*if (LowProfile)
            {
                titleContainer.parent.Remove(titleContainer);
                return;
            }*/
            var nodeInfo = Info;

            var titleLabel = (Label)titleContainer.Q("title-label");
            titleLabel.text = DisplayName;
            titleLabel.tooltip = nodeInfo.tooltip + "\n" + viewDataKey;
            titleLabel.style.fontSize = 12;
            titleLabel.style.marginRight = StyleKeyword.Auto;
            titleLabel.style.marginLeft = StyleKeyword.Auto;
            titleLabel.style.paddingLeft = 6;
            titleLabel.style.paddingRight = 6;
            titleContainer.Insert(0, titleLabel);
            TitleLabel = titleLabel;

            var titleButton = titleContainer.Q("title-button-container");
            titleButton.parent.Remove(titleButton);

            var titleStyle = titleContainer.style;
            titleStyle.height = 24;
            titleStyle.backgroundColor = Color.black;

            /*var precisionLabel = new IMGUIContainer(OnTitleContainerGUI);
            precisionLabel.style.opacity = 0.2f;
            precisionLabel.style.marginLeft = 6;
            titleContainer.Insert(0, precisionLabel);*/
        }

       /* void OnTitleContainerGUI()
        {
            EditorGUILayout.BeginHorizontal();
            switch (DefaultPrecision)
            {
                case Precision.Inherit: EditorGUILayout.LabelField("I", GUILayout.Width(15)); break;
                case Precision.Half: EditorGUILayout.LabelField("H", GUILayout.Width(15)); break;
                case Precision.Float: EditorGUILayout.LabelField("F", GUILayout.Width(15)); break;
            }
            EditorGUILayout.EndHorizontal();
        }*/

        public static int UniqueVariableID = 0;
        public Dictionary<int, GeneratedPortData> PortData { get; set; } = new();
        GeneratedPortData GetInputPortData(int portID, NodeVisitor visitor)
        {
            var port = Inputs.Where(x => x.GetPortID() == portID).First();
            if (port.connected)
            {
                var incomingPort = port.connections.First().output;
                var incomingNode = (ShaderNode)incomingPort.node;

                return incomingNode.PortData[incomingPort.GetPortID()];
            }
            else
            {
                return GetDefaultInput(portID, visitor);
            }
        }

        GeneratedPortData GetDefaultInput(int portID, NodeVisitor visitor)
        {
            var descriptor = portDescriptors[portID];
            string value = SetDefaultBinding(descriptor, visitor);
            return new GeneratedPortData(descriptor.Type, value);
        }

        Dictionary<int, PortBinding> _portBindings = new();

        protected void Bind(int id, PortBinding binding)
        {
            _portBindings[id] = binding;
        }

        string SetDefaultBinding(PortDescriptor portDescriptor, NodeVisitor visitor)
        {
            int id = portDescriptor.ID;
            if (_portBindings.ContainsKey(id))
            {
                var binding = _portBindings[id];
                var pass = visitor._shaderBuilder.passBuilders[visitor.Pass];

                if (portDescriptor.Type is Float @float)
                {
                    return PortBindings.GetBindingString(pass, visitor.Stage, @float.components, binding);
                }
            }
            else if (portDescriptor.Type is Float @float)
            {
                int c = @float.components;
                return c switch
                {
                    1 => PrecisionString(1) + "(0)",
                    2 => PrecisionString(2) + "(0, 0)",
                    3 => PrecisionString(3) + "(0, 0, 0)",
                    4 or _ => PrecisionString(4) + "(0, 0, 0, 0)",
                };
            }
            else if (portDescriptor.Type is Texture2DObject)
            {
                return "nullTexture";
            }
            else if (portDescriptor.Type is SamplerState)
            {
                return "null_LinearRepeat"; 
            }
            else if (portDescriptor.Type is Bool)
            {
                return "false";
            }
            else if (portDescriptor.Type is Int)
            {
                return "int(0)";
            }
            else if (portDescriptor.Type is UInt)
            {
                return "uint(0)";
            }

            return PrecisionString(4) + "(0,0,0,0)";
        }

        internal void BuilderVisit(NodeVisitor visitor, int[] portsMask = null)
        {
            InheritPreviewAndPrecision();
            var stage = visitor.Stage;

            if (stage == ShaderStage.Vertex)
            {
                _inheritedPrecision = true;
            }

            foreach (var descriptor in portDescriptors.Values)
            {
                int id = descriptor.ID;

                if (portsMask is not null && !portsMask.Contains(id))
                {
                    continue;
                }

                if (descriptor.Direction == PortDirection.Input)
                {
                    var newData = GetInputPortData(id, visitor);

                    if (newData.Type is Float incomingFloat && descriptor.Type is Float resultFloat)
                    {
                        // automatic cast
                        if (!resultFloat.dynamic && resultFloat.components != incomingFloat.components)
                        {
                            PortData[id] = newData;
                            newData = Cast(id, resultFloat.components, false);
                        }

                        if (resultFloat.dynamic)
                        {
                            resultFloat.components = incomingFloat.components;
                        }

                        newData.Type = resultFloat;
                    }

                    PortData[id] = newData;
                }
                else
                {
                    var name = SetDefaultBinding(descriptor, visitor);
                    var type = descriptor.Type;
                    PortData[id] = new GeneratedPortData(type, name);
                }
            }

            Generate(visitor);
        }
        string DisplayName => Info.name.Split("/")[^1];
        public string UniqueVariable => DisplayName.Replace(" ", "") + UniqueVariableID++;
        public void SetVariable(int id, string name)
        {
            var data = PortData[id];
            data.Name = name;
            PortData[id] = data;
        }
        public string PrecisionString(int component)
        {
            string precisionString = _inheritedPrecision ? "float" : "half";
            if (component == 1) return precisionString;
            if (component > 4 || component < 0) return "error" + component;
            return precisionString + component;
        }

        public void ChangeComponents(int id, int components)
        {
            var data = PortData[id];
            if (data.Type is Float @float)
            {
                @float.components = components;
                data.Type = @float;
            }
            PortData[id] = data;
        }
        public void Output(NodeVisitor visitor, int outID, string line)
        {
            SetVariable(outID, UniqueVariable);

            var data = PortData[outID];
            var type = (Float)data.Type;
            visitor.AppendLine($"{PrecisionString(type.components)} {data.Name} = {line};");
        }

        public Float ImplicitTruncation(params int[] IDs)
        {
            int trunc = 4;
            int max = 1;

            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                var type = (Float)PortData[ID].Type;
                var components = type.components;
                if (components == 1)
                {
                    continue;
                }
                max = Mathf.Max(max, components);
                trunc = Mathf.Min(trunc, components);
            }
            trunc = Mathf.Min(trunc, max);

            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                Cast(ID, trunc);
            }


            return new Float(trunc);
        }
        public int GetComponents(int id)
        {
            var type = (Float)PortData[id].Type;
            return type.components;
        }

        public GeneratedPortData Cast(int portID, int targetComponent, bool updatePort = true)
        {
            var data = PortData[portID];
            var name = data.Name;
            var type = (Float)PortData[portID].Type;
            var components = type.components;
            string typeName = _inheritedPrecision ? "float" : "half";

            if (components == targetComponent)
            {
                return data;
            }

            // downcast
            if (components > targetComponent)
            {
                name = name + ".xyz"[..(targetComponent + 1)];
            }
            else
            {
                // upcast
                if (components == 1)
                {
                    // no need to upcast
                    // name = "(" + name + ").xxxx"[..(targetComponent + 2)];
                    return data;
                }
                else if (components == 2)
                {
                    if (targetComponent == 3)
                    {
                        name = typeName + "3(" + name + ", 0)";
                    }
                    if (targetComponent == 4)
                    {
                        name = typeName + "4(" + name + ", 0, 0)";
                    }
                }
                else if (components == 3)
                {
                    if (targetComponent == 4)
                    {
                        name = typeName + "4(" + name + ", 0)";
                    }
                }
            }

            type.components = targetComponent;
            var newData = new GeneratedPortData(type, name);
            if (updatePort) PortData[portID] = newData;

            return newData;
        }

        public void UpdateGraphView()
        {
            foreach (var data in PortData)
            {
                var ports = PortElements.Where(x => x.GetPortID() == data.Key).ToArray();
                if (ports.Length < 1)
                {
                    continue;
                }
                var port = ports.First();
                int portID = port.GetPortID();
                var generatedData = data.Value;
                if (generatedData.Type is Float @float)
                {
                    var color = @float.GetPortColor();
                    port.portColor = color;

                    // caps not getting updated
                    var caps = port.Q("connector");
                    if (caps is not null)
                    {
                        caps.style.borderBottomColor = color;
                        caps.style.borderTopColor = color;
                        caps.style.borderLeftColor = color;
                        caps.style.borderRightColor = color;
                    }
                }
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (GraphView.additionalNodeElements.childCount < 10)
            {
                DefaultAdditionalElements(GraphView.additionalNodeElements);
            }
        }

        public override void OnUnselected()
        {
            GraphView.additionalNodeElements.Clear();
        }

        private void DefaultAdditionalElements(VisualElement root)
        {
            var borderColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

            var ve = new VisualElement();
            var nodeInfo = Info;
            var title = new Label(" " + DisplayName);
            title.style.backgroundColor = borderColor;
            title.style.fontSize = 18;
            title.style.height = 24;
            title.style.borderTopLeftRadius = 2;
            title.style.borderTopRightRadius = 2;
            //title.style.marginLeft = StyleKeyword.Auto;
            //title.style.marginRight = StyleKeyword.Auto;
            ve.Add(title);
            var style = ve.style;
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

            style.borderLeftColor = borderColor;
            style.borderRightColor = borderColor;
            style.borderTopColor = borderColor;
            style.borderBottomColor = borderColor;

            const int w = 3;
            style.borderLeftWidth = w;
            style.borderRightWidth = w;
            style.borderTopWidth = w;
            style.borderBottomWidth = w;

            const int r = 8;
            style.borderBottomLeftRadius = r;
            style.borderTopLeftRadius = r;
            style.borderTopRightRadius = r;
            style.borderBottomRightRadius = r;

            style.marginTop = 5;
            style.marginRight = 5;
            style.bottom = 5;

            if (this is not TemplateOutput)
            {
                var precisionSelection = new EnumField("Precision", DefaultPrecision);
                precisionSelection.RegisterValueChangedCallback(x => DefaultPrecision = (Precision)x.newValue);
                ve.Add(precisionSelection);
            }

            if (!DisablePreview)
            {
                var previewSelection = new EnumField("Preview", DefaultPreview);
                previewSelection.RegisterValueChangedCallback(x =>
                {
                    DefaultPreview = (PreviewType)x.newValue;
                    GeneratePreviewForAffectedNodes();
                });
                ve.Add(previewSelection);
            }

            AdditionalElements(ve);
            root.Add(ve);
        }

        public virtual void AdditionalElements(VisualElement root)
        {
        }
        void SetPreviewState(bool enabled)
        {
            _previewDisabled = !enabled;

            if (previewDrawer is null)
            {
                return;
            }

            if (enabled)
            {
                GeneratePreview();
                previewDrawer.Enable();
            }
            else
            {
                previewDrawer.Disable();
            }
        }

        void SetPreviewStateEnabled(DropdownMenuAction action) => InvokeOnSelection(x => x.SetPreviewState(true));
        void SetPreviewStateDisabled(DropdownMenuAction action) => InvokeOnSelection(x => x.SetPreviewState(false));

        public PreviewDrawer previewDrawer;
        void AddPreview()
        {
            previewDrawer = new PreviewDrawer(GraphView, PreviewResolution);
            extensionContainer.Add(previewDrawer);
            if (_previewDisabled)
            {
                previewDrawer.Disable();
            }
        }

        public Action<Material> onUpdatePreviewMaterial = (mat) => { };

        public void UpdatePreviewMaterial()
        {
            /*foreach (var material in PreviewDrawer.materials)
            {
                if (material is null) continue;
                onUpdatePreviewMaterial(material);
            }*/
            onUpdatePreviewMaterial(GraphView.PreviewMaterial);
            //PreviewDrawer.SetProperties -= (mat) => onUpdatePreviewMaterial(mat);
            //PreviewDrawer.SetProperties += (mat) => onUpdatePreviewMaterial(mat);
        }
    }

    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        public void OnDrop(GraphView graphView, Edge edge)
        {
            //            ShaderBuilder.GenerateAllPreviews((ShaderGraphView)graphView);
            ShaderBuilder.GeneratePreviewFromEdge((ShaderGraphView)graphView, edge, false);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            //var shaderNodeIn = (ShaderNode)edge.input.node;
            //var shaderNodeOut = (ShaderNode)edge.output.node;

            //shaderNodeIn.GeneratePreview(null);
            //shaderNodeOut.GeneratePreview(null);
        }
    }
}
