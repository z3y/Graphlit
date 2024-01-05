using System.Text;

namespace z3y.ShaderGraph
{ 
    public class ShaderStringBuilder
    {
        private StringBuilder _sb = new StringBuilder();
        public int indentLevel = 0;

        public void AppendLine(string value)
        {
            _sb.AppendLine();
            for (int i = 0; i < indentLevel; i++)
            {
                _sb.Append("    ");
            }
            _sb.Append(value);
        }

        public void Indent()
        {
            AppendLine("{");
            indentLevel++;
        }
        public void UnIndent(string end = "}")
        {
            indentLevel--;
            AppendLine(end);
            _sb.AppendLine();
        }

        public void Append(string value)
        {
            _sb.Append(value);
        }
        public void AppendLineNormal(string value)
        {
            _sb.AppendLine(value);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }

    public static class StringBuilderExtensionMethods
    {
        public static void AppendLines(this StringBuilder sb, params string[] lines)
        {
            sb.AppendLine();
            foreach (var line in lines)
            {
                sb.Append(line);
            }
        }
    }
}