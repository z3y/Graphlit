using System.Collections.Generic;

namespace ZSG
{
    [System.Serializable]
    public class GraphData
    {
        public string shaderName = "Default Shader";
        public List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
    }
}