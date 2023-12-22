using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph.Nodes
{
    public class ShaderNodeVisualElement : Node
    {
        public ShaderNode shaderNode;

        public void Initialize(Type type, Vector2 position)
        {
            shaderNode = (ShaderNode)Activator.CreateInstance(type);
            shaderNode.Initialize(this, position);
            SetPosition(new Rect(position, Vector3.one));

            extensionContainer.AddToClassList("sg-node__extension-container");
            titleContainer.AddToClassList("sg-node__title-container");
            inputContainer.AddToClassList("sg-node__input-container");
            outputContainer.AddToClassList("sg-node__output-container");

            AddDefaultElements();
        }
        private void AddDefaultElements()
        {
            AddTitleElement();
            shaderNode.AddElements();
            RefreshExpandedState();
        }

        private void AddTitleElement()
        {
            var titleLabel = new Label { text = shaderNode.Title, tooltip = shaderNode.Tooltip };
            titleLabel.style.fontSize = 13;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            titleLabel.style.alignSelf = centerAlign;
            titleContainer.Insert(0, titleLabel);
        }
    }

    [System.Serializable]
    public class ShaderNode
    {
        public void Initialize(ShaderNodeVisualElement node, Vector2 position)
        {
            ID = System.Guid.NewGuid().ToString();
            Debug.Log("Created new node ID " + ID);
            Node = node;
            Position = position;
        }
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public Vector2 Position { get; set; }

        public virtual string Title { get; } = "Default Node";
        public virtual string Tooltip { get; } = string.Empty;
        public ShaderNodeVisualElement Node { get; private set; }

        public virtual void AddElements()
        {
            var inputPort = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "Input Value";
            Node.inputContainer.Add(inputPort);

            var customDataContainer = new VisualElement();
            var textFoldout = new Foldout { text = "cool" };
            textFoldout.Add(new TextField { value = "sometitle xd" });
            customDataContainer.Add(textFoldout);

            customDataContainer.AddToClassList("zs-node__extension-container");

            Node.extensionContainer.Add(customDataContainer);
        }
    }
}