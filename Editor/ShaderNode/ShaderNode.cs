﻿using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace Graphlit
{
    /* class DefaultValueElement : GraphElement
     {
         public DefaultValueElement()
         {
             var s = style;
             name = "default-value";
             s.height = 15;
             const float C = 1.0f / 10f;
             s.backgroundColor = new Color(C, C, C);
             pickingMode = PickingMode.Ignore;
             s.borderTopRightRadius = 0;
             s.borderBottomRightRadius = 0;
             s.position = Position.Absolute;
             s.fontSize = 10;
             s.right = 86;
             //s.flexGrow = 1;
             //s.flexDirection = FlexDirection.RowReverse;
            // s.alignContent = Align.FlexEnd;

             var text = new Label("Value");
             text.style.fontSize = 8;
             //text.StretchToParentSize();

             //text.style.position = Position.Relative;
             Add(text);
         }
     }*/
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
        public void InitializeInternal(ShaderGraphView graphView, Vector2 position, string guid = null)
        {
            UniqueVariableID = graphView.uniqueID++.ToString(System.Globalization.CultureInfo.InvariantCulture);
            SetPosition(position);
            if (guid is not null) viewDataKey = guid;
            GraphView = graphView;
            AddDefaultElements();
        }

        public void SetPosition(Vector2 position)
        {
            base.SetPosition(new Rect(position, Vector3.one));
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
            if (this is IConvertablePropertyNode convertableNode)
            {
                evt.menu.AppendAction("Convert To Property", (action) =>
                {
                    var newNode = convertableNode.ToProperty();
                    var previousPort = Outputs.First();
                    var previousConnections = previousPort.connections.ToArray();
                    var wasConnected = previousPort.connected;

                    var newSerializableNode = new SerializableNode(newNode);
                    var prevSerializableNode = new SerializableNode(this)
                    {
                        type = newSerializableNode.type,
                        data = newSerializableNode.data
                    };

                    foreach (var port in Outputs)
                    {
                        Disconnect(port);
                    }
                    GraphView.RemoveElement(this);

                    var addedNode = GraphView.AddNode(prevSerializableNode);

                    if (wasConnected)
                    {
                        foreach (var con in previousConnections)
                        {
                            var newEdge = addedNode.Outputs.First().ConnectTo(con.input);
                            GraphView.AddElement(newEdge);
                        }
                    }

                    CleanLooseEdges();
                });
            }


            if (this is PropertyNode propNode)
            {
                evt.menu.AppendAction("Delete Property", (action) =>
                {
                    var graphData = GraphView.graphData;
                    var exists = graphData.properties.Find(x => x.guid == propNode._ref);
                    if (exists != null)
                    {
                        graphData.properties.Remove(exists);
                    }
                    foreach (var port in Outputs)
                    {
                        Disconnect(port);
                    }
                    GraphView.RemoveElement(this);
                    CleanLooseEdges();
                });
            }
        }

        public void GeneratePreview(DropdownMenuAction action)
        {
            InvokeOnSelection(x => x.GeneratePreviewForAffectedNodes());
        }
        public void GeneratePreview()
        {
            GeneratePreviewForAffectedNodes();
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

        public IEnumerable<Port> PortElements => Inputs.Concat(Outputs);
        public virtual IEnumerable<Port> Inputs => inputContainer.Children().OfType<Port>();
        public virtual IEnumerable<Port> Outputs => outputContainer.Children().OfType<Port>();

        protected abstract void Generate(NodeVisitor visitor);

        public Dictionary<int, PortDescriptor> portDescriptors = new();
        public Port AddPort(PortDescriptor portDescriptor, bool addDescriptors = true, string displayName = "")
        {
            if (addDescriptors)
            {
                portDescriptors.Add(portDescriptor.ID, portDescriptor);
            }

            var container = portDescriptor.Direction == PortDirection.Input ? inputContainer : outputContainer;

            var type = portDescriptor.Type.GetType();
            var capacity = portDescriptor.Direction == PortDirection.Input ? Capacity.Single : Capacity.Multi;

            var port = Port.Create<Edge>(Orientation.Horizontal, (Direction)portDescriptor.Direction, capacity, type);

            var label = port.Q<Label>("type");
            label.pickingMode = PickingMode.Position;


            /*            if (port.direction == Direction.Input)
                        {
                            var defaultValueElement = new DefaultValueElement();
                            port.Insert(0, defaultValueElement);
                        }
            */

            port.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
            port.portName = string.IsNullOrEmpty(displayName) ? portDescriptor.Name : displayName;
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

           /* if (this is TemplateOutput o && o.TallOutputs)
            {
                port.style.height = 38;
                port.style.SetBorderWidth(2);
                port.style.SetBorderRadius(8);
                port.style.SetBorderColor(new Color(0.15f, 0.15f, 0.15f));
                port.style.marginTop = 8;
                port.style.marginLeft = 8;
                port.style.marginRight = 8;


                port.Q<Label>().style.fontSize = 12;
            }*/


            return port;
        }
        internal void Disconnect(Port port)
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

        public void CleanLooseEdges()
        {
            var edges = GraphView.graphElements.OfType<Edge>();
            foreach (var edge in edges)
            {
                if (!(edge.input.connected && edge.output.connected))
                {
                    edge.parent.Remove(edge);
                }
            }
        }
        // store connections, disconnect and reconnect
        public void SafeModifyPortElements(Action a)
        {
            var connections = new Dictionary<int, (string, int)>();
            
            foreach (var portElement in PortElements)
            {
                foreach (var connection in portElement.connections)
                {
                    var port = portElement.direction == Direction.Input ? connection.output : connection.input;
                    connections[portElement.GetPortID()] = (port.node.viewDataKey, port.GetPortID());
                }
            }

            foreach (var port in PortElements.ToArray())
            {
                Disconnect(port);
                port.parent.Remove(port);
            }
            CleanLooseEdges();
            portDescriptors.Clear();
            _portBindings.Clear();

            a.Invoke();
            
            foreach (var connection in connections)
            {
                var port = PortElements.FirstOrDefault(x => x.GetPortID() == connection.Key);
                if (port is null)
                {
                    continue;
                }

                var guid = connection.Value.Item1;
                var connectedID = connection.Value.Item2;
                var connectedNode = GraphView.graphElements.OfType<ShaderNode>().FirstOrDefault(x => x.viewDataKey == guid);
                if (connectedNode is null)
                {
                    continue;
                }

                var connectedPort = connectedNode.PortElements.FirstOrDefault(x => x.GetPortID() == connectedID);
                if (connectedPort is null)
                {
                    continue;
                }
                var newEdge = port.ConnectTo(connectedPort);
                GraphView.AddElement(newEdge);
            }
        }

        // what the fuck does this do
        public void ResetPorts()
        {
            foreach (var port in PortElements.ToArray())
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

        public HashSet<string> GeneratePreviewForAffectedNodes()
        {
            var rightNodesAdded = new HashSet<string>();
            var rightNodes = new List<ShaderNode>();

            foreach (var output in Outputs)
            {
                foreach (var edge in output.connections)
                {
                    ShaderBuilder.GetConnectedNodesFromEdgeRight(rightNodes, edge, rightNodesAdded);
                }
            }

            ShaderBuilder.GenerateUnifiedPreview(GraphView, this, rightNodes);
            return rightNodesAdded;
        }

        public void InheritPreviewTypeForAffectedNodes()
        {
            var nodesToGenerate = new List<ShaderNode>();
            var nodesAdded = new HashSet<string>();

            foreach (var output in Outputs)
            {
                if (output.connected)
                {
                    foreach (var edge in output.connections)
                    {
                        ShaderBuilder.GetConnectedNodesFromEdgeRight(nodesToGenerate, edge, nodesAdded);
                    }
                }
            }

            InheritPreviewAndPrecision();

            foreach (var node in nodesToGenerate)
            {
                node.InheritPreviewAndPrecision();
            }
        }

        public abstract void Initialize();
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
        internal PreviewType _inheritedPreview;

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

        public void InheritPreviewAndPrecision()
        {
            int is3D = 0;
            int is2D = 0;

            int connectedCount = 0;

            bool inheritedPrecisionIsFloat = false;



            foreach (var port in Inputs)
            {
                int id = port.GetPortID();
                if (!port.connected)
                {
                    continue;
                }

                foreach (var edge in port.connections)
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
            Initialize();
            if (!DisablePreview)
            {
                AddPreview();
            }


            RefreshExpandedState();
            RefreshPorts();
        }
        public virtual Color Accent => Color.gray;

        static Color _color1 = new Color(0.08f, 0.08f, 0.08f, 1);
        static Color _color0 = new Color(0.05f, 0.05f, 0.05f, 1);
        //static Color _color2 = new Color(0.2f, 0.2f, 0.2f, 1);

        void AddStyles()
        {
            extensionContainer.style.backgroundColor = _color0;
            inputContainer.style.backgroundColor = this is TemplateOutput ? _color0 : _color1;
            outputContainer.style.backgroundColor = _color0;

            var accentLine = new VisualElement();
            {
                var s = accentLine.style;
                accentLine.StretchToParentSize();
                s.height = 2;
                s.backgroundColor = Accent;
                s.top = 22;
            }
            titleContainer.Add(accentLine);

            var div1 = topContainer.Q("divider");
            div1.parent.Remove(div1);

            var div2 = extensionContainer.parent.Q("divider");
            div2.parent.Remove(div2);


            var divider = contentContainer.Q("divider");
            divider.parent.Remove(divider);

            var border = this.Q("node-border");
            border.style.SetBorderWidth(0);
            //border.style.display = DisplayStyle.None;

            /*
            border.style.overflow = Overflow.Visible;
            {
                var s = titleContainer.style;
                s.borderTopLeftRadius = 6;
                s.borderTopRightRadius = 6;
            }*/
        }
        public TextElement TitleLabel;

        //float _lastClickTime = 0;

        public string GetTitleTooltip() => GetType().Name + "\n" + viewDataKey;
        void AddTitleElement()
        {
            //var nodeInfo = Info;

            var previousLabel = titleContainer.Q("title-label");
            var parent = previousLabel.parent;
            parent.Remove(previousLabel);

            var titleLabel = new Label() {
                text = DisplayName,
                tooltip = GetTitleTooltip(),
                style = {
                    fontSize = 12,
                    marginRight = StyleKeyword.Auto,
                    marginLeft = StyleKeyword.Auto,
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 4,
                    backgroundColor = Color.clear,
                }
            };

            titleLabel.style.SetBorderWidth(0);

            titleContainer.Insert(0, titleLabel);
            TitleLabel = titleLabel;

            var titleButton = titleContainer.Q("title-button-container");
            titleButton.parent.Remove(titleButton);

            var titleStyle = titleContainer.style;
            titleStyle.height = 24;

            //var color = new Color(0.07f, 0.07f, 0.07f, 1);
            titleStyle.backgroundColor = _color0;
        }

        public string UniqueVariableID { get; private set; }
        public Dictionary<int, GeneratedPortData> PortData { get; set; } = new();
        public GeneratedPortData GetInputPortData(int portID, NodeVisitor visitor)
        {
            var port = Inputs.FirstOrDefault(x => x.GetPortID() == portID);
            if (port is not null && port.connected)
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


        internal GeneratedPortData GetDefaultInput(int portID, NodeVisitor visitor)
        {
            var descriptor = portDescriptors[portID];
            string value = SetDefaultBinding(descriptor, visitor);

            UpdateDefaultValueTooltip(portID, value);

            return new GeneratedPortData(descriptor.Type, value);
        }

        internal protected Dictionary<int, PortBinding> _portBindings = new();

        internal void Bind(int id, PortBinding binding)
        {
            _portBindings[id] = binding;
        }

        string SetDefaultBinding(PortDescriptor portDescriptor, NodeVisitor visitor)
        {
            int id = portDescriptor.ID;
            if (DefaultValues.ContainsKey(id))
            {
                return DefaultValues[id];
            }

            if (_portBindings.ContainsKey(id))
            {
                var binding = _portBindings[id];
                var pass = visitor._shaderBuilder.passBuilders[visitor.Pass];

                if (portDescriptor.Type is Float @float)
                {
                    return PortBindings.GetBindingString(pass, visitor.Stage, @float.dimensions, binding);
                }
                else
                {
                    return PortBindings.GetBindingString(pass, visitor.Stage, 1, binding);
                }
            }
            else if (portDescriptor.Type is Float @float)
            {
                int c = @float.dimensions;
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


        public Dictionary<int, string> DefaultValues { get; set; } = new ();

        public void UpdateDefaultValueTooltip(int id, string tooltip)
        {
            var port = PortElements.Where(x => x.GetPortID() == id).FirstOrDefault();
            if (port is not null)
            {
                var label = port.Q<Label>("type");
                label.tooltip = tooltip;
            }
        }

        internal void BuilderVisit(NodeVisitor visitor, int[] portsMask = null)
        {
            DefaultVisit(visitor, portsMask);
            Generate(visitor);
            UpdateGraphViewDimensions();
        }

        internal void DefaultVisit(NodeVisitor visitor, int[] portsMask = null)
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
                        if (!resultFloat.dynamic && resultFloat.dimensions != incomingFloat.dimensions)
                        {
                            PortData[id] = newData;
                            newData = Cast(id, resultFloat.dimensions, false);
                        }

                        if (resultFloat.dynamic)
                        {
                            resultFloat.dimensions = incomingFloat.dimensions;
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
        }

        string DisplayName => Info.name.Split("/")[^1];
        public string UniqueVariable => DisplayName.Replace(" ", "") + UniqueVariableID;
        public void SetVariable(int id, string name)
        {
            var data = PortData[id];
            data.Name = name;
            PortData[id] = data;
        }
        public string PrecisionString(int dimensions)
        {
            string precisionString = _inheritedPrecision ? "float" : "half";
            if (dimensions == 1) return precisionString;
            if (dimensions > 4 || dimensions < 0) return "error" + dimensions;
            return precisionString + dimensions;
        }

        public void ChangeDimensions(int id, int dimensions)
        {
            var data = PortData[id];
            if (data.Type is Float @float)
            {
                @float.dimensions = dimensions;
                data.Type = @float;
            }
            PortData[id] = data;
        }
        public void Output(NodeVisitor visitor, int outID, string line, string suffix = "")
        {
            SetVariable(outID, UniqueVariable + suffix);

            var data = PortData[outID];
            var type = (Float)data.Type;
            visitor.AppendLine($"{PrecisionString(type.dimensions)} {data.Name} = {line};");
        }

        public Float ImplicitTruncation(params int[] IDs)
        {
            int trunc = 4;
            int max = 1;

            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                var type = (Float)PortData[ID].Type;
                var dimensions = type.dimensions;
                if (dimensions == 1)
                {
                    continue;
                }
                max = Mathf.Max(max, dimensions);
                trunc = Mathf.Min(trunc, dimensions);
            }
            trunc = Mathf.Min(trunc, max);

            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                Cast(ID, trunc);
            }


            return new Float(trunc);
        }


        public int GetDimensions(int id)
        {
            var type = (Float)PortData[id].Type;
            return type.dimensions;
        }

        public GeneratedPortData Cast(int portID, int targetDimensions, bool updatePort = true)
        {
            var data = PortData[portID];
            var name = data.Name;
            var type = (Float)PortData[portID].Type;
            var dimensions = type.dimensions;
            string typeName = _inheritedPrecision ? "float" : "half";

            if (dimensions == targetDimensions)
            {
                return data;
            }

            // downcast
            if (dimensions > targetDimensions)
            {
                name = name + ".xyz"[..(targetDimensions + 1)];
            }
            else
            {
                // upcast
                if (dimensions == 1)
                {
                    // no need to upcast
                    // name = "(" + name + ").xxxx"[..(targetDimensions + 2)];
                    //return data;
                }
                else if (dimensions == 2)
                {
                    if (targetDimensions == 3)
                    {
                        name = typeName + "3(" + name + ", 0)";
                    }
                    if (targetDimensions == 4)
                    {
                        name = typeName + "4(" + name + ", 0, 0)";
                    }
                }
                else if (dimensions == 3)
                {
                    if (targetDimensions == 4)
                    {
                        name = typeName + "4(" + name + ", 0)";
                    }
                }
            }

            type.dimensions = targetDimensions;
            var newData = new GeneratedPortData(type, name);
            if (updatePort) PortData[portID] = newData;

            return newData;
        }

        public void UpdateGraphViewDimensions()
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
                    SetPortColor(port, color);
                }
            }

            if (this is FetchVariableNode fetch)
            {
                var reg = GraphView.cachedRegisterVariablesForBuilder.FirstOrDefault(x => x._name == fetch._name);
                if (reg is not null)
                {
                    SetPortColor(reg.Inputs.First(), Outputs.First().portColor);
                }
            }
        }

        public static void SetPortColor(Port port, Color color)
        {
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

            foreach (var edge in port.connections)
            {
                var control = edge.Q<EdgeControl>();
                if (control is null)
                {
                    continue;
                }
                if (port.direction == Direction.Input)
                {
                    control.inputColor = color;
                }
                else
                {
                    control.outputColor = color;
                }
            }

        }

        public override void OnSelected()
        {
            base.OnSelected();
            const int MaxSelection = 3;
            if (GraphView.selection.OfType<ShaderNode>().Count() <= MaxSelection)
            {
                DefaultAdditionalElements(GraphView.additionalNodeElements);
            }
            else
            {
                GraphView.additionalNodeElements.Clear();
            }
        }

        public override void OnUnselected()
        {
            GraphView.additionalNodeElements.Clear();
        }

        private void DefaultAdditionalElements(VisualElement root)
        {
            var borderColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

            var ve = new ScrollView();
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

            // if (!DisablePreview && previewDrawer is not null && previewDrawer.HasShader)
            // {
            //     ve.Add(previewDrawer.GetExtensionPreview(this));
            // }

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
                    // GeneratePreviewForAffectedNodes();
                    InheritPreviewTypeForAffectedNodes();
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
            previewDrawer = new PreviewDrawer(this, GraphView, PreviewResolution);
            extensionContainer.Add(previewDrawer);
            if (_previewDisabled)
            {
                previewDrawer.Disable();
            }

            const string disabledText = "▲";
            const string enabledText = "▼";


            var previewToggle = new Button()
            {
                text = !_previewDisabled ? disabledText : enabledText,
                style = {
                    marginBottom = 0,
                    marginLeft = 0,
                    marginTop = 0,
                    marginRight = 0,
                    backgroundColor = Color.clear,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    height = 12,
                    fontSize = 8,
                    color = new Color(0.25f, 0.25f, 0.25f),


                },
            };
            previewToggle.style.SetBorderRadius(0);
            previewToggle.clicked += () =>
            {
                previewToggle.text = _previewDisabled ? disabledText : enabledText;
                SetPreviewState(_previewDisabled);
            };
            extensionContainer.Add(previewToggle);
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

            if (GraphView.ImportedMaterial != null)
            {
                onUpdatePreviewMaterial(GraphView.ImportedMaterial);
            }
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
