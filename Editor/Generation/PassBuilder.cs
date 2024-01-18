using System.Collections.Generic;

namespace ZSG
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
        public HashSet<string> functions = new();
        public List<string> vertexDescription = new();
        public List<string> vertexDescriptionStruct = new();
        public List<string> surfaceDescription = new();
        public List<string> surfaceDescriptionStruct = new();
        public List<PropertyDescriptor> properties = new();

        public ShaderAttributes attributes;
        public ShaderVaryings varyings;

        public HashSet<string> generatedBindingsVertex = new();
        public HashSet<string> generatedBindingsFragment = new();


        public string vertexShaderPath;
        public string fragmentShaderPath;

        public int[] Ports { get; }

        public void AppendPass(ShaderStringBuilder sb)
        {
            sb.AppendLine("Name \"" + name + "\"");
            ShaderBuilder.AppendTags(sb, tags);

            sb.AppendLine("// Render States");
            foreach (var state in renderStates)
            {
                sb.AppendLine(state.Key + " " + state.Value);
            }

            sb.AppendLine("HLSLPROGRAM");
            AppendPassHLSL(sb);
            sb.AppendLine("ENDHLSL");
        }
        public void AppendPassHLSL(ShaderStringBuilder sb)
        {
            sb.AppendLine("// Pragmas");

            sb.AppendLine("#include \"UnityCG.cginc\"");

            foreach(var p in pragmas)
            {
                sb.AppendLine(p);
            }

            sb.AppendLine("struct Attributes");
            sb.Indent();
            attributes.AppendAttributes(sb);
            sb.UnIndent("};");

            sb.AppendLine("struct Varyings");
            sb.Indent();
            varyings.PackVaryings();
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

            sb.AppendLine("// CBUFFER");
            foreach (var property in properties)
            {
                if (property.Type == PropertyType.Texture2D || property.Type == PropertyType.TextureCube)
                {
                    continue;
                }

                sb.AppendLine(property.Declaration());
            }
            sb.AppendLine("// CBUFFER END");
            sb.AppendLine();

            foreach (var function in functions)
            {
                var lines = function.Split('\n');
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
            }

            AppendVertexDescription(sb);
            AppendSurfaceDescription(sb);

            varyings.AppendUnpackDefinesForTarget(sb);

            sb.AppendLine("#include_with_pragmas \"" + vertexShaderPath + '"');
            sb.AppendLine("#include_with_pragmas \"" + fragmentShaderPath + '"');
        }

        public void AppendVertexDescription(ShaderStringBuilder sb)
        {
            sb.AppendLine("VertexDescription VertexDescriptionFunction(Attributes attributes, inout Varyings varyings)");
            sb.Indent();
            sb.AppendLine("VertexDescription output = (VertexDescription)0;");
            foreach (var line in generatedBindingsVertex)
            {
                sb.AppendLine(line);
            }
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
            foreach (var line in generatedBindingsFragment)
            {
                sb.AppendLine(line);
            }
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
