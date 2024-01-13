using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZSG
{
    using System.Linq;
    using System.Reflection;
    using ZSG.Nodes;
    public class ShaderNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private ShaderGraphView _graphView;
        private Texture2D _nodeIndentationIcon;
        public void Initialize(ShaderGraphView graphView)
        {
            _graphView = graphView;

            _nodeIndentationIcon = new Texture2D(1, 1);
            _nodeIndentationIcon.SetPixel(0,0, Color.clear);
            _nodeIndentationIcon.Apply();
        }

        private static Type[] _existingNodeTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(type => typeof(ShaderNode).IsAssignableFrom(type)).ToArray();

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Element")));
            //entries.Add(new SearchTreeGroupEntry(new GUIContent("Node"), 1));

            foreach (var node in _existingNodeTypes)
            {
                var nodeInfo = node.GetCustomAttribute<NodeInfo>();
                if (nodeInfo is null)
                {
                    continue;
                }

                entries.Add(new SearchTreeEntry(
                    new GUIContent(nodeInfo.name == null ? "Default" : nodeInfo.name,
                    nodeInfo.icon == null ? _nodeIndentationIcon : nodeInfo.icon)
                    ) { level = 1, userData = node });
            }

            //entries.Add(new SearchTreeEntry(new GUIContent("Multiply", _nodeIndentationIcon)) { level = 1, userData = typeof(MultiplyNode) });
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            _graphView.CreateNode(searchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}
