using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZSG
{
    enum PreviewType
    {
        Disabled = 0,
        _2D = 1,
        _3D = 2,
    }

    public class PreviewDrawer : ImmediateModeElement, IDisposable
    {
        const int Resolution = 96;
        public Material material;
        public static List<Material> materials = new List<Material>();
        public bool preview3D = false;

        public PreviewDrawer()
        {
            material = new Material(Shader.Find("Unlit/Color"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            materials.Add(material);

            style.width = Resolution;
            style.height = Resolution;

            name = "PreviewDrawer";
        }

        public void SetShader(Shader shader)
        {
            if (!material)
            {
                return;
            }
            material.shader = shader;
            MarkDirtyRepaint();
        }

        public void Dispose()
        {
            if (material)
            {
                if (material.shader)
                {
                    GameObject.DestroyImmediate(material.shader);
                }
                materials.Remove(material);
                GameObject.DestroyImmediate(material);
            }
        }

        protected override void ImmediateRepaint()
        {
            Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, material, 0);

            MarkDirtyRepaint();
        }

        ~PreviewDrawer()
        {
            Dispose();
        }
    }
}