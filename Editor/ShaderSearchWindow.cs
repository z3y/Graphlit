using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace z3y.ShaderGraph
{
    using z3y.ShaderGraph.Nodes;
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
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Element")));
            //entries.Add(new SearchTreeGroupEntry(new GUIContent("Node"), 1));
            entries.Add(new SearchTreeEntry(new GUIContent("Default", _nodeIndentationIcon)) {level = 1, userData = typeof(ShaderNode)});
            entries.Add(new SearchTreeEntry(new GUIContent("Multiply", _nodeIndentationIcon)) {level = 1, userData = typeof(MultiplyNode)});

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            _graphView.CreateNode(searchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}
