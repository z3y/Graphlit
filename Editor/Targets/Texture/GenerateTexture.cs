using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Threading.Tasks;
using Palmmedia.ReportGenerator.Core;

namespace Graphlit
{
    [NodeInfo("Preprocess/Generate Texture"), Serializable]
    public class TextureOutput : ShaderNode, IHasPropertyDescriptor
    {
        public override Color Accent => new Color(0f, 0.851f, 0.743f);

        const int COLOR = 0;
        const int TEXTURE = 1;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(4, false), COLOR, "Color"));
            AddPort(new(PortDirection.Output, new Texture2DObject(), TEXTURE, "Texture"));
        }

        public override bool DisablePreview => true; 
        public override void AdditionalElements(VisualElement root)
        {
            var button = new Button() { text = "Generate"};
            button.clicked += () => { GeneratePreviewForAffectedNodes(); };
            root.Add(button);
        }


        public RenderTexture rtOutput = null;
        PropertyDescriptor propertyDescriptor = new PropertyDescriptor(PropertyType.Texture2D);
        protected override void Generate(NodeVisitor visitor)
        {
            Generate(visitor._shaderBuilder.generatedTextureResolution);
            visitor.AddProperty(propertyDescriptor);
            visitor._shaderBuilder.generatedTextures.Add(propertyDescriptor);
        }

        public void Generate(int resolution)
        {
            var builder = new ShaderBuilder(GenerationMode.Final, GraphView, BuildTarget.StandaloneWindows64, false);
            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            var pass = new PassBuilder("FORWARD", ShaderBuilder.VertexPreview, ShaderBuilder.FragmentPreview, COLOR);
            pass.tags["LightMode"] = "ForwardBase";

            pass.renderStates["Cull"] = "Back";
            pass.renderStates["ZWrite"] = "On";
            pass.renderStates["Blend"] = "One One";
            pass.pragmas.Add("#define PREVIEW");
            pass.pragmas.Add("#define TEXTURE_OUTPUT");

            pass.attributes.RequirePositionOS();
            pass.varyings.RequirePositionCS();
            PortBindings.GetBindingString(pass, ShaderStage.Fragment, 2, PortBinding.UV0);

            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
            builder.AddPass(pass);


            var fragmentVisitor = new NodeVisitor(builder, ShaderStage.Fragment, 0);
            builder.TraverseGraph(this, fragmentVisitor);
            DefaultVisit(fragmentVisitor);

            int dimensions = ((Float)GetInputPortData(COLOR, fragmentVisitor).Type).dimensions;
            ChangeDimensions(COLOR, dimensions);

            pass.surfaceDescriptionStruct.Add("float4 Color;");
            pass.surfaceDescription.Add($"output.Color = {PortData[COLOR].Name};");
            if (dimensions < 4)
            {
                pass.surfaceDescription.Add($"output.Color.a = 1;");
            }
            if (dimensions < 3)
            {
                pass.surfaceDescription.Add($"output.Color.b = 1;");
            }
            if (dimensions < 2)
            {
                pass.surfaceDescription.Add($"output.Color.g = 1;");
            }
            var shaderString = builder.ToString();
            Debug.Log(shaderString);
            var shader = ShaderUtil.CreateShaderAsset(shaderString);
            var material = new Material(shader);
            foreach (var tex in builder._nonModifiableTextures.Union(builder._defaultTextures))
            {
                material.SetTexture(tex.Key, tex.Value);
            }
            foreach (var tex in builder.generatedTextures)
            {
                material.SetTexture(tex.GetReferenceName(GenerationMode.Preview), tex.DefaultTextureValue);
            }

            var desc = new RenderTextureDescriptor
            {
                width = resolution,
                height = resolution,
                mipCount = 1,
                autoGenerateMips = false,
                useMipMap = false,
                msaaSamples = 1,
                colorFormat = GetFormatForDimensions(dimensions),
                sRGB = true,
                volumeDepth = 1,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            };

            if (rtOutput == null)
            {
                rtOutput = new RenderTexture(desc);
            }
            if (rtOutput.format != desc.colorFormat || rtOutput.width != resolution)
            {
                UnityEngine.Object.DestroyImmediate(rtOutput);
                rtOutput = new RenderTexture(desc);
            }
            Graphics.Blit(Texture2D.blackTexture, rtOutput);

            Graphics.Blit(Texture2D.blackTexture, rtOutput, material);

            Debug.Log(viewDataKey);

            //Debug.Log(rtOutput.format);

            UnityEngine.Object.DestroyImmediate(shader);
            UnityEngine.Object.DestroyImmediate(material);
            propertyDescriptor.tempTexture = rtOutput;

            PortData[TEXTURE] = new GeneratedPortData(portDescriptors[TEXTURE].Type, propertyDescriptor.GetReferenceName(GenerationMode.Preview));

            InitializeTexture();

        }

        async void InitializeTexture()
        {
            await Task.Delay(100);
            GraphView.PreviewMaterial.SetTexture(propertyDescriptor.GetReferenceName(GenerationMode.Preview), rtOutput);
            await Task.Delay(2000);
            GraphView.PreviewMaterial.SetTexture(propertyDescriptor.GetReferenceName(GenerationMode.Preview), rtOutput);

        }

        RenderTextureFormat GetFormatForDimensions(int dimension)
        {
            return dimension switch
            {
                1 => RenderTextureFormat.RFloat,
                2 => RenderTextureFormat.RGFloat,
                3 or _ => RenderTextureFormat.ARGBFloat,
            };
        }

        public PropertyDescriptor GetPropertyDescriptor()
        {
            return propertyDescriptor;
        }


        /* public override void OnImportAsset(AssetImportContext ctx, ShaderBuilder builder)
         {
             var shaderString = builder.ToString();
             ShaderGraphImporter._lastImport = shaderString;
             var shader = ShaderUtil.CreateShaderAsset(shaderString, false);
             var material = new Material(shader);




             Graphics.Blit(Texture2D.whiteTexture, blitRt, material);

             //desc.width = res;
             //desc.height = res;
             //var copyRt = new RenderTexture(desc);



             var texture = new Texture2D(blitRt.width, blitRt.height, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
             var activeRt = RenderTexture.active;
             RenderTexture.active = blitRt;
             texture.ReadPixels(new Rect(0, 0, blitRt.width, blitRt.height), 0, 0);
             texture.Apply();
             RenderTexture.active = null;

             UnityEngine.Object.DestroyImmediate(shader);
             UnityEngine.Object.DestroyImmediate(material);
             //UnityEngine.Object.DestroyImmediate(copyRt);
             UnityEngine.Object.DestroyImmediate(blitRt);


             ctx.AddObjectToAsset("texture", texture);
             //ctx.AddObjectToAsset("shader", shader);

             //var bytes = ImageConversion.EncodeToPNG(texture);
             //File.WriteAllBytes(ctx.assetPath + "tex.png", bytes);

         }*/
    }
}