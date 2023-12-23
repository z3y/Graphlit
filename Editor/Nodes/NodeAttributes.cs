using System;
using UnityEngine;

namespace z3y.ShaderGraph.Nodes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayName : Attribute
    {
        public string text;
        public DisplayName(string text)
        {
            this.text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Tooltip : Attribute
    {
        public string text;
        public Tooltip(string text)
        {
            this.text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IndentationIcon : Attribute
    {
        public Texture2D texture;
        public IndentationIcon(Texture2D texture)
        {
            this.texture = texture;
        }
    }
    
}
