using NUnit.Framework.Internal;
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

        }

        public void GeneratePreview(DropdownMenuAction action)
        {
            ShaderBuilder.GeneratePreview(GraphView, this, action != null);
        }
        public void RemovePreview(DropdownMenuAction action)
        {
            var d = extensionContainer.Q("PreviewDrawer");
            previewDrawer.Dispose();
            extensionContainer.Remove(d);
        }

        public IEnumerable<Port> PortElements => inputContainer.Children().Concat(outputContainer.Children()).Where(x => x is Port).Cast<Port>();
        public IEnumerable<Port> Inputs => inputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Input);
        public IEnumerable<Port> Outputs => outputContainer.Children().Where(x => x is Port).Cast<Port>().Where(x => x.direction == Direction.Output);

        protected abstract void Generate(NodeVisitor visitor);

        public Dictionary<int, PortDescriptor> portDescriptors = new();
        public void AddPort(PortDescriptor portDescriptor)
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

        private void AddDefaultElements()
        {

            AddStyles();
            AddTitleElement();
            AddElements();
            if (EnablePreview)
            {
                //AddPreview();
            }


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

            var titleLabel = new Label { text = "<b>" + nodeInfo.name + "</b>", tooltip = nodeInfo.tooltip + "\n" + viewDataKey };
            titleLabel.style.fontSize = 12;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            titleLabel.style.alignSelf = centerAlign;
            titleLabel.style.alignItems = centerAlign;
            titleLabel.enableRichText = true;
            titleContainer.Insert(0, titleLabel);

            titleContainer.style.height = 32;
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
            string value = GetDefaultBindingInput(descriptor, visitor);
            return new GeneratedPortData(descriptor.Type, value);
        }

        Dictionary<int, PortBinding> _portBindings = new();

        string GetDefaultBindingInput(PortDescriptor portDescriptor, NodeVisitor visitor)
        {
            int id = portDescriptor.ID;
            if (_portBindings.ContainsKey(id))
            {
                return _portBindings[id].ToString(); // some binding string here and bind
            }
            else
            {
                return "float(0)";
            }
        }

        internal void DefaultVisit(NodeVisitor visitor)
        {
            foreach (var descriptor in portDescriptors.Values)
            {
                if (descriptor.Direction == PortDirection.Input)
                {
                    int id = descriptor.ID;
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

 /*       public override void OnUnselected()
        {
            GeneratePreview(null);
        }*/

        public PreviewDrawer previewDrawer;
        private void AddPreview()
        {
            //previewDrawer = new PreviewDrawer();
            //var previewElement = previewDrawer.GetVisualElement();
            //extensionContainer.Add(previewDrawer);
            // ShaderBuilder.GeneratePreview(GraphView, this);

/*            return;
            var foldout = new Toggle("V");
            foldout.style.opacity = 0.5f;
            var checkmark = foldout.Q("unity-checkmark");
            checkmark.style.flexGrow = 0;
            checkmark.style.width = 0;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            foldout.style.alignSelf = centerAlign;
            foldout.style.alignItems = centerAlign;
            foldout.RegisterValueChangedCallback((evt) => {
                if (evt.newValue == true)
                {
                    previewDrawer = new PreviewDrawer();
                    ShaderBuilder.GeneratePreview(GraphView, this);
                    var previewElement = previewDrawer.GetVisualElement();
                    extensionContainer.Add(previewElement);
                    foldout.label = "Ʌ";
                }
                else
                {
                    var p = extensionContainer.Q("PreviewDrawer");
                    extensionContainer.Remove(p);
                    foldout.label = "V";
                }
            });
            extensionContainer.Add(foldout);
*/
            //foldout.SendEvent(new ChangeEvent<bool>());

            //previewElement.parent.style.marginLeft = 0;
        }

        public Action<Material> onUpdatePreviewMaterial = (mat) => { };

        public void UpdatePreviewMaterial()
        {
            foreach (var material in PreviewDrawer.materials)
            {
                if (material is null) continue;
                onUpdatePreviewMaterial(material);
            }
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


    [NodeInfo("*", "a * b"), Serializable]
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

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(ImplicitTruncation(A, B), "Multiply" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = {PortData[A].Name} * {PortData[B].Name};");
        }
    }

    [NodeInfo("+", "a + b"), Serializable]
    public class AddNode : ShaderNode
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

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(ImplicitTruncation(A, B), "Add" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = {PortData[A].Name} + {PortData[B].Name};");
        }
    }

    [NodeInfo("float3"), Serializable]
    public class Float3Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector3 _value;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(3, true), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) => {
                mat.SetVector(propertyName, _value);
            };

            var f = new Vector3Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float3, "", propertyName, _value.ToString());
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(3), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(3), "float3" + _value.ToString());
            }
        }
    }

    [NodeInfo("float2"), Serializable]
    public class Float2Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector2 _value;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(2, true), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) => {
                mat.SetVector(propertyName, _value);
            };

            var f = new Vector2Field { value = _value };
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float2, "", propertyName, _value.ToString());
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(2), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(2), "float2" + _value.ToString());
            }
        }
    }

    [NodeInfo("float"), Serializable]
    public class FloatNode : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private float _value;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(3, true), OUT));
            string propertyName = GetVariableNameForPreview(OUT);

            onUpdatePreviewMaterial += (mat) => {
                mat.SetFloat(propertyName, _value);
            };

            var f = new FloatField { value = _value, label = "X" };
            f.Children().First().style.minWidth = 0;
            f.RegisterValueChangedCallback((evt) => {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                string propertyName = GetVariableNameForPreview(OUT);
                var prop = new PropertyDescriptor(PropertyType.Float, "", propertyName, _value.ToString());
                visitor.AddProperty(prop);
                PortData[OUT] = new GeneratedPortData(new Float(1), propertyName);
            }
            else
            {
                PortData[OUT] = new GeneratedPortData(new Float(1), "float(" + _value.ToString() + ")");
            }
        }
    }

    [NodeInfo("uv0")]
    public class UV0Node : ShaderNode
    {
        const int OUT = 0;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(2, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(new Float(2), "varyings.uv0");
        }
    }

    [@NodeInfo("swizzle"), Serializable]
    public sealed class SwizzleNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;
        [SerializeField] string swizzle = "x";

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));

            var f = new TextField { value = swizzle };
            f.RegisterValueChangedCallback((evt) =>
            {
                string newValue = Swizzle.Validate(evt, f);
                if (!swizzle.Equals(newValue))
                {
                    swizzle = newValue;
                    GeneratePreviewForAffectedNodes();
                }
            });
            extensionContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            int components = swizzle.Length;
            var data = new GeneratedPortData(new Float(components, false), PortData[IN].Name + "." + swizzle);
            PortData[OUT] = data;
        }
    }

    [NodeInfo("Time")]
    public sealed class TimeNode : ShaderNode
    {
        const int OUT = 0;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(4, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            var data = new GeneratedPortData(new Float(4, false), "_Time");
            PortData[OUT] = data;
        }
    }

    [NodeInfo("sin", "sin(IN)"), Serializable]
    public class SinNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN, "A"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            PortData[OUT] = new GeneratedPortData(new Float(1, true), "Sin" + UniqueVariableID++);

            visitor.AppendLine($"{PortData[OUT].Type} {PortData[OUT].Name} = sin({PortData[IN].Name});");
        }
    }
}
