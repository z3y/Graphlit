using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph.Nodes
{
    public class ShaderNode : Node
    {
        public string Name { get; set; }
        public string Text { get; set; }

        public void InitializeInternal(Vector2 position)
        {
            SetPosition(new Rect(position, Vector3.one));
            extensionContainer.AddToClassList("zs-node__extension-container");
            titleContainer.AddToClassList("zs-node__extension-container");
            inputContainer.AddToClassList("zs-node__extension-container");
            Initialize();
        }

        public void DrawInternal()
        {
            Draw();
            RefreshExpandedState();
        }

        public virtual void Initialize()
        {
            Text = "asdffg";
            Name = "base node xd";
        }

        public virtual void Draw()
        {
            var textField = new TextField { value = Name };
            titleContainer.Insert(0, textField);

            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "Input Value";
            inputContainer.Add(inputPort);

            var customDataContainer = new VisualElement();
            var textFoldout = new Foldout { text = "cool" };
            textFoldout.Add(new TextField { value = Text });
            customDataContainer.Add(textFoldout);

            customDataContainer.AddToClassList("zs-node__extension-container");


            extensionContainer.Add(customDataContainer);
        }
    }
}