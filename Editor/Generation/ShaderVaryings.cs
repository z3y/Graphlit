using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphlit
{
    public class ShaderVaryings
    {
        public ShaderVaryings(ShaderAttributes attributes)
        {
            _attributes = attributes;
        }

        public void UnionWith(ShaderVaryings other)
        {
            foreach (var otherCustomVaryingsString in other.customVaryingsStrings)
            {
                customVaryingsStrings.Add(otherCustomVaryingsString);
            }

            foreach (var varyingDescriptor in other.varyings)
            {
                var existingIndex = varyings.FindIndex(x => x.name == varyingDescriptor.name);
                if (existingIndex >= 0)
                {
                    var existing = varyings[existingIndex];
                    existing.channels = Mathf.Max(existing.channels, varyingDescriptor.channels);
                    varyings[existingIndex] = existing;
                }
                else
                {
                    varyings.Add(varyingDescriptor);
                }
            }
        }

        readonly ShaderAttributes _attributes;
        
        [System.Serializable]
        public struct VaryingDescriptor
        {
            public string name;
            public string passthrough;
            public int channels;
        }

        public List<VaryingDescriptor> varyings = new();
        public HashSet<string> customVaryingsStrings = new();

        public void RequirePositionCS()
        {
            _attributes.RequirePositionOS(3);
            RequireCustomString("float4 positionCS : SV_POSITION;");
        }

        public void RequireCustomString(string varying)
        {
            customVaryingsStrings.Add(varying);
        }
        private int _interpCounter = 0;
        public string RequireCustom(int channels, string value)
        {
            var desc = new VaryingDescriptor
            {
                name = "interp" + _interpCounter++,
                channels = channels,
                passthrough = value,
            };
            varyings.Add(desc);

            return Mask(desc.name, channels);
        }

        public string RequireUV(int texcoord, int channels = 2)
        {
            return RequireInternal("uv" + texcoord, channels, _attributes.RequireUV(texcoord, channels));
        }

        public string RequireCullFace()
        {
            RequireCustomString("#ifdef SHADER_STAGE_FRAGMENT\nFRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;\n#endif");
            return "varyings.cullFace";
        }
        public string RequireColor()
        {
            RequireInternal("color", 4, _attributes.RequireColor());
            return "UNPACK_COLOR";
        }

        internal string RequireInternal(string name, int channels = 4, string passthrough = null)
        {
            int index = varyings.FindIndex(x => x.name == name);

            if (index < 0)
            {
                var desc = new VaryingDescriptor
                {
                    name = name,
                    channels = channels,
                    passthrough = passthrough
                };
                varyings.Add(desc);
            }
            else
            {
                var attr = varyings[index];
                attr.channels = Mathf.Max(attr.channels, channels);
                varyings[index] = attr;
            }

            return Mask(name, channels);
        }

        public void AppendVaryingsStruct(ShaderStringBuilder sb)
        {
            int semanticCounter = 0;
            foreach (var b in _packingBins)
            {
                b.name = "interp" + semanticCounter;
                sb.AppendLine($"float{b.capacity} {b.name} : TEXCOORD{semanticCounter++};");
            }
            foreach (string var in customVaryingsStrings)
            {
                if (var.Contains("cullFace"))
                {
                    continue;
                }
                string value = var;
                if (value.Contains('*'))
                {
                    value = value.Replace("*", semanticCounter++.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                foreach (var split in value.Split('\n'))
                {
                    sb.AppendLine(split);
                }
            }
            // quick hack to place cull face last
            foreach (string var in customVaryingsStrings)
            {
                if (!var.Contains("cullFace"))
                {
                    continue;
                }
                string value = var;
                if (value.Contains('*'))
                {
                    value = value.Replace("*", semanticCounter++.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                foreach (var split in value.Split('\n'))
                {
                    sb.AppendLine(split);
                }
            }
        }

        List<string> _varyingsWithoutPacking = new();
        List<string> _varyingPackingVertex = new();
        List<PackingBin> _packingBins = new();

        public void AppendVaryingPacking(ShaderStringBuilder sb)
        {
            _unpackDefines.Clear();

            foreach (var vary in _varyingPackingVertex)
            {
                sb.AppendLine(vary);
            }
            foreach (var b in _packingBins)
            {
                int offset = 0;
                foreach (var v in b.varyings)
                {
                    if (!string.IsNullOrEmpty(v.passthrough))
                    {
                        string vMasked = Mask(b.name, v.channels, offset);
                        sb.AppendLine("varyings." + vMasked + " = " + Mask(v.passthrough, v.channels) + ";");
                    }


                    
                    string input = Mask("varyings." + b.name, v.channels, offset);
                    if (v.name.StartsWith("uv") && v.channels == 2)
                    {
                        input = $"float4({input}, 0, 0)";
                    }
                    _unpackDefines.Add($"#define UNPACK_{v.name.ToUpper()} {input}");

                    offset += v.channels;
                }
            }
        }

        public void PackVaryings()
        {
            _varyingsWithoutPacking.Clear();
            _varyingPackingVertex.Clear();
            _packingBins.Clear();
            var packed = varyings.ToList();

            var toPack = packed.OrderByDescending(x => x.channels).ToList();
            _packingBins = Pack(toPack);
        }

        List<PackingBin> Pack(List<VaryingDescriptor> varyings)
        {
            var bins = new List<PackingBin>();
            for (int i = 0; i < varyings.Count; i++)
            {
                VaryingDescriptor v = varyings[i];
                PackingBin bestFit = null;
                int bestFitCapacity = 0;
                foreach (var b in bins)
                {
                    int capacity = b.CanPack(v);
                    if (capacity > 0 && capacity > bestFitCapacity)
                    {
                        bestFit = b;
                        bestFitCapacity = capacity;
                    }
                }
                if (bestFitCapacity == 0)
                {
                    var bin = new PackingBin();
                    bin.Pack(v);
                    bins.Add(bin);
                }
                else
                {
                    bestFit.Pack(v);
                }
                i--;
                varyings.Remove(v);
            }

            return bins;
        }

        class PackingBin
        {
            public int capacity = 0;
            public List<VaryingDescriptor> varyings = new();
            public string name = "";

            public int CanPack(VaryingDescriptor desc)
            {
                int result = desc.channels + capacity;
                if (result <= 4)
                {
                    return result;
                }
                return 0;
            }

            public void Pack(VaryingDescriptor desc)
            {
                capacity += desc.channels;
                varyings.Add(desc);
            }
        }

        string Mask(string input, int count, int offset = 0)
        {
            if (count == 4) return input;
            return input + "." + "xyzw".Substring(offset, count);
        }
        private List<string> _unpackDefines = new List<string>();
        public void AppendVaryingUnpacking(ShaderStringBuilder sb)
        {
            foreach (var v in _varyingsWithoutPacking)
            {
                sb.AppendLine(v);
            }
            foreach (var b in _packingBins)
            {
                int offset = 0;
                foreach (var v in b.varyings)
                {
                    string input = Mask("varyings." + b.name, v.channels, offset);
                    offset += v.channels;
                    if (!v.name.StartsWith("uv") && !v.name.StartsWith("interp"))
                    {
                        continue;
                    }
                    sb.AppendLine($"float{v.channels} {v.name} = {input};");
                }
            }
        }

        public void AppendUnpackDefinesForTarget(ShaderStringBuilder sb)
        {
            foreach (var d in _unpackDefines)
            {
                sb.AppendLine(d);
            }
        }
    }
}