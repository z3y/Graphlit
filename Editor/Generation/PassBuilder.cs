using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;

namespace Graphlit
{
    public class PassBuilder
    {
        public PassBuilder(string name, string vertexShaderPath, string fragmentShaderPath, params int[] ports)
        {
            this.name = name;
            this.vertexShaderPath = vertexShaderPath;
            this.fragmentShaderPath = fragmentShaderPath;

            Ports = ports;
            attributes = new ShaderAttributes();
            varyings = new ShaderVaryings(attributes);
        }

        public string name;
        public Dictionary<string, string> tags = new();
        public Dictionary<string, string> renderStates = new();
        public List<string> pragmas = new();
        public List<string> functions = new();
        public List<string> preincludes = new();
        public List<string> vertexDescription = new();
        public List<string> vertexDescriptionStruct = new();
        public List<string> surfaceDescription = new();
        public List<string> surfaceDescriptionStruct = new();
        public List<PropertyDescriptor> properties = new();
        public bool outlinePass = false;

        public ShaderAttributes attributes;
        public ShaderVaryings varyings;

        public string target = "4.5";

        public string fragmentDataPath = "Packages/com.z3y.graphlit/Editor/Targets/FragmentData.hlsl";
        public string vertexDataPath = "Packages/com.z3y.graphlit/Editor/Targets/VertexData.hlsl";

        public GenerationMode generationMode;


        public string vertexShaderPath;
        public string fragmentShaderPath;

        public int[] Ports { get; }

        const string AudioLinkPath = "Packages/com.llealloo.audiolink/Runtime/Shaders/AudioLink.cginc";
        static bool AudioLinkExists = System.IO.File.Exists(AudioLinkPath);

        public void AppendPass(ShaderStringBuilder sb, GraphData graphData)
        {
            varyings.PackVaryings();

            sb.AppendLine("Name \"" + name + "\"");
            ShaderBuilder.AppendTags(sb, tags);

            sb.AppendLine();
            foreach (var state in renderStates)
            {
                sb.AppendLine(state.Key + " " + state.Value);
            }
            if (graphData.stencil)
            {
                AppendStencil(sb);
            }

            sb.AppendLine("HLSLPROGRAM");
            AppendPassHLSL(sb, graphData);
            sb.AppendLine("ENDHLSL");
        }
        static string StencilStates = @"
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
        
                CompBack[_StencilCompBack]
                PassBack[_StencilPassBack]
                FailBack[_StencilFailBack]
                ZFailBack[_StencilZFailBack]
                CompFront[_StencilCompFront]
                PassFront[_StencilPassFront]
                FailFront[_StencilFailFront]
                ZFailFront[_StencilZFailFront]
            }";
        static string StencilStatesOutline = @"
            Stencil
            {
                Ref [_OutlineStencilRef]
                ReadMask [_OutlineStencilReadMask]
                WriteMask [_OutlineStencilWriteMask]
        
                CompBack[_OutlineStencilCompBack]
                PassBack[_OutlineStencilPassBack]
                FailBack[_OutlineStencilFailBack]
                ZFailBack[_OutlineStencilZFailBack]
                CompFront[_OutlineStencilCompFront]
                PassFront[_OutlineStencilPassFront]
                FailFront[_OutlineStencilFailFront]
                ZFailFront[_OutlineStencilZFailFront]
            }";
        void AppendStencil(ShaderStringBuilder sb)
        {
            if (outlinePass)
            {
                sb.AppendLine(StencilStatesOutline);
            }
            else
            {
                sb.AppendLine(StencilStates);
            }
        }

        bool AllPropertiesHaveSameValue(List<Material> mats, string referenceName, PropertyType type)
        {
            if (mats.Count < 2)
            {
                return true;
            }

            if (type == PropertyType.Color)
            {
                var value = mats[0].GetColor(referenceName);
                for (int i = 1; i < mats.Count; i++)
                {
                    if (mats[i].GetColor(referenceName) != value)
                    {
                        return false;
                    }
                }
            }

            else if (type == PropertyType.Float || type == PropertyType.Toggle)
            {
                var value = mats[0].GetFloat(referenceName);
                for (int i = 1; i < mats.Count; i++)
                {
                    if (mats[i].GetFloat(referenceName) != value)
                    {
                        return false;
                    }
                }
            }

            else if (type == PropertyType.Integer)
            {
                var value = mats[0].GetInteger(referenceName);
                for (int i = 1; i < mats.Count; i++)
                {
                    if (mats[i].GetInteger(referenceName) != value)
                    {
                        return false;
                    }
                }
            }

            else if (type == PropertyType.Float2 || type == PropertyType.Float3 || type == PropertyType.Float4)
            {
                var value = mats[0].GetVector(referenceName);
                for (int i = 1; i < mats.Count; i++)
                {
                    if (mats[i].GetVector(referenceName) != value)
                    {
                        return false;
                    }
                }
            }

            else if (type == PropertyType.Texture2D ||
                type == PropertyType.Texture2DArray ||
                type == PropertyType.Texture3D ||
                type == PropertyType.TextureCube ||
                type == PropertyType.TextureCubeArray)
            {
                var value = mats[0].GetTexture(referenceName);
                for (int i = 1; i < mats.Count; i++)
                {
                    if (mats[i].GetTexture(referenceName) != value)
                    {
                        return false;
                    }
                }
            }


            return true;
        }

        struct MaterialId
        {
            public Material material;
            public int id;
        }

        public void AppendPassHLSL(ShaderStringBuilder sb, GraphData graphData)
        {
            sb.AppendLine();

            if (graphData.enableLockMaterials)
            {
                // attributes.RequireVertexID();
                varyings.RequireUV(0, 3);
                varyings.RequireCustomString("uint materialID : MATERIALID;");

                if (!properties.Any(x => x.referenceName == "_Cull"))
                {
                    properties.Add(new PropertyDescriptor(PropertyType.Float, "Cull", "_Cull"));
                }

                if (graphData.optimizerMixedCull)
                {
                    varyings.RequireCullFace();
                }
            }

            sb.AppendLine("#pragma target " + target);
            sb.AppendLine("#pragma vertex vert");
            sb.AppendLine("#pragma fragment frag");

            if (graphData.enableLockMaterials)
            {
                sb.AppendLine("#define GRAPHLIT_OPTIMIZER_ENABLED");

                var enabledKeywords = graphData.lockMaterials[0].enabledKeywords;

                foreach (var keyword in enabledKeywords)
                {
                    sb.AppendLine($"#define {keyword.name}");
                    sb.AppendLine($"#pragma skip_variants {keyword.name}");
                }
            }

            if (TemplateOutput.GetRenderPipeline() == TemplateOutput.RenderPipeline.URP)
            {
                sb.AppendLine("#define UNIVERSALRP");
            }
            foreach (var p in pragmas)
            {
                sb.AppendLine(p);
            }
            if (outlinePass) sb.AppendLine("#define OUTLINE_PASS");
            foreach (var property in properties)
            {
                if (property.type == PropertyType.KeywordToggle)
                {
                    if (property.keywordPassFlags == 0 || generationMode == GenerationMode.Preview)
                    {
                        sb.AppendLine(property.GetFieldDeclaration(generationMode));
                    }
                    else if (System.Enum.TryParse(typeof(KeywordPassFlags), name, out var flagObj))
                    {
                        var flag = (KeywordPassFlags)flagObj;
                        if (property.keywordPassFlags.HasFlag(flag))
                        {
                            sb.AppendLine(property.GetFieldDeclaration(generationMode));
                        }
                    }
                }
            }
            if (AudioLinkExists)
            {
                sb.AppendInclude(AudioLinkPath);
            }
            sb.AppendLine();

            attributes.AppendAttributeDefines(sb);
            sb.AppendLine("struct Attributes");
            sb.Indent();
            attributes.AppendAttributes(sb);
            sb.UnIndent("};");

            sb.AppendLine("struct Varyings");
            sb.Indent();
            varyings.AppendVaryingsStruct(sb);
            sb.UnIndent("};");

            if (varyings.requireCullFace)
            {
                sb.AppendLine("#define VARYINGS_NEED_FACE");
            }

            sb.AppendLine("struct VertexDescription");
            sb.Indent();
            foreach (var s in vertexDescriptionStruct)
            {
                sb.AppendLine(s);
            }
            sb.UnIndent("};");

            sb.AppendLine("struct SurfaceDescription");
            sb.Indent();
            foreach (var s in surfaceDescriptionStruct)
            {
                sb.AppendLine(s);
            }
            sb.UnIndent("};");

            sb.AppendLine();
            foreach (var property in properties)
            {
                if (!property.IsTextureType) continue;
                sb.AppendLine(property.GetFieldDeclaration(generationMode));
            }
            sb.AppendLine();

            if (!graphData.enableLockMaterials)
            {
                sb.AppendLine("CBUFFER_START(UnityPerMaterial)");
                foreach (var property in properties)
                {
                    if (property.IsTextureType || property.type == PropertyType.KeywordToggle || property.declaration == PropertyDeclaration.Instance)
                    {
                        continue;
                    }
                    sb.AppendLine(property.GetFieldDeclaration(generationMode));
                }
                sb.AppendLine("CBUFFER_END");
            }
            else
            {
                WriteLockMaterialProperties(sb, graphData);
            }

            sb.AppendLine();
            sb.AppendLine("UNITY_INSTANCING_BUFFER_START(UnityPerInstance)");
            foreach (var property in properties)
            {
                if (property.declaration != PropertyDeclaration.Instance) continue;
                var decl = property.GetFieldDeclaration(generationMode);
                var declSplit = decl.Split(' ');
                string type = declSplit[0];
                string referenceName = declSplit[1].TrimEnd(';');
                sb.AppendLine($"UNITY_DEFINE_INSTANCED_PROP({type}, {referenceName})");
                sb.AppendLine($"#define {referenceName} UNITY_ACCESS_INSTANCED_PROP(UnityPerInstance, {referenceName})");
            }
            sb.AppendLine("UNITY_INSTANCING_BUFFER_END(UnityPerInstance)");
            sb.AppendLine();

            sb.AppendLine("#include \"Packages/com.z3y.graphlit/ShaderLibrary/GraphFunctions.hlsl\"");


            if (graphData.enableLockMaterials)
            {
                sb.AppendLine("#define Texture2D GraphlitTexture2D");
            }

            foreach (var function in functions)
            {
                if (string.IsNullOrEmpty(function))
                {
                    continue;
                }
                var lines = function.Split('\n');
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
            }

            if (graphData.enableLockMaterials)
            {
                sb.AppendLine("#undef Texture2D");
            }

            sb.AppendInclude(vertexDataPath);
            sb.Space();
            AppendVertexDescription(sb, graphData);

            varyings.AppendUnpackDefinesForTarget(sb);

            sb.AppendInclude(fragmentDataPath);
            sb.Space();
            AppendSurfaceDescription(sb, graphData);

            foreach (var include in preincludes)
            {
                sb.AppendLine("#include_with_pragmas \"" + include + '"');
            }
            sb.AppendLine("#include_with_pragmas \"" + vertexShaderPath + '"');
            sb.AppendLine("#include_with_pragmas \"" + fragmentShaderPath + '"');
        }

        int GenerateSamplerStateHash(Texture tex)
        {
            var hash = new System.HashCode();

            hash.Add(tex.wrapMode);
            hash.Add(tex.filterMode);
            hash.Add(tex.anisoLevel);
            // hash.Add(tex.wrapModeU);
            // hash.Add(tex.wrapModeV);
            // hash.Add(tex.wrapModeW);

            return hash.ToHashCode();
        }

        void WriteLockMaterialProperties(ShaderStringBuilder sb, GraphData graphData)
        {
            sb.AppendLine("static uint materialID;");

            foreach (var property in properties)
            {
                if (property.IsTextureType || property.type == PropertyType.KeywordToggle || property.declaration == PropertyDeclaration.Instance)
                {
                    continue;
                }
                string referenceName = property.GetReferenceName(generationMode);
                string referenceNameArray = referenceName + "_Array";
                string typeOnly = property.GetFieldTypeOnly();

                bool sameValue = AllPropertiesHaveSameValue(graphData.lockMaterials, referenceName, property.type);

                if (sameValue)
                {
                    string stringValue = GetPropertyStringValue(property, typeOnly, referenceName, graphData.lockMaterials[0]);
                    sb.AppendLine($"const static {typeOnly} {referenceName} = {stringValue};");
                }
                else
                {

                    sb.Append($" const static {typeOnly} ");
                    sb.Append(referenceNameArray);
                    int materialCount = graphData.lockMaterials.Count;
                    sb.Append($"[{materialCount}] = ");
                    sb.Append("{ ");

                    for (int i = 0; i < materialCount; i++)
                    {
                        Material mat = graphData.lockMaterials[i];
                        string value = GetPropertyStringValue(property, typeOnly, referenceName, mat);

                        sb.Append(value);

                        if (i < materialCount - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    sb.Append(" };");
                    sb.AppendLine();

                    sb.AppendLine($"#define {referenceName} {referenceNameArray}[materialID]");
                }

            }

            var samplerStateMap = new Dictionary<int, string>();

            foreach (var property in properties.Where(x => x.IsTextureType))
            {
                string referenceName = property.GetReferenceName(GenerationMode.Final);
                for (int i = 0; i < graphData.lockMaterials.Count; i++)
                {
                    Material mat = graphData.lockMaterials[i];
                    var tex = mat.GetTexture(referenceName);
                    if (tex)
                    {
                        int samplerHash = GenerateSamplerStateHash(tex);
                        sb.AppendLine($"Texture2D {referenceName}{i};");

                        if (!samplerStateMap.TryGetValue(samplerHash, out var name))
                        {
                            string sampler = "optimizer_";

                            sampler += tex.filterMode switch
                            {
                                FilterMode.Point => "Point",
                                FilterMode.Bilinear => "Linear",
                                FilterMode.Trilinear => "Trilinear",
                                _ => "Linear",
                            };

                            sampler += tex.wrapMode switch
                            {
                                TextureWrapMode.Repeat => "_Repeat",
                                TextureWrapMode.Clamp => "_Clamp",
                                TextureWrapMode.Mirror => "_Mirror",
                                TextureWrapMode.MirrorOnce => "_MirrorOnce",
                                _ => "_Repeat",
                            };

                            if (tex.filterMode != FilterMode.Point)
                            {
                                sampler += "_Aniso" + (tex.anisoLevel == 1 ? 16 : tex.anisoLevel);
                            }

                            samplerStateMap[samplerHash] = sampler;
                            sb.AppendLine($"SamplerState {sampler};");
                        }
                    }
                }
            }

            sb.AppendLine("struct GraphlitTexture2D");
            sb.Indent();
            // sb.AppendLine("Texture2D tex;");
            sb.AppendLine("int type;");

            sb.AppendLine("float4 Sample(SamplerState smp, float2 uv)");
            sb.Indent();
            AppendOptimizerTextureSampleMethod(sb, graphData, samplerStateMap, "SAMPLE_TEXTURE2D");
            sb.UnIndent();

            sb.AppendLine("float4 SampleLevel(SamplerState smp, float2 uv, float arg3)");
            sb.Indent();
            AppendOptimizerTextureSampleMethod(sb, graphData, samplerStateMap, "SAMPLE_TEXTURE2D_LOD");
            sb.UnIndent();

            sb.AppendLine("float4 SampleBias(SamplerState smp, float2 uv, float arg3)");
            sb.Indent();
            AppendOptimizerTextureSampleMethod(sb, graphData, samplerStateMap, "SAMPLE_TEXTURE2D_BIAS");
            sb.UnIndent();

            sb.UnIndent("};");
        }

        private void AppendOptimizerTextureSampleMethod(ShaderStringBuilder sb, GraphData graphData, Dictionary<int, string> samplerStateMap, string sampleMethod)
        {
            bool useArg3 = sampleMethod != "SAMPLE_TEXTURE2D";

            foreach (var property in properties.Where(x => x.IsTextureType))
            {
                sb.AppendLine($"if (type == {properties.IndexOf(property)})");
                sb.Indent();

                string referenceName = property.GetReferenceName(generationMode);
                bool sameValue = AllPropertiesHaveSameValue(graphData.lockMaterials, referenceName, property.type);

                string defaultTextureValueString = property.DefaultTextureToValue();

                if (sameValue)
                {
                    Material mat = graphData.lockMaterials[0];
                    var tex = mat.GetTexture(referenceName);
                    if (tex)
                    {
                        int samplerHash = GenerateSamplerStateHash(tex);
                        sb.AppendLine($"return {sampleMethod}({referenceName}, {samplerStateMap[samplerHash]}, uv{(useArg3 ? ", arg3" : "")}); // {tex.name}");
                    }
                    else
                    {
                        sb.AppendLine($"return {defaultTextureValueString};");
                    }

                }
                else
                {
                    var lockMats = graphData.lockMaterials;
                    int materialCount = lockMats.Count;

                    var sortedMats = new List<MaterialId>();

                    for (int j = 0; j < materialCount; j++)
                    {
                        var matId = new MaterialId
                        {
                            material = lockMats[j],
                            id = j
                        };
                        sortedMats.Add(matId);
                    }

                    sortedMats.Sort((a, b) =>
                    {
                        var ta = a.material.GetTexture(referenceName);
                        var tb = b.material.GetTexture(referenceName);

                        if (ta == null && tb == null) return 0;
                        if (ta == null) return -1;
                        if (tb == null) return 1;

                        return ta.GetInstanceID().CompareTo(tb.GetInstanceID());
                    });


                    int nonNullTextureCount = lockMats.Count(x => x.GetTexture(referenceName) != null);

                    if (nonNullTextureCount > 2)
                    {
                        sb.AppendLine($"[forcecase] switch (materialID) ");
                    }
                    else
                    {
                        sb.AppendLine($"switch (materialID) ");
                    }

                    sb.Indent();

                    if (nonNullTextureCount < materialCount)
                    {
                        sb.AppendLine($"default: return {defaultTextureValueString};");
                    }
                    else
                    {
                        sb.AppendLine("default:");
                    }

                    Texture previousTexture = null;

                    var switchSb = new List<string>();

                    for (int i = sortedMats.Count - 1; i >= 0; i--)
                    {
                        Material mat = sortedMats[i].material;
                        int id = sortedMats[i].id;
                        var tex = mat.GetTexture(referenceName);

                        if (!tex)
                        {
                            continue;
                        }

                        if (previousTexture == tex)
                        {
                            switchSb.Add($"case {id}: // {tex.name}");
                            continue;
                        }
                        else
                        {
                            previousTexture = tex;
                        }

                        int samplerHash = GenerateSamplerStateHash(tex);

                        switchSb.Add($"case {id}: return {sampleMethod}({referenceName}{id}, {samplerStateMap[samplerHash]}, uv{(useArg3 ? ", arg3" : "")}); // {tex.name}");
                    }

                    for (int i = switchSb.Count - 1; i >= 0; i--)
                    {
                        sb.AppendLine(switchSb[i]);
                    }

                    sb.UnIndent();
                }
                sb.UnIndent();
            }
        }

        private static string GetPropertyStringValue(PropertyDescriptor property, string typeOnly, string referenceName, Material mat)
        {
            string value;
            switch (property.type)
            {
                default:
                case PropertyType.Float:
                case PropertyType.Toggle:
                    value = mat.GetFloat(referenceName).ToString(CultureInfo.InvariantCulture);
                    break;
                case PropertyType.Color:
                    value = typeOnly + mat.GetColor(referenceName).linear.ToString()[4..];
                    break;
                case PropertyType.Float2:
                    value = typeOnly + ((Vector2)mat.GetVector(referenceName)).ToString();
                    break;
                case PropertyType.Float3:
                    value = typeOnly + ((Vector3)mat.GetVector(referenceName)).ToString();
                    break;
                case PropertyType.Float4:
                    value = typeOnly + mat.GetVector(referenceName).ToString();
                    break;
                case PropertyType.Integer:
                    value = mat.GetInteger(referenceName).ToString();
                    break;
            }

            return value;
        }

        void AppendOptimizerTextureStructs(ShaderStringBuilder sb, GraphData graphData)
        {

            foreach (var property in properties.Where(x => x.IsTextureType))
            {
                var referenceName = property.GetReferenceName(GenerationMode.Final);
                string structName = referenceName + "Struct";

                int index = properties.IndexOf(property);

                if (property.type == PropertyType.Texture2D)
                {
                    sb.AppendLine($"GraphlitTexture2D {structName};");
                    sb.AppendLine($"{structName}.type = {index};");
                    sb.AppendLine($"#define {referenceName} {structName}");
                }
            }

        }

        public void AppendVertexDescription(ShaderStringBuilder sb, GraphData graphData)
        {
            sb.AppendLine("VertexDescription VertexDescriptionFunction(Attributes attributes, inout Varyings varyings)");
            sb.Indent();
            sb.AppendLine("VertexData data = VertexData::Create(attributes);");
            sb.AppendLine("VertexDescription output = (VertexDescription)0;");
            if (graphData.enableLockMaterials)
            {
                // string materialIdOut = @"materialID = 0;
                // [unroll]
                // for (uint t = 0; t < materialIDThresholdsLength; t++)
                // {
                //     if (attributes.vertexID >= materialIDThresholds[t])
                //     {
                //         materialID++;
                //     }
                // }
                // varyings.materialID = materialID;
                // ";
                // sb.AppendLine(materialIdOut);
                sb.AppendLine("varyings.materialID = attributes.uv0.z;");
                sb.AppendLine("materialID = varyings.materialID;");

                AppendOptimizerTextureStructs(sb, graphData);
            }

            foreach (var line in vertexDescription)
            {
                sb.AppendLine(line);
            }
            varyings.AppendVaryingPacking(sb);


            sb.AppendLine("return output;");
            sb.UnIndent();
        }

        public void AppendSurfaceDescription(ShaderStringBuilder sb, GraphData graphData)
        {
            sb.AppendLine("SurfaceDescription SurfaceDescriptionFunction(Varyings varyings)");
            sb.Indent();
            varyings.AppendVaryingUnpacking(sb);

            sb.AppendLine("FragmentData data = FragmentData::Create(varyings);");
            sb.AppendLine($"SurfaceDescription output = (SurfaceDescription)0;");
            if (graphData.enableLockMaterials)
            {
                // setup static materialID
                sb.AppendLine($"materialID = varyings.materialID;");

                AppendOptimizerTextureStructs(sb, graphData);

                if (graphData.optimizerMixedCull)
                {
                    sb.AppendLine("if (_Cull == 1 && data.frontFace) discard; // Cull Back");
                    sb.AppendLine("if (_Cull == 2 && !data.frontFace) discard; // Cull Front");
                }
            }
            foreach (var line in surfaceDescription)
            {
                sb.AppendLine(line);
            }
            sb.AppendLine("return output;");
            sb.UnIndent();
        }
    }
}
