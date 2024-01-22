using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public NodeInfo Info => GetType().GetCustomAttribute<NodeInfo>();

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Generate Preview", GeneratePreview);
            evt.menu.AppendAction("Remove Preview", RemovePreview);
            evt.menu.AppendAction("Preview 3D Toggle", Preview3DToggle);
        }

        public void Preview3DToggle(DropdownMenuAction action)
        {
            preview3D = !preview3D;
            GeneratePreviewForAffectedNodes();
        }
        public void GeneratePreview(DropdownMenuAction action)
        {
            ShaderBuilder.GeneratePreview(GraphView, this, action != null);
        }
        public void RemovePreview(DropdownMenuAction action)
        {
            var d = extensionContainer.Q("PreviewDrawer");
            extensionContainer.Remove(d);
            previewDrawer.Dispose();
        }

        public IEnumerable<Port> PortElements => inputContainer.Children().Concat(outputContainer.Children()).Where(x => x is Port).Cast<Port>();
        public IEnumerable<Port> Inputs => inputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Input);
        public IEnumerable<Port> Outputs => outputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Output);

        protected abstract void Generate(NodeVisitor visitor);

        public Dictionary<int, PortDescriptor> portDescriptors = new();
        public Port AddPort(PortDescriptor portDescriptor)
        {
            portDescriptors.Add(portDescriptor.ID, portDescriptor);

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
        /*
                public void RemovePort(int id)
                {
                    int i = portDescriptors.FindIndex(x => x.ID == id);
                    if (i < 0)
                    { 
                        return;
                    }
                    portDescriptors.RemoveAt(i);
                    //TODO:
                }*/

        public abstract void AddElements();

        public virtual bool EnablePreview => true;
        public virtual bool LowProfile => false;
        public bool preview3D = false;

        private void AddDefaultElements()
        {

            AddStyles();
            AddTitleElement();
            AddElements();
            if (EnablePreview)
            {
                AddPreview();
            }


            RefreshExpandedState();
            RefreshPorts();
        }
        public virtual Color Accent => Color.gray;
        private void AddStyles()
        {
            //extensionContainer.AddToClassList("sg-node__extension-container");
            //titleContainer.AddToClassList("sg-node__title-container");
            //inputContainer.AddToClassList("sg-node__input-container");
            //outputContainer.AddToClassList("sg-node__output-container");

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
        private void AddTitleElement()
        {
            if (LowProfile)
            {
                titleContainer.parent.Remove(titleContainer);
                return;
            }
            var nodeInfo = Info;

            var titleLabel = (Label)titleContainer.Q("title-label");
            titleLabel.text = nodeInfo.name;
            titleLabel.tooltip = nodeInfo.tooltip + "\n" + viewDataKey;
            titleLabel.style.fontSize = 12;
            titleLabel.style.marginRight = StyleKeyword.Auto;
            titleLabel.style.marginLeft = StyleKeyword.Auto;
            titleLabel.style.paddingLeft = 6;
            titleLabel.style.paddingRight = 6;
            titleContainer.Insert(0, titleLabel);

            var titleButton = titleContainer.Q("title-button-container");
            titleButton.parent.Remove(titleButton);

            var titleStyle = titleContainer.style;
            titleStyle.height = 24;
            titleStyle.backgroundColor = Color.black;
        }

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

            return "float4(0,0,0,0)";
        }

        internal void BuilderVisit(NodeVisitor visitor, int[] portsMask = null)
        {
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

                        // inherit precision
                        resultFloat.fullPrecision = incomingFloat.fullPrecision;
                        newData.Type = resultFloat;
                    }

                    PortData[id] = newData;
                }
                else
                {
                    var name = SetDefaultBinding(descriptor, visitor);
                    PortData[id] = new GeneratedPortData(descriptor.Type, name);
                }
            }

            Generate(visitor);
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

        public GeneratedPortData Cast(int portID, int targetComponent, bool updatePort = true)
        {
            var data = PortData[portID];
            var name = data.Name;
            var type = (Float)PortData[portID].Type;
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";

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
                var port = PortElements.Where(x => x.GetPortID() == data.Key).First();
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
            DefaultAdditionalElements(GraphView.additionalNodeElements);
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
            var title = new Label(" " + nodeInfo.name);
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

            const int w = 2;
            style.borderLeftWidth = w;
            style.borderRightWidth = w;
            style.borderTopWidth = w;
            style.borderBottomWidth = w;

            const int r = 5;
            style.borderBottomLeftRadius = r;
            style.borderTopLeftRadius = r;
            style.borderTopRightRadius = r;
            style.borderBottomRightRadius = r;

            style.marginTop = 5;
            style.marginRight = 5;
            style.bottom = 5;

            AdditionalElements(ve);
            root.Add(ve);
        }

        public virtual void AdditionalElements(VisualElement root)
        {
        }

        public PreviewDrawer previewDrawer;
        private void AddPreview()
        {
            previewDrawer = new PreviewDrawer
            {
                preview3D = preview3D
            };
            extensionContainer.Add(previewDrawer);
        }

        public Action<Material> onUpdatePreviewMaterial = (mat) => { };

        public void UpdatePreviewMaterial()
        {
            /*foreach (var material in PreviewDrawer.materials)
            {
                if (material is null) continue;
                onUpdatePreviewMaterial(material);
            }*/
            onUpdatePreviewMaterial(PreviewDrawer.PreviewMaterial);
            //PreviewDrawer.SetProperties -= (mat) => onUpdatePreviewMaterial(mat);
            //PreviewDrawer.SetProperties += (mat) => onUpdatePreviewMaterial(mat);
        }
        public string GetVariableNameForPreview(int ID)
        {
            return "_" + viewDataKey.Replace("-", "_") + "_" + ID;
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
