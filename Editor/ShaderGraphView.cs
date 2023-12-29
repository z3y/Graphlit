using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    using NUnit.Framework.Internal.Execution;
    using z3y.ShaderGraph.Nodes;

    public class ShaderGraphView : GraphView
    {
        private ShaderNodeSearchWindow _searchWindow;
        private ShaderGraphWindow _editorWindow;

        public ShaderGraphView(ShaderGraphWindow editorWindow)
        {


            _editorWindow = editorWindow;
            // manipulators
            SetupZoom(0.15f, 2.0f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateGroupContextualMenu());

            // background
            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);

            // search window
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<ShaderNodeSearchWindow>();
                _searchWindow.Initialize(this);
            }
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);

            RegisterCallback<KeyDownEvent>(NodeHotkeyKeyDown);
            RegisterCallback<KeyUpEvent>(NodeHotkeyUpDown);

            RegisterCallback<ClickEvent>(NodeHotkey);
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
            var group = new Group
            {
                title = title
            };

            group.SetPosition(new Rect(localMousePosition, Vector3.one));
            return group;
        }

        public void CreateNode(Type type, Vector2 position, bool transform = true)
        {
            //TODO: check type
            if (transform) TransformMousePositionToLocalSpace(ref position, true);
            var node = new ShaderNodeVisualElement();
            node.Initialize(type, position);
            AddElement(node);
        }

        public void AddNode(ShaderNode shaderNode)
        {
            var node = new ShaderNodeVisualElement();
            node.AddAlreadyInitialized(shaderNode);
            AddElement(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var node = startPort.node;
            var direction = startPort.direction;
            var type = startPort.portType;

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (node == port.node)
                {
                    return;
                }

                if (direction == port.direction)
                {
                    return;
                }

                if (type != port.portType)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

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
            var key = e.keyCode;
            if (key != KeyCode.None) _lastKeyCode = e.keyCode;
        }

        private void NodeHotkeyUpDown(KeyUpEvent evt)
        {
            _lastKeyCode = KeyCode.None;
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
                case KeyCode.M: CreateNode(typeof(MultiplyNode), position, false); break;
                case KeyCode.A: CreateNode(typeof(AddNode), position, false); break;
                case KeyCode.Period: CreateNode(typeof(DotNode), position, false); break;
                case KeyCode.Z: CreateNode(typeof(SwizzleNode), position, false); break;
            }
        }

    }
}