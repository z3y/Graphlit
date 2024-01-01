using System.Collections.Generic;

namespace z3y.ShaderGraph
{
    public class PassBuilder
    {
        public PassBuilder(string name)
        {
            this.name = name;
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


    }
}
