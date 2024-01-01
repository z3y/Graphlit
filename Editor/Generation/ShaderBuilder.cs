using System.Collections.Generic;

namespace z3y.ShaderGraph
{
    public class ShaderBuilder
    {
        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public HashSet<string> properties = new();
        public List<PassBuilder> passBuilders = new();


        private ShaderStringBuilder _sb;
        public override string ToString()
        {
            _sb = new ShaderStringBuilder();
            _sb.AppendLine("Shader \"" + shaderName + '"');

            _sb.Indent();
            {
                _sb.AppendLine("Properties");
                _sb.Indent();
                {
                    AppendProperties();
                }
                _sb.UnIndent();

                _sb.AppendLine("SubShader");
                _sb.Indent();
                {
                    AppendSubshader();
                }
                _sb.UnIndent();
            }

            _sb.AppendLine(string.IsNullOrEmpty(fallback) ? "// Fallback None" : "Fallback \"" + fallback + "\"");
            _sb.AppendLine(string.IsNullOrEmpty(customEditor) ? "// CustomEditor None" : "CustomEditor \"" + customEditor + "\"");

            _sb.UnIndent();

            return _sb.ToString();
        }

        private void AppendProperties()
        {
            foreach (var property in properties)
            {
                _sb.AppendLine(property);
            }
        }

        private void AppendSubshader()
        {
            AppendTags(subshaderTags);

            AppendPasses();
        }

        private void AppendTags(Dictionary<string, string> tags)
        {
            _sb.AppendLine("Tags");

            _sb.Indent();
            foreach (var tag in tags)
            {
                _sb.AppendLine('"' + tag.Key + "\" = \"" + tag.Value + '"');
            }
            _sb.UnIndent();
        }

        private void AppendPasses()
        {
            foreach (var pass in passBuilders)
            {
                _sb.AppendLine("Pass");
                _sb.Indent();
                {
                    AppendPass(pass);
                }
                _sb.UnIndent();
            }
        }

        private void AppendPass(PassBuilder pass)
        {
            _sb.AppendLine("Name \"" + pass.name + "\"");
            AppendTags(pass.tags);

            _sb.AppendLine("// Render States");

            _sb.AppendLine("HLSLPROGRAM");
            AppendPassHLSL(pass);
            _sb.AppendLine("ENDHLSL");
        }

        private void AppendPassHLSL(PassBuilder pass)
        {
            _sb.AppendLine("// Pragmas");

            _sb.AppendLine("// Attributes");
            _sb.AppendLine("// Varyings");

            AppendVertexDescription(pass);
            AppendSurfaceDescription(pass);
        }

        private void AppendSurfaceDescription(PassBuilder pass)
        {
            _sb.AppendLine("SurfaceDescription SurfaceDescriptionFunction(Varyings varyings)");
            _sb.Indent();
            foreach (var line in pass.surfaceDescription)
            {
                _sb.AppendLine(line);
            }
            _sb.UnIndent();

        }

        private void AppendVertexDescription(PassBuilder pass)
        {
            _sb.AppendLine("VertexDescription VertexDescriptionFunction(Attributes attributes)");
            _sb.Indent();
            foreach (var line in pass.vertexDescription)
            {
                _sb.AppendLine(line);
            }
            _sb.UnIndent();
        }
    }
}
