using System.Collections.Generic;
using System.Globalization;
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

            else if (type == PropertyType.Float)
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

            else if (type == PropertyType.Integer || type == PropertyType.Bool)
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

        public void AppendPassHLSL(ShaderStringBuilder sb, GraphData graphData)
        {
            sb.AppendLine();

            if (graphData.enableLockMaterials)
            {
                attributes.RequireVertexID();
                varyings.RequireCustomString("uint materialID : MATERIALID;");
            }

            sb.AppendLine("#pragma target " + target);
            sb.AppendLine("#pragma vertex vert");
            sb.AppendLine("#pragma fragment frag");

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
                sb.AppendLine("#define GRAPHLIT_SAMPLE_TEXTURE2D(tex, smp, uv) SAMPLE_TEXTURE2D(tex, smp, uv)");
            }
            else
            {
                sb.AppendLine("#define GRAPHLIT_SAMPLE_TEXTURE2D(tex, smp, uv) SAMPLE_TEXTURE2D_SWITCH_##tex(smp, uv)");

                var thresholds = graphData.materialIDThresholds;
                sb.AppendLine($"const static uint materialIDThresholdsLength = {thresholds.Count - 1};");
                sb.AppendLine("const static uint materialIDThresholds[materialIDThresholdsLength] = {");

                for (int i = 1; i < thresholds.Count; i++)
                {
                    int threshold = thresholds[i];
                    sb.Append($"{threshold}");
                    if (i != thresholds.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append("};");
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

                        sb.Append($"const static {typeOnly} ");
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

                foreach (var property in properties)
                {
                    if (!property.IsTextureType)
                    {
                        continue;
                    }

                    string referenceName = property.GetReferenceName(generationMode);
                    bool sameValue = AllPropertiesHaveSameValue(graphData.lockMaterials, referenceName, property.type);

                    if (sameValue)
                    {

                    }
                    else
                    {
                        int materialCount = graphData.lockMaterials.Count;

                        string defaultTextureValueString = property.DefaultTextureToValue();


                        int firstSampler = -1;
                        for (int i = 0; i < materialCount; i++)
                        {
                            Material mat = graphData.lockMaterials[i];
                            if (mat.GetTexture(referenceName))
                            {
                                sb.AppendLine($"Texture2D {referenceName}{i};");
                                if (firstSampler < 0)
                                {
                                    firstSampler = i;
                                }
                            }
                        }
                        sb.AppendLine($"SamplerState sampler{referenceName}{firstSampler};");

                        sb.AppendLine($"float4 SAMPLE_TEXTURE2D_SWITCH_{referenceName}(SamplerState smp, float2 uv)");
                        sb.AppendLine("{ [forcecase] switch (materialID) {");
                        sb.AppendLine("default:");

                        for (int i = 0; i < materialCount; i++)
                        {
                            Material mat = graphData.lockMaterials[i];
                            if (mat.GetTexture(referenceName))
                            {
                                sb.AppendLine($"case {i}: return SAMPLE_TEXTURE2D({referenceName}{i}, sampler{referenceName}{firstSampler}, uv);");
                            }
                            else
                            {
                                sb.AppendLine($"case {i}: return {defaultTextureValueString};");
                            }
                        }
                        sb.AppendLine("}}");

                    }


                }
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

            foreach (var property in properties)
            {
                if (!property.IsTextureType) continue;
                sb.AppendLine(property.GetFieldDeclaration(generationMode));
            }
            sb.AppendLine();

            sb.AppendLine("#include \"Packages/com.z3y.graphlit/ShaderLibrary/GraphFunctions.hlsl\"");

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

        private static string GetPropertyStringValue(PropertyDescriptor property, string typeOnly, string referenceName, Material mat)
        {
            string value;
            switch (property.type)
            {
                default:
                case PropertyType.Float:
                    value = mat.GetFloat(referenceName).ToString(CultureInfo.InvariantCulture);
                    break;
                case PropertyType.Color:
                    value = typeOnly + mat.GetColor(referenceName).linear.ToString()[4..];
                    break;
                case PropertyType.Float2:
                case PropertyType.Float3:
                case PropertyType.Float4:
                    value = typeOnly + mat.GetVector(referenceName).ToString();
                    break;
                case PropertyType.Integer:
                case PropertyType.Bool:
                    value = mat.GetInteger(referenceName).ToString();
                    break;
            }

            return value;
        }

        public void AppendVertexDescription(ShaderStringBuilder sb, GraphData graphData)
        {
            sb.AppendLine("VertexDescription VertexDescriptionFunction(Attributes attributes, inout Varyings varyings)");
            sb.Indent();
            sb.AppendLine("VertexData data = VertexData::Create(attributes);");
            sb.AppendLine("VertexDescription output = (VertexDescription)0;");
            if (graphData.enableLockMaterials)
            {
                string materialIdOut = @"materialID = 0;
                [unroll]
                for (uint t = 0; t < materialIDThresholdsLength; t++)
                {
                    if (attributes.vertexID >= materialIDThresholds[t])
                    {
                        materialID++;
                    }
                }
                varyings.materialID = materialID;
                ";
                sb.AppendLine(materialIdOut);
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
