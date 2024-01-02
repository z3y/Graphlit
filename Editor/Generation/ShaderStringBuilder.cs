using System.Text;
using UnityEngine;

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
}