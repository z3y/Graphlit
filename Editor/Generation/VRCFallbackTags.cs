using System;
using System.Text;

namespace Graphlit
{
    [Serializable]
    public class VRCFallbackTags
    {
        public enum ShaderType
        {
            Standard,
            Unlit,
            VertexLit,
            Toon,
            Particle,
            Sprite,
            Matcap,
            MobileToon,
            Hidden
        };

        public enum ShaderMode
        {
            Opaque,
            Cutout,
            Transparent,
            Fade
        };

        public ShaderType type = ShaderType.Standard;
        public ShaderMode mode = ShaderMode.Opaque;
        public bool doubleSided = false;

        public override string ToString()
        {
            if (type == 0 && mode == 0 && !doubleSided)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            if (type != 0) sb.Append(Enum.GetName(typeof(ShaderType), type));
            if (mode != 0) sb.Append(Enum.GetName(typeof(ShaderMode), mode));
            if (doubleSided) sb.Append("DoubleSided");

            return sb.ToString();
        }
    }
}