/*using UnityEngine.UIElements;
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

        [SerializeField] TextureFormat _format = TextureFormat.DXT1;
        [SerializeField] TextureCompressionQuality _quality = TextureCompressionQuality.Best;
        [SerializeField] string _textureName = "Texture";
        [SerializeField] Resolution _resolution = Resolution._512;

        public enum Resolution
        {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        }

        public override void AdditionalElements(VisualElement root)
        {
            var graphData = GraphView.graphData;

            var defaultPreviewState = new EnumField("Default Preview", graphData.defaultPreviewState);
            defaultPreviewState.RegisterValueChangedCallback(x => graphData.defaultPreviewState = (GraphData.DefaultPreviewState)x.newValue);
            root.Add(defaultPreviewState);

            var format = new EnumField("Format", _format);
            format.RegisterValueChangedCallback(x => _format = (TextureFormat)x.newValue);
            root.Add(format);

            var quality = new EnumField("Quality", _quality);
            quality.RegisterValueChangedCallback(x => _quality = (TextureCompressionQuality)x.newValue);
            root.Add(quality);

            var res = new EnumField("Resolution", _resolution);
            res.RegisterValueChangedCallback(x => _resolution = (Resolution)x.newValue);
            root.Add(res);

            var nameField = new TextField("Name") { value = _textureName};
            nameField.RegisterValueChangedCallback(x => _textureName = x.newValue);
            root.Add(nameField);
        }


        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.generatedTextureResolution = (int)_resolution;
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
            node.Generate((int)_resolution);
            var rt = node.rtOutput;

            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true, true)
            {
                name = _textureName
            };
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            //texture.Apply();

            RenderTexture.active = null;
            EditorUtility.CompressTexture(texture, _format, _quality);

            ctx.AddObjectToAsset(_textureName, texture);
        }
    }
}*/