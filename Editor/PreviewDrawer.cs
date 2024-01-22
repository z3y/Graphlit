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
        private Shader _shader;

        public bool preview3D = false;
        private Material _material;

        public PreviewDrawer(ShaderGraphView graphView)
        {
            _material = graphView.PreviewMaterial;
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
            if (_shader)
            {
                GameObject.DestroyImmediate(_shader);
            }
        }

        protected override void ImmediateRepaint()
        {
            if (!_material)
            {
                return;
            }

            if (_shader is not null)
            {
                _material.shader = _shader;
            }

            Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, _material, 0);

            MarkDirtyRepaint();
        }

        ~PreviewDrawer()
        {
            Dispose();
        }
    }
}