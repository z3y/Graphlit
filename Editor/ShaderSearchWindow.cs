using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Graphlit
{
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using Graphlit.Nodes;
    using Unity.Collections;

    public class ShaderNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private ShaderGraphView _graphView;
        private Texture2D _nodeIndentationIcon;
        public void Initialize(ShaderGraphView graphView)
        {
            _graphView = graphView;

            _nodeIndentationIcon = new Texture2D(1, 1);
            _nodeIndentationIcon.SetPixel(0, 0, Color.clear);
            _nodeIndentationIcon.Apply();
        }

        private static Type[] _existingNodeTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(type => typeof(ShaderNode).IsAssignableFrom(type))
                .Where(x =>
                {
                    var info = x.GetCustomAttribute<NodeInfo>();
                    return info is not null && !info.name.StartsWith("_");
                })
                .OrderBy(x => x.GetCustomAttribute<NodeInfo>().name)
                .ToArray();

        public class CreateVarContext
        {
            public string name;
        }

        public class CreateSubgraphInputContext
        {
            public int id;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
            };


            if (_graphView.graphData.subgraphInputs.Count > 0)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Subgraph Inputs"), 1));

                foreach (var var in _graphView.graphData.subgraphInputs)
                {
                    var name = var.name;
                    var ctx = new CreateSubgraphInputContext() { id = var.id };
                    tree.Add(new SearchTreeEntry(new GUIContent($"Input: {name}", _nodeIndentationIcon)) { level = 2, userData = ctx });
                }
            }

            tree.Add(new SearchTreeGroupEntry(new GUIContent("Material Properties"), 1));
            for (int i = 0; i < _graphView.graphData.properties.Count; i++)
            {
                PropertyDescriptor property = _graphView.graphData.properties[i];
                string text = $"Prop: {property.displayName}";
                if (property.type != PropertyType.KeywordToggle && !string.IsNullOrEmpty(property.referenceName))
                {
                    text += $" [{property.GetReferenceName(GenerationMode.Final)}]";
                }
                tree.Add(new SearchTreeEntry(new GUIContent(text, _nodeIndentationIcon)) { level = 2, userData = i });
            }


            tree.Add(new SearchTreeGroupEntry(new GUIContent("Functions"), 1));

            var functionIncludes = AssetDatabase.FindAssets("l:" + CustomFunctionNode.Tag[0])
                .Union(AssetDatabase.FindAssets("l:" + CustomFunctionNode.Tag[1]));

            foreach (var guid in functionIncludes)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shaderInclude = AssetDatabase.LoadAssetAtPath<ShaderInclude>(path);

                if (shaderInclude == null)
                {
                    continue;
                }

                tree.Add(new SearchTreeEntry(new GUIContent(shaderInclude.name, _nodeIndentationIcon)) { level = 2, userData = shaderInclude });
            }

            var existingVars = _graphView.graphElements.OfType<RegisterVariableNode>().ToArray();

            if (existingVars.Length > 0)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Variables"), 1));

                foreach (var var in existingVars)
                {
                    var name = var._name;
                    var ctx = new CreateVarContext() { name = name };
                    tree.Add(new SearchTreeEntry(new GUIContent($"Get: {name}", _nodeIndentationIcon)) { level = 2, userData = ctx });
                }
            }



            List<string> groups = new();

            // taken from shader graph, why isnt this already included ;w;
            foreach (var node in _existingNodeTypes)
            {
                if (!_graphView.IsSubgraph)
                {
                    if (node.Name == "SubgraphInputNode" ||
                        node.Name == "SubgraphOutputNode")
                    {
                        continue;
                    }
                }
                // `createIndex` represents from where we should add new group entries from the current entry's group path.
                var createIndex = int.MaxValue;
                var nodeInfo = node.GetCustomAttribute<NodeInfo>();

                string[] split = nodeInfo.name.Split('/');

                // Compare the group path of the current entry to the current group path.
                for (var i = 0; i < split.Length - 1; i++)
                {
                    var group = split[i];
                    if (i >= groups.Count)
                    {
                        // The current group path matches a prefix of the current entry's group path, so we add the
                        // rest of the group path from the currrent entry.
                        createIndex = i;
                        break;
                    }
                    if (groups[i] != group)
                    {
                        // A prefix of the current group path matches a prefix of the current entry's group path,
                        // so we remove everyfrom from the point where it doesn't match anymore, and then add the rest
                        // of the group path from the current entry.
                        groups.RemoveRange(i, groups.Count - i);
                        createIndex = i;
                        break;
                    }
                }

                // Create new group entries as needed.
                // If we don't need to modify the group path, `createIndex` will be `int.MaxValue` and thus the loop won't run.
                for (var i = createIndex; i < split.Length - 1; i++)
                {
                    var group = split[i];
                    groups.Add(group);
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(group)) { level = i + 1 });
                }

                // Finally, add the actual entry.
                tree.Add(new SearchTreeEntry(new GUIContent(split.Last(), nodeInfo.icon != null ? nodeInfo.icon : _nodeIndentationIcon))
                {
                    level = split.Length,
                    userData = node
                });
            }


            //entries.Add(new SearchTreeEntry(new GUIContent("Multiply", _nodeIndentationIcon)) { level = 1, userData = typeof(MultiplyNode) });
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var userData = searchTreeEntry.userData;
            if (userData is int value)
            {
                var property = _graphView.graphData.properties[value];
                var node = (PropertyNode)Activator.CreateInstance(property.GetNodeType());
                node._ref = property.guid;
                _graphView.CreateNode(node, context.screenMousePosition);
                return true;
            }
            else if (userData is ShaderInclude shaderInclude)
            {
                var node = new CustomFunctionNode();
                node.UseFile(shaderInclude);
                _graphView.CreateNode(node, context.screenMousePosition);
                return true;
            }
            else if (userData is CreateVarContext var)
            {
                var node = new FetchVariableNode();
                node._name = var.name;
                _graphView.CreateNode(node, context.screenMousePosition);
                return true;
            }
            else if (userData is CreateSubgraphInputContext var1)
            {
                var node = (SubgraphInputNode)Activator.CreateInstance(typeof(SubgraphInputNode));
                node.SetReference(var1.id);
                _graphView.CreateNode(node, context.screenMousePosition);
                return true;
            }
            _graphView.CreateNode(searchTreeEntry.userData as Type, context.screenMousePosition);
            return true;
        }
    }
}
