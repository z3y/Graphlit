using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.AssetImporters;
using System.Linq;

namespace Graphlit
{
    [NodeInfo("Targets/Texture Output"), Serializable]
    public class TextureOutput : TemplateOutput
    {
        /*[MenuItem("Assets/Create/Graphlit/Unlit Graph")]
        public static void CreateVariantFile() => ShaderGraphImporter.CreateEmptyTemplate(new UnlitTemplate(),
            x => x.graphData.vrcFallbackTags.type = VRCFallbackTags.ShaderType.Unlit);*/
        public override bool TallOutputs => false;
        public override string Name { get; } = "Unlit";
        public override int[] VertexPorts => new int[] {};
        public override int[] FragmentPorts => new int[] { COLOR };

        const int COLOR = 0;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(4, false), COLOR, "Color"));
        }

        public override void AdditionalElements(VisualElement root)
        {
            var graphData = GraphView.graphData;

            var defaultPreviewState = new EnumField("Default Preview", graphData.defaultPreviewState);
            defaultPreviewState.RegisterValueChangedCallback(x => graphData.defaultPreviewState = (GraphData.DefaultPreviewState)x.newValue);
            root.Add(defaultPreviewState);
        }

        const string Vertex = "Packages/com.z3y.graphlit/Editor/Targets/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.graphlit/Editor/Targets/Unlit/FragmentForward.hlsl";
        const string FragmentShadow = "Packages/com.z3y.graphlit/Editor/Targets/FragmentShadow.hlsl";

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.properties.Add(_surfaceOptionsStart);
            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_cull);

            builder.properties.Add(_properties);
            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            {
                var pass = new PassBuilder("FORWARD", Vertex, FragmentForward, COLOR);
                pass.tags["LightMode"] = "ForwardBase";

                pass.renderStates["Cull"] = "Back";
                pass.renderStates["ZWrite"] = "On";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";
                pass.pragmas.Add("#define RETURN_COLOR");

                pass.attributes.RequirePositionOS();
                pass.varyings.RequirePositionCS();

                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.NormalWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }

        }

        public override void OnAfterBuild(ShaderBuilder builder)
        {
            var vertexDescription = builder.passBuilders[0].vertexDescriptionStruct;
            vertexDescription.Add("float3 Position;");
            vertexDescription.Add("float3 Normal;");
            vertexDescription.Add("float3 Tangent;");
        }

        public override void OnImportAsset(AssetImportContext ctx, ShaderBuilder builder)
        {
            var shaderString = builder.ToString();
            ShaderGraphImporter._lastImport = shaderString;
            var shader = ShaderUtil.CreateShaderAsset(shaderString, false);
            var material = new Material(shader);

            foreach (var tex in builder._nonModifiableTextures.Union(builder._defaultTextures))
            {
                material.SetTexture(tex.Key, tex.Value);
            }

            var desc = new RenderTextureDescriptor
            {
                width = 1024,
                height = 1024,
                mipCount = 1,
                autoGenerateMips = false,
                useMipMap = false,
                msaaSamples = 1,
                colorFormat = RenderTextureFormat.ARGB32,
                sRGB = true,
                volumeDepth = 1,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            };

            var rt = new RenderTexture(desc);
            Graphics.Blit(Texture2D.whiteTexture, rt, material);

            var texture = new Texture2D(rt.width, rt.height);
            var activeRt = RenderTexture.active;
            RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;

            //UnityEngine.Object.DestroyImmediate(shader);
            UnityEngine.Object.DestroyImmediate(material);
            UnityEngine.Object.DestroyImmediate(rt);


            ctx.AddObjectToAsset("texture", texture);
            ctx.AddObjectToAsset("shader", shader);

        }
    }
}