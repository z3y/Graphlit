using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph.Nodes
{
    public class ShaderNode : Node
    {
        public string ID { get; private set; }

        public ShaderNode()
        {
            ID = System.Guid.NewGuid().ToString();
            Debug.Log(ID);
        }

        public virtual string Title { get; } = "Default Node";
        public virtual string Tooltip { get; } = string.Empty;

        public void InitializeInternal(Vector2 position)
        {
            SetPosition(new Rect(position, Vector3.one));
            extensionContainer.AddToClassList("sg-node__extension-container");
            titleContainer.AddToClassList("sg-node__title-container");
            inputContainer.AddToClassList("sg-node__input-container");
            outputContainer.AddToClassList("sg-node__output-container");
            //Initialize();
        }

        internal void AddDefaultElements()
        {
            var titleLabel = new Label { text = Title, tooltip = Tooltip };
            titleLabel.style.fontSize = 13;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            titleLabel.style.alignSelf = centerAlign;
            titleContainer.Insert(0, titleLabel);

            AddElements();
            RefreshExpandedState();
        }

        public virtual void AddElements()
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "Input Value";
            inputContainer.Add(inputPort);

            var customDataContainer = new VisualElement();
            var textFoldout = new Foldout { text = "cool" };
            textFoldout.Add(new TextField { value = Title });
            customDataContainer.Add(textFoldout);

            customDataContainer.AddToClassList("zs-node__extension-container");


            extensionContainer.Add(customDataContainer);
        }
    }
}