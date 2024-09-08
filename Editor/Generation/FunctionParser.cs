using System;
using System.Collections.Generic;
using UnityEngine;
using Graphlit.Nodes.PortType;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Graphlit
{
    public class FunctionParser
    {
        public static IPortType StringToPortType(string type, bool unknown)
        {
            if (unknown)
            {
                return new CustomType(type);
            }
            return type switch
            {
                "float" or "half" => new Float(1),
                "float2" or "half2" => new Float(2),
                "float3" or "half3" => new Float(3),
                "float4" or "half4" => new Float(4),
                "Texture2D" => new Texture2DObject(),
                "Texture2DArray" => new Texture2DArrayObject(),
                "Texture3D" => new Texture3DObject(),
                "TextureCube" => new TextureCubeObject(),
                "TextureCubeArray" => new TextureCubeArrayObject(),
                "SamplerState" => new SamplerState(),
                "bool" => new Bool(),
                "int" => new Int(),
                "uint" => new UInt(),
                _ => new CustomType(type),
            }; ;
        }

        readonly string[] EntryKeywords = new[]
        {
            "void ",
            "float",
            "half",
            "Texture",
            "SamplerState",
            "bool",
            "int",
            "uint"
        };

        public List<PortDescriptor> descriptors = new List<PortDescriptor>();
        public string methodName;
        public Dictionary<int, PortBinding> bindings = new Dictionary<int, PortBinding>();
        public Dictionary<int, string> defaultValues = new Dictionary<int, string>();
        public bool TryParse(string code)
        {
            descriptors.Clear();
            bindings.Clear();
            defaultValues.Clear();
            try
            {
                int entry = 0;
                string[] lines = code.Split('\n');
                bool hasMain = false;
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    if (Array.Exists(EntryKeywords, x => lines[i].StartsWith(x)))
                    {
                        entry = i;
                        break;
                    }
                    else if (lines[i].StartsWith("[Main] "))
                    {
                        entry = i;
                        hasMain = true;
                        break;
                    }
                }

                string[] split1 = lines[entry].Split('(');
                string[] whitespaceSplit = lines[entry].Split(" ");
                string entryKeyword = hasMain ? whitespaceSplit[1] : whitespaceSplit[0];
                methodName = hasMain ? split1[0][$"[Main] {entryKeyword} ".Length..] : split1[0][$"{entryKeyword} ".Length..];

                string allargs = split1[1].Split(')')[0];
                bool emptyArts = string.IsNullOrEmpty(allargs);
                bool isVoid = entryKeyword.StartsWith("void");

                if (emptyArts && isVoid)
                {
                    return false;
                }

                if (!emptyArts)
                {
                    string[] args = allargs.Split(',');
                    for (int i = 0; i < args.Length; i++)
                    {
                        PortDirection direction = PortDirection.Input;
                        string[] arg = args[i].Trim().Split(' ');
                        for (int j = 0; j < arg.Length; j++)
                        {
                            arg[j] = arg[j].Trim();
                        }

                        int typeArgIndex = 0;
                        if (arg[0] == "out")
                        {
                            direction = PortDirection.Output;
                            typeArgIndex++;
                        }
                        string type = arg[typeArgIndex].Trim();
                        string name = arg[typeArgIndex + 1].Trim();

                        bool isArray = false;
                        if (name.EndsWith(']'))
                        {
                            var splitName = name.Split('[');
                            isArray = true;
                        }

                        int id = i;
                        /*var nameSplit = name.Split("_");
                        if (nameSplit.Length > 1 && int.TryParse(nameSplit[1], out int newId))
                        {
                            id = newId;
                        }*/
                        //var displayName = nameSplit[0];
                        var displayName = name;
                        if (direction == PortDirection.Output) id += 100;
                        descriptors.Add(new(direction, StringToPortType(type, isArray), id, AddSpacesBeforeCapitals(displayName)));

                        if (Enum.TryParse(displayName, true, out PortBinding binding))
                        {
                            bindings[id] = binding;
                        }
                        else if (displayName.Equals("uv", StringComparison.OrdinalIgnoreCase))
                        {
                            bindings[id] = PortBinding.UV0;
                        }
                        else if (displayName.Equals("position", StringComparison.OrdinalIgnoreCase))
                        {
                            bindings[id] = PortBinding.PositionWS;
                        }
                        else if (displayName.Equals("normal", StringComparison.OrdinalIgnoreCase))
                        {
                            bindings[id] = PortBinding.NormalWS;
                        }

                        if (arg.Length > 3 && arg[2].Trim() == "=")
                        {
                            defaultValues[id] = args[i].Split('=')[1].Trim();
                            //Debug.Log($"args = '{args[i]}'");

                            //Debug.Log($"Default Value = '{defaultValues[id]}'");
                        }
                        //Debug.Log($"PortDirection = '{direction}', Type = '{type}', PortName = '{name}'");
                    }
                }

                if (!isVoid)
                {
                    descriptors.Add(new(PortDirection.Output, StringToPortType(entryKeyword.Trim(), false), 99, methodName));
                }


                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse " + code + e);
            }

            return false;

        }

        public static string AddSpacesBeforeCapitals(string input)
        {
            // Add spaces before capital letters and numbers, but not between consecutive numbers
            string spaced = Regex.Replace(input, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]|(?<=[a-zA-Z])\\d)", " $1");

            // Capitalize each word
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(spaced);
        }
    }
}