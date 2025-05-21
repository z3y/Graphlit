using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering;

namespace Graphlit
{
    public abstract class TemplateOutput : ShaderNode
    {
        public enum RenderPipeline
        {
            BuiltIn,
            URP,
        }

        public static RenderPipeline GetRenderPipeline() => GraphicsSettings.defaultRenderPipeline == null ? RenderPipeline.BuiltIn : RenderPipeline.URP;

        public abstract string Name { get; }
        public virtual void OnBeforeBuild(ShaderBuilder builder) { }
        public virtual void OnAfterBuild(ShaderBuilder builder) { }
        public abstract int[] VertexPorts { get; }
        public abstract int[] FragmentPorts { get; }

        public abstract string TemplateGUID { get; }

        public override Color Accent => Color.magenta;
        public virtual bool TallOutputs => true;
        public override void AdditionalElements(VisualElement root)
        {
            var graphData = GraphView.graphData;
            var shaderName = new TextField("Shader Name") { value = graphData.shaderName };
            shaderName.RegisterValueChangedCallback((evt) =>
            {
                graphData.shaderName = evt.newValue;
                GraphView.SetDirty();
            });
            root.Add(shaderName);

            var customEditor = new TextField("Custom Editor") { value = graphData.customEditor };
            customEditor.RegisterValueChangedCallback(x => graphData.customEditor = x.newValue);
            root.Add(customEditor);

            var fallback = new TextField("Fallback") { value = graphData.fallback };
            fallback.RegisterValueChangedCallback(x => graphData.fallback = x.newValue);
            root.Add(fallback);

            var graphPrecisionSelection = new EnumField("Graph Precision", graphData.precision);
            graphPrecisionSelection.RegisterValueChangedCallback(x => graphData.precision = (GraphData.GraphPrecision)x.newValue);
            root.Add(graphPrecisionSelection);

            var defaultPreviewState = new EnumField("Default Preview", graphData.defaultPreviewState);
            defaultPreviewState.RegisterValueChangedCallback(x => graphData.defaultPreviewState = (GraphData.DefaultPreviewState)x.newValue);
            root.Add(defaultPreviewState);

            var include = new TextField("Include") { value = graphData.include, multiline = true };
            include.RegisterValueChangedCallback(x => graphData.include = x.newValue);
            root.Add(include);

            var outline = new EnumField("Outline Pass", graphData.outlinePass);
            outline.RegisterValueChangedCallback(x => graphData.outlinePass = (GraphData.OutlinePassMode)x.newValue);
            root.Add(outline);
            
            // https://github.com/pema99/shader-knowledge/blob/main/tips-and-tricks.md#avoiding-draw-order-issues-with-transparent-shaders
            var depthFill = new Toggle("Depth Fill Pass") { value = graphData.depthFillPass, tooltip = "Avoid draw order issues with transparent shaders" };
            depthFill.RegisterValueChangedCallback(x => graphData.depthFillPass = x.newValue);
            root.Add(depthFill);

            var stencil = new Toggle("Stencil") { value = graphData.stencil };
            stencil.RegisterValueChangedCallback(x => graphData.stencil = x.newValue);
            root.Add(stencil);

            var mode = new EnumField("Mode", defaultMode);
            mode.RegisterValueChangedCallback(x => defaultMode = (MaterialRenderMode)x.newValue);
            root.Add(mode);

            var cull = new EnumField("Cull", defaultCull);
            cull.RegisterValueChangedCallback(x => defaultCull = (UnityEngine.Rendering.CullMode)x.newValue);
            root.Add(cull);

            AddVRCTagsElements(root, graphData);

            root.Add(PropertyDescriptor.CreateReordableListElement(graphData.properties, GraphView));
        }


        void AddVRCTagsElements(VisualElement root, GraphData graphData)
        {
            var foldout = new Foldout
            {
                text = "VRChat Fallback",
                value = false
            };
            root.Add(foldout);

            var mode = new EnumField("Mode", graphData.vrcFallbackTags.mode);
            mode.RegisterValueChangedCallback(x => graphData.vrcFallbackTags.mode = (VRCFallbackTags.ShaderMode)x.newValue);
            foldout.Add(mode);

            var type = new EnumField("Type", graphData.vrcFallbackTags.type);
            type.RegisterValueChangedCallback(x => graphData.vrcFallbackTags.type = (VRCFallbackTags.ShaderType)x.newValue);
            foldout.Add(type);

            var doubleSided = new Toggle("Double-Sided") { value = graphData.vrcFallbackTags.doubleSided };
            doubleSided.RegisterValueChangedCallback(x => graphData.vrcFallbackTags.doubleSided = x.newValue);
            foldout.Add(doubleSided);
        }

        public void VisitTemplate(NodeVisitor visitor, int[] ports)
        {
            var structField = visitor.Stage == ShaderStage.Fragment ?
                 visitor._shaderBuilder.passBuilders[visitor.Pass].surfaceDescriptionStruct
                 : visitor._shaderBuilder.passBuilders[visitor.Pass].vertexDescriptionStruct;

            foreach (var input in Inputs)
            {
                int currentID = input.GetPortID();

                if (!ports.Contains(currentID))
                {
                    continue;
                }

                var portDesc = portDescriptors[currentID];

                if (portDesc.Type is Float @float)
                {
                    var inputData = PortData[currentID];
                    visitor.AppendLine($"output.{portDesc.Name} = {inputData.Name};");

                    structField.Add($"{@float} {portDesc.Name};");
                }
            }
        }
        public override bool DisablePreview => true;

        protected sealed override void Generate(NodeVisitor visitor) { }

        internal static readonly PropertyDescriptor _surfaceOptions = new(PropertyType.Float, "Surface Type", "_Surface") { customAttributes = "[Enum(Opaque, 0, Transparent, 1)]" };
        internal static readonly PropertyDescriptor _surfaceBlend = new(PropertyType.Float, "Blending Mode", "_Blend") { customAttributes = "[ShowIf(_Surface, 1)] [Enum(Alpha, 0, Premultiply, 1, Additive, 2, Multiply, 3)]" };
        internal static readonly PropertyDescriptor _transClipping = new(PropertyType.Float, "Transparent Shadows", "_TransClipping") { customAttributes = "[ShowIf(_Surface, 1)] [ToggleUI]" };
        internal static readonly PropertyDescriptor _blendModePreserveSpecular = new(PropertyType.Float, "Preserve Specular", "_BlendModePreserveSpecular") { FloatValue = 1.0f, customAttributes = "[ShowIf(_Surface, 1)] [ShowIf(_Surface, 2)] [ToggleUI]" };
        internal static readonly PropertyDescriptor _alphaClip = new(PropertyType.Float, "Alpha Clipping", "_AlphaClip") { customAttributes = "[ToggleUI]" };
        internal static readonly PropertyDescriptor _alphaToMask = new(PropertyType.Float, "_AlphaToMask", "_AlphaToMask") { customAttributes = "[HideInInspector] [ToggleUI]" };

        internal PropertyDescriptor _mode => new(PropertyType.Float, "Rendering Mode", "_Mode") { customAttributes = "[HideInInspector]" };
        internal static readonly PropertyDescriptor _srcBlend = new(PropertyType.Float, "Source Blend", "_SrcBlend") { FloatValue = 1, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        internal static readonly PropertyDescriptor _dstBlend = new(PropertyType.Float, "Destination Blend", "_DstBlend") { FloatValue = 0, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        internal static readonly PropertyDescriptor _zwrite = new(PropertyType.Float, "ZWrite", "_ZWrite") { FloatValue = 1, customAttributes = "[Enum(Off, 0, On, 1)]" };
        internal static readonly PropertyDescriptor _ztest = new(PropertyType.Float, "ZTest", "_ZTest") { FloatValue = 4, customAttributes = "[Enum(UnityEngine.Rendering.CompareFunction)]" };
        
        internal PropertyDescriptor _cull => new(PropertyType.Float, "Cull", "_Cull") { FloatValue = (int)defaultCull, customAttributes = "[Enum(UnityEngine.Rendering.CullMode)]" };

        [SerializeField] public MaterialRenderMode defaultMode = MaterialRenderMode.Opaque;
        [SerializeField] public UnityEngine.Rendering.CullMode defaultCull = UnityEngine.Rendering.CullMode.Back;
        public enum MaterialRenderMode
        {
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3,
            Additive = 4,
            Multiply = 5,
            TransClipping = 6,
        }

        protected Texture2D _dfg = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.z3y.graphlit/Editor/Targets/Lit/dfg-multiscatter.exr");
        protected static readonly PropertyDescriptor _dfgProperty = new(PropertyType.Texture2D, "", "_DFG")
        { defaultAttributes = MaterialPropertyAttribute.HideInInspector | MaterialPropertyAttribute.NonModifiableTextureData };

        public virtual void OnImportAsset(AssetImportContext ctx, ShaderBuilder builder)
        {
            var result = builder.ToString();
            GraphlitImporter._lastImport = result;
            var shader = ShaderUtil.CreateShaderAsset(ctx, result, false);

            if (builder._nonModifiableTextures.Count > 0)
            {
                EditorMaterialUtility.SetShaderNonModifiableDefaults(shader, builder._nonModifiableTextures.Keys.ToArray(), builder._nonModifiableTextures.Values.ToArray());
            }
            if (builder._defaultTextures.Count > 0)
            {
                EditorMaterialUtility.SetShaderDefaults(shader, builder._defaultTextures.Keys.ToArray(), builder._defaultTextures.Values.ToArray());
            }

            ctx.AddObjectToAsset("Main Asset", shader, GraphlitImporter.Thumbnail);

            var path = AssetDatabase.GUIDToAssetPath(TemplateGUID);

            ctx.DependsOnSourceAsset(path);


            string prefix = GraphView.graphData.unlocked ? "Unlocked " : "";
            var material = new Material(shader)
            {
                name = $"{prefix}{builder.shaderName.Replace("/", "_")}"
            };
            material.SetFloat("_Mode", (int)defaultMode);
            ShaderInspector.UpgradeMode(material, false, false);
            ctx.AddObjectToAsset("Material", material);
        }

        protected static void AddURPLightingPragmas(PassBuilder pass)
        {
            pass.pragmas.Add("#define UNIVERSAL_FORWARD");

            // Universal Pipeline keywords
            pass.pragmas.Add("#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN");
            // pass.pragmas.Add("#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS");
            pass.pragmas.Add("#pragma multi_compile _ _ADDITIONAL_LIGHTS");
            // pass.pragmas.Add("#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX");
            pass.pragmas.Add("#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS");
            // pass.pragmas.Add("#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING");
            // pass.pragmas.Add("#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION");
            pass.pragmas.Add("#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH");
            // pass.pragmas.Add("#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION");
            // pass.pragmas.Add("#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3");
            pass.pragmas.Add("#pragma multi_compile_fragment _ _LIGHT_COOKIES");
            pass.pragmas.Add("#pragma multi_compile _ _LIGHT_LAYERS");
            // pass.pragmas.Add("#pragma multi_compile _ _FORWARD_PLUS");
#if UNITY_6000_0_OR_NEWER
            pass.pragmas.Add("#include_with_pragmas \"Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl\"");
#endif
            pass.pragmas.Add("#include_with_pragmas \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl\"");

            // Unity defined keywords
            pass.pragmas.Add("#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING");
            pass.pragmas.Add("#pragma multi_compile _ SHADOWS_SHADOWMASK");
            pass.pragmas.Add("#pragma multi_compile _ DIRLIGHTMAP_COMBINED");
            pass.pragmas.Add("#pragma multi_compile _ LIGHTMAP_ON");
            pass.pragmas.Add("#pragma multi_compile _ DYNAMICLIGHTMAP_ON");
            // pass.pragmas.Add("#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE");
            // pass.pragmas.Add("#pragma multi_compile_fragment _ DEBUG_DISPLAY");


        }

        public void CreateUniversalDepthPass(PassBuilder pass)
        {
            pass.tags["LightMode"] = "DepthOnly";
            pass.renderStates["ZWrite"] = "On";
            pass.renderStates["ColorMask"] = "R";
            pass.renderStates["Cull"] = "[_Cull]";

            pass.pragmas.Add("#pragma multi_compile_instancing");
            pass.pragmas.Add("#define DEPTH_ONLY");

            pass.attributes.RequirePositionOS();
            pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

            pass.varyings.RequirePositionCS();
            pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
            pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
        }

        public void CreateUniversalDepthNormalsPass(PassBuilder pass)
        {
            pass.tags["LightMode"] = "DepthNormals";
            pass.renderStates["ZWrite"] = "On";
            pass.renderStates["Cull"] = "[_Cull]";

            pass.pragmas.Add("#pragma multi_compile_instancing");
            pass.pragmas.Add("#define DEPTH_NORMALS");

            pass.attributes.RequirePositionOS();
            pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

            pass.varyings.RequirePositionCS();
            pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
            pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");

            PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
            PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.BitangentWS);
            PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);
            PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
        }

        public static void CreateShadowCaster(PassBuilder pass, bool urp)
        {
            pass.tags["LightMode"] = "ShadowCaster";
            pass.renderStates["ZWrite"] = "On";
            pass.renderStates["ZTest"] = "LEqual";
            pass.renderStates["ColorMask"] = "0";
            pass.renderStates["Cull"] = "[_Cull]";

            pass.pragmas.Add("#pragma multi_compile_instancing");

            pass.attributes.RequirePositionOS();
            pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

            if (urp)
            {
                pass.pragmas.Add("#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW");
            }
            else
            {
                //pass.pragmas.Add("#pragma dynamic_branch _ SHADOWS_CUBE SHADOWS_DEPTH");
                pass.pragmas.Add("#pragma multi_compile_shadowcaster");
            }
            pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
            pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");


            pass.varyings.RequirePositionCS();
            pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
            pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
        }
    }
}