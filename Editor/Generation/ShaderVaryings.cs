using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace ZSG
{
    public class ShaderVaryings
    {
        public ShaderVaryings(ShaderAttributes attributes)
        {
            _attributes = attributes;
        }

        readonly ShaderAttributes _attributes;

        public enum VaryingType
        {
            PositionCS,
            PositionWS,
            NormalWS,
            UV0,
            UV1,
            UV2,
            UV3,
            TangentWS,
            Color,
            Custom
        }

        public struct VaryingDescriptor
        {
            public string name;
            public string semantic;
            public string prefix;
            public VaryingType type;
            public string passthrough;
            public int channels;
        }

        public List<VaryingDescriptor> varyings = new();
        public HashSet<string> customVaryingsStrings = new();

        public string RequirePositionCS(int channels = 4)
        {
            _attributes.RequirePositionOS(3);
            return RequireInternal(VaryingType.PositionCS, "positionCS", "SV_POSITION", channels);
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
                semantic = "TEXCOORD",
                type = VaryingType.Custom,
                channels = channels,
                passthrough = value
            };
            varyings.Add(desc);

            return Mask(desc.name, channels);
        }

        /*        public string RequirePositionWS(int channels = 4)
                {
                    _attributes.RequirePositionOS(3);
                    return RequireInternal(VaryingType.PositionWS, "positionWS", "TEXCOORD", channels);
                }*/

        public string RequireUV(int texcoord, int channels = 4)
        {
            VaryingType type = VaryingType.UV0;
            switch (texcoord)
            {
                case 0: type = VaryingType.UV0; break;
                case 1: type = VaryingType.UV1; break;
                case 2: type = VaryingType.UV2; break;
                case 3: type = VaryingType.UV3; break;
            }
            return RequireInternal(type, "uv" + texcoord, "TEXCOORD", channels, _attributes.RequireUV(texcoord, channels));
        }

        private string RequireInternal(VaryingType type, string name, string semantic, int channels = 4, string passthrough = null)
        {
            int index = varyings.FindIndex(x => x.type == type);

            if (index < 0)
            {
                var desc = new VaryingDescriptor
                {
                    name = name,
                    semantic = semantic,
                    type = type,
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
            foreach (var vary in varyings)
            {
                var semantic = vary.semantic;
                if (semantic == "TEXCOORD")
                {
                    continue;
                }
                sb.AppendLine($"float{vary.channels} {vary.name} : {semantic};");
            }
            foreach (var b in _packingBins)
            {
                b.name = "interp" + semanticCounter;
                sb.AppendLine($"float{b.capacity} {b.name} : TEXCOORD{semanticCounter++};");
            }
            foreach (var var in customVaryingsStrings)
            {
                sb.AppendLine(var);
            }
        }

        List<string> _varyingsWithoutPacking = new();
        List<string> _varyingPackingVertex = new();
        List<PackingBin> _packingBins = new();

        public void AppendVaryingPacking(ShaderStringBuilder sb)
        {
            foreach (var vary in _varyingPackingVertex)
            {
                sb.AppendLine(vary);
            }
            foreach (var b in _packingBins)
            {
                int offset = 0;
                foreach (var v in b.varyings)
                {
                    string vMasked = Mask(b.name, v.channels, offset);
                    offset += v.channels;
                    sb.AppendLine("varyings." + vMasked + " = " + v.passthrough + ";");
                }
            }
        }

        public void PackVaryings()
        {
            _varyingsWithoutPacking.Clear();
            _varyingPackingVertex.Clear();
            _packingBins.Clear();
            var packed = new List<VaryingDescriptor>();
            //var regular = new List<VaryingDescriptor>();

            foreach (var v in varyings)
            {
                if (v.semantic == "TEXCOORD")
                {
                    packed.Add(v);
                }
                else
                {
                    //regular.Add(v);
                }
            }
/*            foreach (var var in regular)
            {
                if (!string.IsNullOrEmpty(var.passthrough))
                {
                    _varyingPackingVertex.Add("varyings." + var.name + " = " + var.passthrough + ";");

                    string input = Mask("varyings." + var.name, var.channels);
                    _varyingsWithoutPacking.Add($"float{var.channels} {var.name} = {input};");
                }
            }*/

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
            return input + "." + "xyzw".Substring(offset, count);
        }

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
                    sb.AppendLine($"float{v.channels} {v.name} = {input};");
                }
            }
        }
    }
}