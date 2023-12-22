using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Playables;
using z3y.ShaderGraph.Nodes;
using UnityEditor;

namespace z3y.ShaderGraph
{
    [ScriptedImporter(1, EXTENSION, 0)]
    public class Importer : ScriptedImporter
    {
        public const string EXTENSION = "zsg";
        [SerializeField] private List<ShaderNode> _shaderNodes;

        public override void OnImportAsset(AssetImportContext ctx)
        {
           /* _shaderNodes = new List<ShaderNode>();
            _shaderNodes.Add(new MultiplyNode());
            _shaderNodes.Add(new ShaderNode());
*/

            ctx.AddObjectToAsset("Main Asset", new TextAsset("asdf"));
        }

        [MenuItem("Assets/Create/z3y/Shader Graph")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.{EXTENSION}", string.Empty);
        }
    }
}