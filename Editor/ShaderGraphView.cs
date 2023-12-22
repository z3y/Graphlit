using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    using z3y.ShaderGraph.Nodes;

    public class ShaderGraphView : GraphView
    {
        private ShaderNodeSearchWindow _searchWindow;
        private ShaderGraphWindow _editorWindow;

        public ShaderGraphView(ShaderGraphWindow editorWindow)
        {
            _editorWindow = editorWindow;
            // manipulators
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateGroupContextualMenu());

            // nodes
            //this.AddManipulator(CreateNodeContextualMenu<ShaderNode>("Default"));
            //this.AddManipulator(CreateNodeContextualMenu<MultiplyNode>("Multiply"));

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

            //test

            CreateNode(typeof(Nodes.MultiplyNode), Vector2.zero);

        }

       /* private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : ShaderNode, new()
        {
            return new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode<T>(actionEvent.eventInfo.localMousePosition)))
            );
        }
*/
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

      /*  public Node CreateNode<T>(Vector2 position) where T : ShaderNode, new()
        {
            TransformMousePositionToLocalSpace(ref position, false);
            var sn = new T();
            sn.InitializeInternal(position);
            sn.AddDefaultElements();
            return sn;
        }*/
        public void CreateNode(Type type, Vector2 position)
        {
            TransformMousePositionToLocalSpace(ref position, true);
            var snv = new ShaderNodeVisualElement();
            snv.Initialize(type, position);
            //var sn = (ShaderNodeVisualElement)Activator.CreateInstance(type);
            //sn.InitializeInternal(position);
            //sn.AddDefaultElements();
            AddElement(snv);
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
    }
}