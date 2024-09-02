using System.Collections.Generic;

namespace Enlit
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
        public List<string> cbuffer = new();
        public List<string> objectDecleration = new();
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

        public string fragmentDataPath = "Packages/com.enlit/Editor/Targets/FragmentData.hlsl";
        public string vertexDataPath = "Packages/com.enlit/Editor/Targets/VertexData.hlsl";

        public GenerationMode generationMode;


        public string vertexShaderPath;
        public string fragmentShaderPath;

        public int[] Ports { get; }

        const string AudioLinkPath = "Packages/com.llealloo.audiolink/Runtime/Shaders/AudioLink.cginc";
        static bool AudioLinkExists = System.IO.File.Exists(AudioLinkPath);

        public void AppendPass(ShaderStringBuilder sb, bool stencil = false)
        {
            varyings.PackVaryings();

            sb.AppendLine("Name \"" + name + "\"");
            ShaderBuilder.AppendTags(sb, tags);

            sb.AppendLine();
            foreach (var state in renderStates)
            {
                sb.AppendLine(state.Key + " " + state.Value);
            }
            if (stencil)
            {
                AppendStencil(sb);
            }

            sb.AppendLine("HLSLPROGRAM");
            AppendPassHLSL(sb);
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
        public void AppendPassHLSL(ShaderStringBuilder sb)
        {
            sb.AppendLine();

            sb.AppendLine("#pragma target " + target);

            foreach (var p in pragmas)
            {
                sb.AppendLine(p);
            }
            if (outlinePass) sb.AppendLine("#define OUTLINE_PASS");
            foreach (var property in properties)
            {
                if (property.type != PropertyType.KeywordToggle) continue;
                sb.AppendLine(property.GetFieldDeclaration(generationMode));
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

            sb.AppendLine("//CBUFFER_START(UnityPerMaterial)");
            foreach (var property in properties)
            {
                if (property.IsTextureType || property.type == PropertyType.KeywordToggle || property.declaration == PropertyDeclaration.Instance) continue;
                sb.AppendLine(property.GetFieldDeclaration(generationMode));
            }
            sb.AppendLine("//CBUFFER_END");
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
            AppendVertexDescription(sb);

            varyings.AppendUnpackDefinesForTarget(sb);

            sb.AppendInclude(fragmentDataPath);
            sb.Space();
            AppendSurfaceDescription(sb);

            foreach (var include in preincludes)
            {
                sb.AppendLine("#include_with_pragmas \"" + include + '"');
            }
            sb.AppendLine("#include_with_pragmas \"" + vertexShaderPath + '"');
            sb.AppendLine("#include_with_pragmas \"" + fragmentShaderPath + '"');
        }

        public void AppendVertexDescription(ShaderStringBuilder sb)
        {
            sb.AppendLine("VertexDescription VertexDescriptionFunction(Attributes attributes, inout Varyings varyings)");
            sb.Indent();
            sb.AppendLine("VertexData data = VertexData::Create(attributes);");
            sb.AppendLine("VertexDescription output = (VertexDescription)0;");
            foreach (var line in vertexDescription)
            {
                sb.AppendLine(line);
            }
            varyings.AppendVaryingPacking(sb);
            sb.AppendLine("return output;");
            sb.UnIndent();
        }

        public void AppendSurfaceDescription(ShaderStringBuilder sb)
        {
            sb.AppendLine("SurfaceDescription SurfaceDescriptionFunction(Varyings varyings)");
            sb.Indent();
            varyings.AppendVaryingUnpacking(sb);

            sb.AppendLine("FragmentData data = FragmentData::Create(varyings);");
            sb.AppendLine($"SurfaceDescription output = (SurfaceDescription)0;");
            foreach (var line in surfaceDescription)
            {
                sb.AppendLine(line);
            }
            sb.AppendLine("return output;");
            sb.UnIndent();
        }
    }
}
