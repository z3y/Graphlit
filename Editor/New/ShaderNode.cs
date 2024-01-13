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
using static UnityEditor.ObjectChangeEventStream;

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
        }

        private void GeneratePreview(DropdownMenuAction action)
        {
            var shaderBuilder = new ShaderBuilder(GenerationMode.Preview, GraphView);
            var target = new UnlitBuildTarget();
            target.BuilderPassthourgh(shaderBuilder);
            shaderBuilder.Build(this);

            UnityEngine.Debug.Log(shaderBuilder.ToString());
        }

        public IEnumerable<Port> PortElements => inputContainer.Children().Concat(outputContainer.Children()).Where(x => x is Port).Cast<Port>();
        public IEnumerable<Port> Inputs => inputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Input);
        public IEnumerable<Port> Outputs => outputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Input);

        public abstract void Generate(NodeVisitor visitor);

        public List<PortDescriptor> portDescriptors = new();
        public void AddPort(PortDescriptor portDescriptor)
        {
            portDescriptors.Add(portDescriptor);

            var container = portDescriptor.Direction == PortDirection.Input ? inputContainer : outputContainer;

            var type = portDescriptor.Type.GetType();
            var capacity = portDescriptor.Direction == PortDirection.Input ? Capacity.Single : Capacity.Multi;

            var port = InstantiatePort(Orientation.Horizontal, (Direction)portDescriptor.Direction, capacity, type);
            //portElement.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
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
        }

        public void RemovePort(int id)
        {
            int i = portDescriptors.FindIndex(x => x.ID == id);
            if (i < 0)
            { 
                return;
            }
            portDescriptors.RemoveAt(i);
            //TODO:
        }

        public abstract void AddElements();

        public virtual bool EnablePreview => true;

        private void AddDefaultElements()
        {

            AddStyles();
            AddTitleElement();
            AddElements();
            // AddPreview();


            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddStyles()
        {
            extensionContainer.AddToClassList("sg-node__extension-container");
            titleContainer.AddToClassList("sg-node__title-container");
            inputContainer.AddToClassList("sg-node__input-container");
            outputContainer.AddToClassList("sg-node__output-container");
        }
        private void AddTitleElement()
        {
            var nodeInfo = Info;

            var titleLabel = new Label { text = nodeInfo.name, tooltip = nodeInfo.tooltip + "\n" + viewDataKey };
            titleLabel.style.fontSize = 14;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            titleLabel.style.alignSelf = centerAlign;
            titleLabel.style.alignItems = centerAlign;
            titleContainer.Insert(0, titleLabel);

            /*var noRadius = new StyleLength { value = 0 };
            var borderStyle = this.ElementAt(0).style;
            var borderSelectionStyle = this.ElementAt(1).style;

            borderStyle.borderBottomLeftRadius = noRadius;
            borderStyle.borderBottomRightRadius = noRadius;
            borderStyle.borderTopLeftRadius = noRadius;
            borderStyle.borderTopRightRadius = noRadius;

            borderSelectionStyle.borderBottomLeftRadius = noRadius;
            borderSelectionStyle.borderBottomRightRadius = noRadius;
            borderSelectionStyle.borderTopLeftRadius = noRadius;
            borderSelectionStyle.borderTopRightRadius = noRadius;*/
        }

        public static int UniqueVariableID = 0;
        public Dictionary<int, GeneratedPortData> portData = new();
        public GeneratedPortData GetInputPortData(int portID)
        {
            var port = Inputs.Where(x => x.GetPortID() == portID).First();
            if (port.connected)
            {
                var incomingPort = port.connections.First().output;
                var incomingNode = (ShaderNode)incomingPort.node;

                return incomingNode.portData[incomingPort.GetPortID()];
            }
            else
            {
                return GetDefaultInput(portID);
            }
        }

        public virtual GeneratedPortData GetDefaultInput(int portID)
        {
            var descriptor = portDescriptors.Find(x => x.ID == portID);
            return new GeneratedPortData(descriptor.Type, "0");
        }


        public Float ImplicitTruncation(params int[] IDs)
        {
            int trunc = 4;
            int max = 1;
            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                var type = (Float)portData[ID].Type;
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

        public void Cast(int portID, int targetComponent)
        {
            var data = portData[portID];
            var name = data.Name;
            var type = (Float)portData[portID].Type;
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";

            if (components == targetComponent)
            {
                return;
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
                    return;
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
            portData[portID] = new GeneratedPortData(type, name);
        }

        public void UpdateGraphView()
        {
            foreach (var data in portData)
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
    }

    [NodeInfo("*", "a * b")]
    public class MultiplyNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        public override void Generate(NodeVisitor visitor)
        {
            //visitor.SetOutputType(OUT, visitor.ImplicitTruncation(A, B));
            //visitor.OutputExpression(OUT, A, "*", B, "Multiply");
            // inherit or if not connected use default
            portData[A] = GetInputPortData(A);
            portData[B] = GetInputPortData(B);
            var t = ImplicitTruncation(A, B);
            portData[OUT] = new GeneratedPortData(t, "Multiply" + UniqueVariableID++); // new name

            visitor.AppendLine($"{portData[OUT].Type} {portData[OUT].Name} = {portData[A].Name} * {portData[B].Name};");
        }
    }

    [NodeInfo("float3test")]
    public class Float3TestNode : ShaderNode
    {
        const int OUT = 0;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(3, true), OUT));
        }

        public override void Generate(NodeVisitor visitor)
        {
            //visitor.SetOutputType(OUT, visitor.ImplicitTruncation(A, B));
            //visitor.OutputExpression(OUT, A, "*", B, "Multiply");
            // inherit or if not connected use default
            portData[OUT] = new GeneratedPortData(new Float(3), "float3(0,1,2)"); // new name

            //visitor.AppendLine($"{portData[OUT].Type} {portData[OUT].Name} = float3(0,1,2);");
        }
    }
}
