using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.AssetImporters;
using System.Linq;
using System.IO;

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

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            {
                var pass = new PassBuilder("FORWARD", ShaderBuilder.VertexPreview, ShaderBuilder.FragmentPreview, COLOR);
                pass.tags["LightMode"] = "ForwardBase";

                pass.renderStates["Cull"] = "Back";
                pass.renderStates["ZWrite"] = "On";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";
                pass.pragmas.Add("#define PREVIEW");
                pass.pragmas.Add("#define TEXTURE_OUTPUT");

                pass.attributes.RequirePositionOS();
                pass.varyings.RequirePositionCS();
                PortBindings.GetBindingString(pass, ShaderStage.Fragment, 2, PortBinding.UV0);

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }

        }

        public override void OnAfterBuild(ShaderBuilder builder)
        {
            /*var vertexDescription = builder.passBuilders[0].vertexDescriptionStruct;
            vertexDescription.Add("float3 Position;");
            vertexDescription.Add("float3 Normal;");
            vertexDescription.Add("float3 Tangent;");*/
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

            int res = 512;

            var desc = new RenderTextureDescriptor
            {
                width = res * 4,
                height = res * 4,
                mipCount = 1,
                autoGenerateMips = false,
                useMipMap = false,
                msaaSamples = 1,
                colorFormat = RenderTextureFormat.ARGBFloat,
                sRGB = true,
                volumeDepth = 1,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            };


            var blitRt = new RenderTexture(desc);


            Graphics.Blit(Texture2D.whiteTexture, blitRt, material);

            desc.width = res;
            desc.height = res;
            var copyRt = new RenderTexture(desc);

            Graphics.Blit(blitRt, copyRt);

            var texture = new Texture2D(copyRt.width, copyRt.height, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            var activeRt = RenderTexture.active;
            RenderTexture.active = copyRt;
            texture.ReadPixels(new Rect(0, 0, copyRt.width, copyRt.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;

            //UnityEngine.Object.DestroyImmediate(shader);
            UnityEngine.Object.DestroyImmediate(material);
            UnityEngine.Object.DestroyImmediate(copyRt);
            UnityEngine.Object.DestroyImmediate(blitRt);


            ctx.AddObjectToAsset("texture", texture);
            ctx.AddObjectToAsset("shader", shader);

            //var bytes = ImageConversion.EncodeToPNG(texture);
            //File.WriteAllBytes(ctx.assetPath + "tex.png", bytes);

        }
    }
}