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

        internal static readonly PropertyDescriptor _surfaceOptionsStart = new(PropertyType.Float, "Surface Options", "_SurfaceOptions") { customAttributes = "[Foldout]" };
        internal PropertyDescriptor _mode => new(PropertyType.Float, "Rendering Mode", "_Mode") { FloatValue = (int)defaultMode, customAttributes = "[Enum(Opaque, 0, Cutout, 1, Fade, 2, Transparent, 3, Additive, 4, Multiply, 5, TransClipping, 6)]" };
        internal static readonly PropertyDescriptor _srcBlend = new(PropertyType.Float, "Source Blend", "_SrcBlend") { FloatValue = 1, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        internal static readonly PropertyDescriptor _dstBlend = new(PropertyType.Float, "Destination Blend", "_DstBlend") { FloatValue = 0, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        internal static readonly PropertyDescriptor _zwrite = new(PropertyType.Float, "ZWrite", "_ZWrite") { FloatValue = 1, customAttributes = "[Enum(Off, 0, On, 1)]" };
        internal PropertyDescriptor _cull => new(PropertyType.Float, "Cull", "_Cull") { FloatValue = (int)defaultCull, customAttributes = "[Enum(UnityEngine.Rendering.CullMode)]" };
        internal static readonly PropertyDescriptor _properties = new(PropertyType.Float, "Properties", "_Properties") { customAttributes = "[Foldout]" };

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
            DefaultInspector.SetupRenderingMode(material);
            ctx.AddObjectToAsset("Material", material);
        }
    }
}