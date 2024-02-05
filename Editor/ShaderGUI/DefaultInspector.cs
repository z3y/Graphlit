using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZSG
{
    public partial class DefaultInspector : ShaderGUI
    {
        void OnGUIStart(MaterialEditor editor, MaterialProperty[] properties)
        {
            _material = (Material)editor.target;
            _shader = _material.shader;

            Initialize(editor, properties);
        }

        bool _start = true;
        Material _material;
        Shader _shader;
        PropertyElement _propertyTree;
        static bool _reinitialize = false;
        public static void Reinitialize() => _reinitialize = true;
        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            if (_start || _reinitialize)
            {
                OnGUIStart(editor, properties);
                _start = false;
                _reinitialize = false;
            }


            UserProperties(editor, properties);
            Footer(editor);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            material.shaderKeywords = null;
            SetupRenderingMode(material);
        }

        void Initialize(MaterialEditor editor, MaterialProperty[] properties)
        {
            _propertyTree = new PropertyElement(0);
            PropertyElement current = _propertyTree;
            int baseIndentation = EditorGUI.indentLevel;
            for (int i = 0; i < properties.Length; i++)
            {
                MaterialProperty materialProperty = properties[i];
                var flags = materialProperty.flags;

                if (flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                {
                    continue;
                }

                var p = new PropertyElement(i);
                var attributes = _shader.GetPropertyAttributes(i).ToHashSet();
                string referenceName = materialProperty.name;

                string tooltip = null;
                foreach (string a in attributes)
                {
                    if (a.StartsWith("Tooltip("))
                    {
                        tooltip = a.Substring("Tooltip(".Length).TrimEnd(')');
                    }
                }


                var guiContent = new GUIContent(materialProperty.displayName, tooltip);

                if (attributes.Contains("Foldout"))
                {
                    p.onGui = (e, m, i) =>
                    {
                        CoreEditorUtils.DrawSplitter();
                        p.hideChildren = CoreEditorUtils.DrawHeaderFoldout(materialProperty.displayName, m[i].floatValue == 0);
                        m[i].floatValue = p.hideChildren ? 0 : 1;
                    };

                    _propertyTree.Add(p);
                    current = p;
                    continue;
                }
                if (attributes.Contains("FoldoutEnd"))
                {
                    current = _propertyTree;
                    continue;
                }

                if (referenceName == "_Mode")
                {
                    p.onGui = (e, m, i) => RenderingModeProperty(e, m[i], guiContent);
                }
                else if (materialProperty.type == MaterialProperty.PropType.Texture)
                {
                    p.onGui = (e, m, i) => TextureProperty(e, m[i], guiContent);
                    if (attributes.Contains("Linear"))
                    {
                        p.onGui += (e, m, i) => LinearWarning(m[i]);
                    }
                }
                else if (materialProperty.type == MaterialProperty.PropType.Vector)
                {
                    if (attributes.Contains("Vector2"))
                    {
                        p.onGui = (e, m, i) => Vector2Property(e, m[i], guiContent);
                    }
                    else if (attributes.Contains("Vector3"))
                    {
                        p.onGui = (e, m, i) => Vector3Property(e, m[i], guiContent);
                    }
                    else
                    {
                        p.onGui = (e, m, i) => Vector4Property(e, m[i], guiContent);
                    }
                }
                else
                {
                    p.onGui = (e, m, i) => e.ShaderProperty(m[i], guiContent);
                }

                current.Add(p);
            }
        }

        void UserProperties(MaterialEditor editor, MaterialProperty[] properties)
        {
            foreach (PropertyElement p in _propertyTree.Children)
            {
                p.OnGUI(editor, properties);
            }
        }

        public void Footer(MaterialEditor editor)
        {
            CoreEditorUtils.DrawSplitter();
            EditorGUILayout.Space();
            editor.LightmapEmissionProperty();
            editor.RenderQueueField();
            editor.EnableInstancingField();
            editor.DoubleSidedGIField();
        }

        public class PropertyElement
        {
            public PropertyElement(int index)
            {
                _index = index;
            }
            int _index;
            public bool hideChildren = false;
            public Action<MaterialEditor, MaterialProperty[], int> onGui;
            public List<PropertyElement> Children { get; private set; }
            public PropertyElement Parent { get; private set; }
            public void Add(PropertyElement p)
            {
                Children ??= new List<PropertyElement>();
                p.Parent = this;
                Children.Add(p);
            }

            public void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
            {
                onGui.Invoke(editor, properties, _index);

                if (hideChildren && Children is not null)
                {
                    foreach (PropertyElement p in Children)
                    {
                        p.OnGUI(editor, properties);
                    }
                }
            }
        }

        public virtual void SetupRenderingMode(Material material)
        {
            const string Mode = "_Mode";
            if (!material.HasFloat(Mode))
            {
                return;
            }
            int mode = (int)material.GetFloat(Mode);
            ToggleKeyword(material, "_ALPHATEST_ON", mode == 1);
            ToggleKeyword(material, "_ALPHAFADE_ON", mode == 2);
            ToggleKeyword(material, "_ALPHAPREMULTIPLY_ON", mode == 3);
            ToggleKeyword(material, "_ALPHAMODULATE_ON", mode == 5);

            switch (mode)
            {
                case 0:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_AlphaToMask", 0);
                    material.renderQueue = -1;
                    break;
                case 1: // cutout a2c
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    material.SetInt("_AlphaToMask", 1);
                    break;
                case 2: // alpha fade
                    SetupTransparentMaterial(material);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case 3: // premultiply
                    SetupTransparentMaterial(material);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case 4: // additive
                    SetupTransparentMaterial(material);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case 5: // multiply
                    SetupTransparentMaterial(material);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    break;
            }
        }

        public static void SetupTransparentMaterial(Material material)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetInt("_ZWrite", 0);
            material.SetInt("_AlphaToMask", 0);
        }

        public static void ToggleKeyword(Material material, string keyword, bool value)
        {
            if (value)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }
    }
}