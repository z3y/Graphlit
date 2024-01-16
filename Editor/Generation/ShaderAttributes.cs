using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZSG
{
/*    [Flags]
    public enum ChannelMask : int
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        W = 1 << 3,
        XY = X | Y,
        XYZ = X | Y | Z,
        XYZW = X | Y | Z | W
    }
*/
    public class ShaderAttributes
    {
        public enum AttributeType
        {
            PositionOS,
            NormalOS,
            UV0,
            UV1,
            UV2,
            UV3,
            TangentOS,
            Color,
            VertexID
        }

        public struct AttributeDescriptor
        {
            public string name;
            public string semantic;
            public string prefix;
            public AttributeType type;
            public int channels;
        }

        public List<AttributeDescriptor> attributes = new();

        public HashSet<string> customAttributes = new();

        public string RequirePositionOS(int channels = 3) => RequireInternal(AttributeType.PositionOS, "positionOS", "POSITION", channels);
        public string RequireNormalOS(int channels = 3) => RequireInternal(AttributeType.NormalOS, "normalOS", "NORMAL", channels);
        public string RequireTangentOS(int channels = 4) => RequireInternal(AttributeType.TangentOS, "tangentOS", "TANGENT", channels);
        public string RequireColor(int channels = 4) => RequireInternal(AttributeType.Color, "color", "COLOR", channels);

        public void Require(string attribute)
        {
            customAttributes.Add(attribute);
        }

        public string RequireUV(int texcoord, int channels = 4)
        {
            AttributeType type = AttributeType.UV0;
            switch (texcoord)
            {
                case 0: type = AttributeType.UV0; break;
                case 1: type = AttributeType.UV1; break;
                case 2: type = AttributeType.UV2; break;
                case 3: type = AttributeType.UV3; break;
            }
            return RequireInternal(type, "uv" + texcoord, "TEXCOORD" + texcoord, channels);
        }

        private string RequireInternal(AttributeType type, string name, string semantic, int channels = 4)
        {
            int index = attributes.FindIndex(x => x.type == type);

            if (index < 0)
            {
                var desc = new AttributeDescriptor
                {
                    name = name,
                    semantic = semantic,
                    type = type,
                    channels = channels
                };
                attributes.Add(desc);
            }
            else
            {
                var attr = attributes[index];
                attr.channels = Mathf.Max(attr.channels, channels);
                attributes[index] = attr;
            }

            return "attributes." + name;
        }

        public void AppendAttributes(ShaderStringBuilder sb)
        {
            foreach (var attr in attributes)
            {
                sb.AppendLine($"float{attr.channels} {attr.name} : {attr.semantic};");
            }
            foreach (var attr in customAttributes)
            {
                sb.AppendLine(attr);
            }
        }
    }
}