using System;
using UnityEngine;

namespace z3y.ShaderGraph.Nodes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeInfo : Attribute
    {
        public string name;
        public string tooltip;
        public Texture2D icon;

        public NodeInfo(string name, string tooltip, Texture2D texture)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.icon = texture;
        }

        public NodeInfo(string name)
        {
            this.name = name;
            this.tooltip = null;
            this.icon = null;
        }

        public NodeInfo(string name, string tooltip)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.icon = null;
        }
    }
}
