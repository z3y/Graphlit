using System.Collections.Generic;

namespace z3y.ShaderGraph
{
    public class PassBuilder
    {
        public PassBuilder(string name, string vertexShaderPath, string fragmentShaderPath, params int[] ports)
        {
            this.name = name;
            this.vertexShaderPath = vertexShaderPath;
            this.fragmentShaderPath = fragmentShaderPath;

            Ports = ports;
        }
        public string name;
        public Dictionary<string, string> tags = new();
        public Dictionary<string, string> renderStates = new();
        public List<string> pragmas = new();
        public List<string> attributes = new();
        public List<string> varyings = new();
        public List<string> cbuffer = new();
        public List<string> objectDecleration = new();
        public Dictionary<string, string> functions = new();
        public List<string> vertexDescription = new();
        public List<string> surfaceDescription = new();
        public HashSet<string> properties = new();

        public string vertexShaderPath;
        public string fragmentShaderPath;

        public int[] Ports { get; }
    }
}
