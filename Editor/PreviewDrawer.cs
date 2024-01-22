using System;
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
        public static Material PreviewMaterial = new (Shader.Find("Unlit/Color"))
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        private Shader _shader;

        public bool preview3D = false;

        public PreviewDrawer()
        {
            style.width = Resolution;
            style.height = Resolution;

            name = "PreviewDrawer";
        }

        public void SetShader(Shader shader)
        {
            _shader = shader;
            MarkDirtyRepaint();
        }

        public void Dispose()
        {
            if (PreviewMaterial)
            {
                if (PreviewMaterial.shader)
                {
                    GameObject.DestroyImmediate(PreviewMaterial.shader);
                }
                GameObject.DestroyImmediate(PreviewMaterial);
            }
        }

        protected override void ImmediateRepaint()
        {
            if (!PreviewMaterial)
            {
                return;
            }

            if (_shader is not null)
            {
                PreviewMaterial.shader = _shader;
            }

            Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, PreviewMaterial, 0);

            MarkDirtyRepaint();
        }

        ~PreviewDrawer()
        {
            Dispose();
        }
    }
}