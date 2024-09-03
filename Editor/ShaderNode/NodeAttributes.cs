using System;
using UnityEngine;

namespace Graphlit.Nodes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
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
