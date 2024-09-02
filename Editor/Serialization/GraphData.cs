using System.Collections.Generic;

namespace Enlit
{
    [System.Serializable]
    public class GraphData
    {
        public string shaderName = "Default Shader";
        public List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
        public GraphPrecision precision = GraphPrecision.Half;
        public DefaultPreviewState defaultPreviewState = DefaultPreviewState.Enabled;
        public string customEditor = string.Empty;
        public string fallback = string.Empty;
        public string include = string.Empty;
        public OutlinePassMode outlinePass = OutlinePassMode.Disabled;
        public bool stencil = false;
        public VRCFallbackTags vrcFallbackTags = new VRCFallbackTags();

        public enum GraphPrecision
        {
            Half = 0,
            Float = 1
        }
        public enum OutlinePassMode
        {
            Disabled = 0,
            Enabled = 1,
            EnabledEarly = 2,
        }
        public enum DefaultPreviewState
        {
            Enabled = 0,
            Disabled = 1
        }
    }
}