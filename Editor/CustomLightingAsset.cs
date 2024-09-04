using System.Collections.Generic;
using UnityEngine;
using static Graphlit.CustomLighting;

namespace Graphlit
{
    public class CustomLightingAsset : ScriptableObject
    {
        public List<PropertyDescriptor> properties;
        public List<CustomPort> outputs = new();
    }
}