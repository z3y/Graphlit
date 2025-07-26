using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using UnityEngine.UIElements;

namespace Graphlit
{
    [NodeInfo("Texture/Sample Texture 2D"), System.Serializable]
    public class SampleTexture2DNode : SampleTextureNode
    {
        [SerializeField] public bool autoKeyword = false;

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var toggle = new Toggle("Auto Keyword") { value = autoKeyword, tooltip = "Automatically create a keyword toggle when the texture is not set in the inspector and use the default texture value to skip sampling" };
            toggle.RegisterValueChangedCallback(x => autoKeyword = x.newValue);
            root.Add(toggle);
        }
    }
    [NodeInfo("Texture/Sample Texture 2D LOD"), System.Serializable]
    public class SampleTexture2DLodNode : SampleTexture2DNode
    {
        public override bool HasLod => true;
        public override string SampleMethod => $"SAMPLE_TEXTURE2D_LOD({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[LOD].Name})";
    }

    [NodeInfo("Texture/Sample Texture 2D BIAS"), System.Serializable]
    public class SampleTexture2DBiasNode : SampleTexture2DNode
    {
        public override bool HasBias => true;
        public override string SampleMethod => $"SAMPLE_TEXTURE2D_BIAS({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[BIAS].Name})";
    }
}