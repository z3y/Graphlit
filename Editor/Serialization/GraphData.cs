using System.Collections.Generic;

namespace ZSG
{
    [System.Serializable]
    public class GraphData
    {
        public string shaderName = "Default Shader";
        public List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
        public GraphPrecision precision = GraphPrecision.Half;

        public enum GraphPrecision
        {
            Half = 0,
            Float = 1
        }
    }
}