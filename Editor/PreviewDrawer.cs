using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZSG
{
    public class PreviewDrawer : ImmediateModeElement, IDisposable
    {
        const int Resolution = 128;
        private static readonly Matrix4x4 _matrix = Matrix4x4.TRS(new Vector3(Resolution / 2.0f, Resolution / 2.0f, -10f), Quaternion.Euler(0,180,180), new Vector3(Resolution, Resolution, 1));
        public Material material;
        public static List<Material> materials = new List<Material>();

        public static MaterialPropertyBlock propertyBlock = new();

        private static Mesh _quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        private static Mesh _sphere = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

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
            material.SetPass(0);
            if (preview3D)
            {
                //r t = Time.realtimeSinceStartup;
                var rotation = Quaternion.Euler(0, 180 , 180);
                Graphics.DrawMeshNow(_sphere, Matrix4x4.TRS(new Vector3(Resolution / 2.0f, Resolution / 2.0f, -10f), rotation, new Vector3(Resolution/2f, Resolution/2f, 1)));
            }
            else
            {
                Graphics.DrawMeshNow(_quad, _matrix);
            }
        }

        ~PreviewDrawer()
        {
            Dispose();
        }
    }
}