/*using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using System.Globalization;

namespace Graphlit
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
            if (Array.Exists(properties, x => x.name == "_GraphlitPreviewEnabled"))
            {
                EditorGUILayout.LabelField("LIVE PREVIEW ENABLED");
                return;
            }

            var style = GUI.skin.customStyles;
            bool[] richTextState = new bool[style.Length];
            for (int i = 0; i < style.Length; i++)
            {
                richTextState[i] = style[i].richText;
                style[i].richText = true;
            }

            var material = (Material)editor.target;
            if (_shader != material.shader)
            {
                _reinitialize = true;
            }

            if (material.GetFloat("__reset") == 1)
            {
                material.SetFloat("__reset", 0);
                _start = true;
            }

            if (_start || _reinitialize)
            {
                OnGUIStart(editor, properties);
                _start = false;
                _reinitialize = false;
            }

            UserProperties(editor, properties);
            Footer(editor);

            for (int i = 0; i < style.Length; i++)
            {
                style[i].richText = richTextState[i];
            }
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

                p.guiContent = new GUIContent(materialProperty.displayName, tooltip);

                if (attributes.Contains("Foldout"))
                {
                    p.onGui = (e, prop, g) =>
                    {
                        CoreEditorUtils.DrawSplitter();
                        p.hideChildren = CoreEditorUtils.DrawHeaderFoldout(g.text, prop.floatValue == 0);
                        prop.floatValue = p.hideChildren ? 0 : 1;
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


                var minMax = attributes.Where(x => x.StartsWith("MinMax("));
                if (minMax.Count() > 0)
                {
                    string[] split = minMax.First().Substring("MinMax(".Length).TrimEnd(')').Split(',');
                    if (split.Length != 2)
                    {
                        Debug.LogError("Invalid Min Max");
                        continue;
                    }
                    float.TryParse(split[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float min);
                    float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float max);

                    p.onGui = (e, p, g) => Vector2MinMaxProperty(e, p, g, min, max);
                }
                else if (referenceName == "_Mode")
                {
                    p.onGui = (e, p, g) => RenderingModeProperty(e, p, g);
                }
                *//*else if (referenceName == "_OutlineToggle")
                {
                    p.onGui = (e, p, g) => OutlinePassToggle(e, p, g);
                }*/
                /*else if (referenceName == "_GrabpassToggle")
                {
                    p.onGui = (e, p, g) => GrabpassToggle(e, p, g);
                }*//*
                else if (materialProperty.type == MaterialProperty.PropType.Texture)
                {
                    var textureToggleAttribute = attributes.FirstOrDefault(x => x.StartsWith("AutoKeyword("));

                    bool textureToggle = false;
                    string keywordName = "";
                    if (!string.IsNullOrEmpty(textureToggleAttribute))
                    {
                        keywordName = textureToggleAttribute.Split('(')[1].TrimEnd(')');
                        textureToggle = true;
                    }


                    // run on start as well
                    if (textureToggle)
                    {
                        foreach (Material target in editor.targets.Cast<Material>())
                        {
                            ToggleKeyword(target, keywordName, target.GetTexture(materialProperty.name) != null);
                        }
                    }

                    p.onGui = (e, p, g) =>
                    {   
                        if (textureToggle)
                        {
                            EditorGUI.BeginChangeCheck();
                        }
                        TextureProperty(e, p, g);

                        if (textureToggle)
                        {
                            if (EditorGUI.EndChangeCheck())
                            {
                                foreach (Material target in e.targets.Cast<Material>())
                                {
                                    ToggleKeyword(target, keywordName, target.GetTexture(p.name) != null);
                                }
                            }
                        }
                    };
                    if (attributes.Contains("Linear"))
                    {
                        p.onGui += (e, p, g) => LinearWarning(p);
                    }
                    
                    if (flags.HasFlag(MaterialProperty.PropFlags.Normal))
                    {
                        p.onGui += DrawGlNormalWarning;
                    }

                    if (attributes.Contains("TextureToggle"))
                    { 

                    }
                }
                else if (materialProperty.type == MaterialProperty.PropType.Vector)
                {
                    if (attributes.Contains("Vector2"))
                    {
                        p.onGui = Vector2PropertyAction;
                    }
                    else if (attributes.Contains("Vector3"))
                    {
                        p.onGui = Vector3PropertyAction;
                    }
                    else
                    {
                        p.onGui = Vector4PropertyAction;
                    }
                }
                else
                {
                    p.onGui = ShaderPropertyAction;
                }

                current.Add(p);
            }
        }

        static void ShaderPropertyAction(MaterialEditor e, MaterialProperty p, GUIContent g) => e.ShaderProperty(p, g);
        static void Vector2PropertyAction(MaterialEditor e, MaterialProperty p, GUIContent g) => Vector2Property(e, p, g);
        static void Vector3PropertyAction(MaterialEditor e, MaterialProperty p, GUIContent g) => Vector3Property(e, p, g);
        static void Vector4PropertyAction(MaterialEditor e, MaterialProperty p, GUIContent g) => Vector4Property(e, p, g);
        static void DrawGlNormalWarning(MaterialEditor e, MaterialProperty p, GUIContent g) => GlNormalWarning(p);


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
            Material t = editor.target as Material;
            if (t && t.HasProperty("_EmissionColor"))
            {
                bool emission = t.IsKeywordEnabled("_EMISSION");
                editor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, emission);
            }
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
            public Action<MaterialEditor, MaterialProperty, GUIContent> onGui;
            public List<PropertyElement> Children { get; private set; }
            public PropertyElement Parent { get; private set; }
            public GUIContent guiContent;
            public void Add(PropertyElement p)
            {
                Children ??= new List<PropertyElement>();
                p.Parent = this;
                Children.Add(p);
            }

            public void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
            {
                onGui.Invoke(editor, properties[_index], guiContent);

                if (hideChildren && Children is not null)
                {
                    foreach (PropertyElement p in Children)
                    {
                        p.OnGUI(editor, properties);
                    }
                }
            }
        }

        public static void SetupRenderingMode(Material material)
        {
            const string Mode = "_Mode";
            if (!material.HasFloat(Mode))
            {
                return;
            }
            int mode = (int)material.GetFloat(Mode);
            ToggleKeyword(material, "_ALPHATEST_ON", mode == 1);
            ToggleKeyword(material, "_ALPHAFADE_ON", mode == 2 || mode == 4);
            ToggleKeyword(material, "_ALPHAPREMULTIPLY_ON", mode == 3 || mode == 6);
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
                case 6: // transclipping
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_AlphaToMask", 0);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest + 10;
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
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
}*/