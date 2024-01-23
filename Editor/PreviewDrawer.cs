using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZSG
{
    public enum PreviewType
    {
        Disabled = 0,
        _2D = 1,
        _3D = 2,
    }

    public class PreviewDrawer : ImmediateModeElement
    {
        int _resolution = 96;
        string _shader;
        Shader _cachedShader;
        Material _material;
        ShaderGraphView _graphView;
        static Shader _defaultShader = Shader.Find("Unlit/Color");
        Shader PreviewShader
        {
            get
            {
                if (_cachedShader == null)
                {
                    CompileShader();
                }
                return _cachedShader;
            }
        }

        void CompileShader()
        {
            if (string.IsNullOrEmpty(_shader))
            {
                _cachedShader = _defaultShader;
                return;
            }
            _cachedShader = ShaderUtil.CreateShaderAsset(_shader);
        }

        public PreviewDrawer(ShaderGraphView graphView, int resolution = 96)
        {
            _graphView = graphView;
            _material = graphView.PreviewMaterial;
            _resolution = resolution;
            style.width = _resolution;
            style.height = _resolution;

            name = "PreviewDrawer";
        }

        public void SetShader(string shader)
        {
            Dispose();
            _shader = shader;
            CompileShader();
            MarkDirtyRepaint();
        }

        public void Dispose()
        {
            if (_cachedShader)
            {
                GameObject.DestroyImmediate(_cachedShader);
            }
        }
        int _graphTimeId = Shader.PropertyToID("_GraphTime");
        protected override void ImmediateRepaint()
        {
            if (!_material)
            {
                _material = _graphView.PreviewMaterial;
                return;
            }

            if (!string.IsNullOrEmpty(_shader))
            {
                _material.shader = PreviewShader;
            }

            float t = Time.realtimeSinceStartup;

            Vector4 time = new (t / 20.0f, t, t * 2.0f, t * 3.0f);
            //Vector4 timeParameters = new Vector4(t, Mathf.Sin(t), Mathf.Cos(t), 0.0f);

            _material.SetVector(_graphTimeId, time);
            Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, _material, 0);

            MarkDirtyRepaint();
        }
    }
}