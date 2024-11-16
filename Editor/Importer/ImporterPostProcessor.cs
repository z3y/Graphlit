using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Graphlit
{
    public class ImporterPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var importedAsset in importedAssets)
            {
                var subgraphObject = AssetDatabase.LoadAssetAtPath<SubgraphObject>(importedAsset);
                if (!subgraphObject)
                {
                    continue;
                }
                
                var extension = Path.GetExtension(importedAsset);
                if (extension == ".subgraphlit")
                {
                    var activeGraphViews = GraphlitImporter._graphViews.Values;
                    foreach (var activeGraphView in activeGraphViews)
                    {
                        if (activeGraphView is null)
                        {
                            continue;
                        }

                        var subgraphNodes = activeGraphView.graphElements.OfType<SubgraphNode>();
                        foreach (var subgraphNode in subgraphNodes)
                        {
                            if (subgraphNode.subgraph != subgraphObject)
                            {
                                continue;
                            }

                            subgraphNode.ReinitializePorts();
                            Debug.Log("Updating node");
                        }
                    }
                }
                
            }
        }
    }
}