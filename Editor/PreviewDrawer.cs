using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZSG
{
    public class PreviewDrawer : IDisposable
    {
        const int width = 128, height = 128;

        private CustomRenderTexture _rt = new CustomRenderTexture(width, height, RenderTextureFormat.ARGB32);
        public Material material;

        public static List<Material> materials = new List<Material>();

        public void Initialize(Shader shader)
        {
            if (material)
            {
                if (material.shader)
                {
                    GameObject.DestroyImmediate(material.shader);
                }
                material.shader = shader;
            }
            else
            {
                material = new Material(shader);
            }

            _rt.material = material;
            _rt.updateMode = CustomRenderTextureUpdateMode.Realtime;
            _rt.initializationMode = CustomRenderTextureUpdateMode.Realtime;
            _rt.initializationSource = CustomRenderTextureInitializationSource.Material;
            _rt.initializationMaterial = material;
            _rt.updatePeriod = 0;

            _rt.Initialize();

            materials.Add(material);
        }

        private void UpdateTime()
        {
            if (!material) return;

            float editorTime = (float)EditorApplication.timeSinceStartup;
            material.SetFloat("_Time", editorTime);
        }

        public VisualElement GetVisualElement()
        {
            var gui = new IMGUIContainer(OnGUI);
            gui.name = "PreviewDrawer";
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

        ~PreviewDrawer()
        {
            Dispose();
        }
    }
}