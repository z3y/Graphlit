using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    public class PreviewDrawer : IDisposable
    {
        const int width = 128, height = 128;

        private CustomRenderTexture _rt = new CustomRenderTexture(width, height, RenderTextureFormat.ARGB32);
        public Material material;

        public void Initialize(Shader shader)
        {
            material = new Material(shader);
            _rt.material = material;
            _rt.updateMode = CustomRenderTextureUpdateMode.Realtime;
            _rt.initializationMode = CustomRenderTextureUpdateMode.Realtime;
            _rt.initializationSource = CustomRenderTextureInitializationSource.Material;
            _rt.initializationMaterial = material;
            _rt.updatePeriod = 0;

            _rt.Initialize();
        }

        public VisualElement GetVisualElement()
        {
            var gui = new IMGUIContainer(OnGUI);
            return gui;
        }

        public void OnGUI()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));
            EditorGUI.DrawPreviewTexture(rect, _rt);
        }
            
        public void Dispose()
        {
            _rt.Release();
            material = null;
        }
    }
}