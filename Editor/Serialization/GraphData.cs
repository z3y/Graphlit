using System.Collections.Generic;

namespace ZSG
{
    [System.Serializable]
    public class GraphData
    {
        public string shaderName = "Default Shader";
        public List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
        public GraphPrecision precision = GraphPrecision.Half;
        public DefaultPreviewState defaultPreviewState = DefaultPreviewState.Enabled;

        public enum GraphPrecision
        {
            Half = 0,
            Float = 1
        }
        public enum DefaultPreviewState
        {
            Enabled = 0,
            Disabled = 1
        }
    }
}