using System;
using System.Reflection;
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
            AddDefaultElements();
            SetNodePosition(position);
        }

        public void AddAlreadyInitialized(ShaderNode shaderNode)
        {
            shaderNode.SetNodeVisualElement(this);
            this.shaderNode = shaderNode;
            AddDefaultElements();
            SetNodePosition(shaderNode.Position);
        }
        private void AddDefaultElements()
        {
            AddStyles();
            AddTitleElement();
            shaderNode.AddElements();
            RefreshExpandedState();
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
            var type = shaderNode.GetType();
            var displayName = type.GetCustomAttribute<DisplayName>();
            var tooltip = type.GetCustomAttribute<DisplayName>();

            var titleLabel = new Label {
                text = displayName == null ? "Default Title" : displayName.text,
                tooltip = tooltip == null ? "Default Title" : tooltip.text
            };
            titleLabel.style.fontSize = 13;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            titleLabel.style.alignSelf = centerAlign;
            titleContainer.Insert(0, titleLabel);
        }
        private void SetNodePosition(Vector2 position)
        {
            SetPosition(new Rect(position, Vector3.one));
        }
    }

    [System.Serializable]
    [@DisplayName("Default Title")]
    public class ShaderNode
    {
        public void Initialize(ShaderNodeVisualElement node, Vector2 position)
        {
            ID = System.Guid.NewGuid().ToString();
            Debug.Log("Created new node ID " + ID);
            Node = node;
            Position = position;
        }
        internal void SetNodeVisualElement(ShaderNodeVisualElement node)
        {
            this.Node = node;
        }
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        internal void UpdateSerializedPosition()
        {
            var rect = Node.GetPosition();
            Position = new Vector2(rect.x, rect.y);
        }
        public static DisplayName GetDisplayNameAttribute(Type type) => _displayNameAttribute ??= type.GetCustomAttribute<DisplayName>();
        public static Tooltip GetTooltipAttribute(Type type) => _tooltopAttribute ??= type.GetCustomAttribute<Tooltip>();
        public static IndentationIcon GetIndentationIconAttribute(Type type) => _indentationIconAttribute ??= type.GetCustomAttribute<IndentationIcon>();

        private static DisplayName _displayNameAttribute = null;
        private static Tooltip _tooltopAttribute = null;
        private static IndentationIcon _indentationIconAttribute = null;

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

            customDataContainer.AddToClassList("sg-node__extension-container");

            Node.extensionContainer.Add(customDataContainer);
        }

        public virtual void Visit()
        {

        }
    }
}