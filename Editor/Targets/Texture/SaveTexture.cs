using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEditor;

namespace Graphlit
{
    [NodeInfo("Preprocess/Save Texture"), Serializable]
    public class SaveTexture : TemplateOutput
    {
        public override Color Accent => new Color(0f, 0.851f, 0.743f);

        const int TEXTURE = 0;

        public override bool TallOutputs => false;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Texture2DObject(), TEXTURE, "Texture"));
        }

        public override bool DisablePreview => true;
        public override string Name => "Save Texture";
        public override int[] VertexPorts => new int[] { };
        public override int[] FragmentPorts => new int[] { TEXTURE };

        public override void AdditionalElements(VisualElement root)
        {
            var graphData = GraphView.graphData;

            var defaultPreviewState = new EnumField("Default Preview", graphData.defaultPreviewState);
            defaultPreviewState.RegisterValueChangedCallback(x => graphData.defaultPreviewState = (GraphData.DefaultPreviewState)x.newValue);
            root.Add(defaultPreviewState);
        }


        public override void OnImportAsset(AssetImportContext ctx, ShaderBuilder builder)
        {
            var port = Inputs.First();

            if (!port.connected)
            {
                return;
            }

            var conn = port.connections.First();
            var node = (TextureOutput)conn.output.node;
            node.Generate();
            var rt = node.rtOutput;

            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true);
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            //texture.Apply();

            RenderTexture.active = null;
            EditorUtility.CompressTexture(texture, TextureFormat.BC5, TextureCompressionQuality.Best);

            ctx.AddObjectToAsset("texture", texture);
        }
    }
}