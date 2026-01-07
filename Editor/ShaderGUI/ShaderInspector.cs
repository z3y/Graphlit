using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Graphlit
{
    public class ShaderInspector : ShaderGUI
    {
        public enum PropertyElementType
        {
            Float,
            MinMax,
            Vector2,
            Vector3,
            Vector4,
            Texture,
            Color,
        }
        public class PropertyElement
        {
            public GUIContent guiContent;
            public string referenceName;
            public int index;
            public PropertyElementType type;
            public Action<Material, PropertyElement> onValueChange;
            public Vector2 minMax;
            public Dictionary<int, float> showIf;
            public string textureToggleKeyword;
            public int indent;
            public string folder;
            public bool linearWarning;
            public int extraProperty = -1;
            public bool helpBox;
            public bool maskMapPack;
        }

        public class PropertyFolder
        {
            public string name;

            public PropertyFolder(string name)
            {
                this.name = name;
            }

            public List<PropertyElement> elements = new List<PropertyElement>();

            public void Add(PropertyElement element)
            {
                elements.Add(element);
            }
        }

        bool _start = true;
        Material _material;
        Shader _shader;
        List<PropertyFolder> _folders;
        static bool _reinitialize = false;
        int _queuePropertyIndex = 0;
        public static void Reinitialize() => _reinitialize = true;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
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

            var material = (Material)materialEditor.target;
            if (_shader != material.shader)
            {
                _reinitialize = true;
            }

            if (_start || _reinitialize)
            {
                OnGUIStart(materialEditor, properties);
                _start = false;
                _reinitialize = false;
            }


            DrawProperties(materialEditor, properties);

            Footer(materialEditor, properties);
            Links();

            for (int i = 0; i < style.Length; i++)
            {
                style[i].richText = richTextState[i];
            }

            if (material.GetFloat("__reset") > 0)
            {
                material.SetFloat("__reset", 0);
                OnReset(material);
            }
        }

        void DrawProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            int baseIndentation = EditorGUI.indentLevel;

            foreach (var folder in _folders)
            {
                EditorGUI.indentLevel = baseIndentation;
                CoreEditorUtils.DrawSplitter();
                const string folderPrefix = "folder_";
                string defaultValue = folder.name == "Surface Inputs" ? "1" : "0";
                bool folderVisible = _material.GetTag(folderPrefix + folder.name, false, defaultValue) == "1";
                bool folderVisibleChange = CoreEditorUtils.DrawHeaderFoldout(folder.name, folderVisible);
                if (folderVisible != folderVisibleChange)
                {
                    _material.SetOverrideTag(folderPrefix + folder.name, folderVisibleChange ? "1" : "0");
                }

                if (!folderVisible)
                {
                    continue;
                }

                foreach (var element in folder.elements)
                {
                    var materialProperty = properties[element.index];

                    if (element.showIf is not null)
                    {
                        bool visible = true;
                        foreach (var keyPair in element.showIf)
                        {
                            var targetProp = properties[keyPair.Key];

                            if ((targetProp.type == MaterialProperty.PropType.Float ||
                             targetProp.type == MaterialProperty.PropType.Range)
                             && targetProp.floatValue != keyPair.Value)
                            {
                                visible = false;
                            }
                            else if (targetProp.type == MaterialProperty.PropType.Int && targetProp.intValue != keyPair.Value)
                            {
                                visible = false;
                            }
                            else if (targetProp.type == MaterialProperty.PropType.Texture && targetProp.textureValue != (keyPair.Value > 0))
                            {
                                visible = false;
                            }
                        }

                        if (!visible)
                        {
                            continue;
                        }
                    }

                    EditorGUI.indentLevel = baseIndentation + element.indent;

                    if (element.helpBox)
                    {
                        EditorGUILayout.HelpBox(element.guiContent.text, MessageType.Info);
                        continue;
                    }

                    bool hasOnValueChange = element.onValueChange is not null;

                    if (hasOnValueChange)
                    {
                        EditorGUI.BeginChangeCheck();
                    }

                    switch (element.type)
                    {
                        case PropertyElementType.Float or PropertyElementType.Color:
                            materialEditor.ShaderProperty(materialProperty, element.guiContent);
                            break;
                        case PropertyElementType.Vector2:
                            Vector2Property(materialEditor, materialProperty, element.guiContent);
                            break;
                        case PropertyElementType.Vector3:
                            Vector3Property(materialEditor, materialProperty, element.guiContent);
                            break;
                        case PropertyElementType.Vector4:
                            Vector4Property(materialEditor, materialProperty, element.guiContent);
                            break;
                        case PropertyElementType.Texture:
                            //TextureProperty(materialEditor, materialProperty, element.guiContent);
                            MaterialProperty extraProperty = null;
                            materialEditor.TexturePropertySingleLine(element.guiContent, materialProperty);
                            if (element.extraProperty >= 0)
                            {
                                extraProperty = properties[element.extraProperty];
                                TexturePropertySingleLineExtraProp(materialEditor, element.guiContent, extraProperty);
                            }
                            if (!materialProperty.flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset))
                            {
                                materialEditor.TextureScaleOffsetProperty(materialProperty);
                            }
                            if (element.maskMapPack) MaskMapPackingButton(materialEditor, materialProperty);
                            if (element.linearWarning) LinearWarning(materialProperty);
                            if (materialProperty.flags.HasFlag(MaterialProperty.PropFlags.Normal)) GlNormalWarning(materialProperty);

                            break;
                        case PropertyElementType.MinMax:
                            Vector2MinMaxProperty(materialEditor, materialProperty, element.guiContent, element.minMax.x, element.minMax.y);
                            break;
                    }

                    if (hasOnValueChange && EditorGUI.EndChangeCheck())
                    {
                        element.onValueChange.Invoke(_material, element);
                    }
                }
            }
        }

        void OnGUIStart(MaterialEditor editor, MaterialProperty[] properties)
        {
            _material = (Material)editor.target;
            _shader = _material.shader;

            PraseProperties(editor, properties);
        }


        readonly string[] _renderingFolderProps =
        {
            "_Surface",
            "_Blend",
            "_BlendModePreserveSpecular",
            "_AlphaClip",
            "_AlphaToMask",
            "_ZWrite",
            "_Cull",
            "_SrcBlend",
            "_DstBlend",
            "_SrcBlendAlpha",
            "_DstBlendAlpha",
            "_Cutoff",
            "_TransClipping",
            "_ZTest"
        };

        void PraseProperties(MaterialEditor editor, MaterialProperty[] properties)
        {
            _queuePropertyIndex = Array.FindIndex(properties, x => x.name == "_QueueOffset");

            _folders = new List<PropertyFolder>();

            var renderingFolder = new PropertyFolder("Rendering Options");
            _folders.Add(renderingFolder);

            var advancedFolder = new PropertyFolder("Advanced Options");
            _folders.Add(advancedFolder);

            var mainFolder = new PropertyFolder("Surface Inputs");

            _folders.Add(mainFolder);

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                {
                    continue;
                }

                var element = ParseElement(editor, properties, i);

                if (_renderingFolderProps.Contains(element.referenceName))
                {
                    renderingFolder.Add(element);
                }
                else
                {
                    if (string.IsNullOrEmpty(element.folder))
                    {
                        mainFolder.Add(element);
                    }
                    else
                    {
                        var folder = _folders.Find(x => x.name == element.folder);
                        if (folder is null)
                        {
                            folder = new PropertyFolder(element.folder);
                            _folders.Add(folder);
                        }
                        folder.Add(element);
                    }
                }


                // move cutoff below alpha clip because its nicer
                if (element.referenceName == "_AlphaClip")
                {
                    int cutoffIndex = Array.FindIndex(properties, x => x.name == "_Cutoff");
                    if (cutoffIndex >= 0)
                    {
                        var cutoffElement = ParseElement(editor, properties, cutoffIndex);
                        cutoffElement.showIf = new()
                        {
                            { element.index, 1.0f }
                        };
                        cutoffElement.indent = 1;
                        renderingFolder.Add(cutoffElement);

                    }
                }

            }

            _folders.Remove(advancedFolder);
            _folders.Add(advancedFolder);
        }

        PropertyElement ParseElement(MaterialEditor editor, MaterialProperty[] properties, int i)
        {
            var property = properties[i];
            var element = new PropertyElement();
            var attributes = _shader.GetPropertyAttributes(i).ToList();
            string tooltip = TryParseStringParam(attributes, "Tooltip");

            element.guiContent = new GUIContent(property.displayName, tooltip);
            element.referenceName = property.name;
            element.index = i;
            element.type = property.type switch
            {
                MaterialProperty.PropType.Texture => PropertyElementType.Texture,
                MaterialProperty.PropType.Vector => PropertyElementType.Vector4,
                MaterialProperty.PropType.Color => PropertyElementType.Color,
                _ => PropertyElementType.Float,
            };

            if (property.type == MaterialProperty.PropType.Vector)
            {
                if (attributes.Contains("Vector2"))
                {
                    element.type = PropertyElementType.Vector2;
                }
                else if (attributes.Contains("Vector3"))
                {
                    element.type = PropertyElementType.Vector3;
                }
            }

            if (TryParseMinMax(attributes, out float min, out float max))
            {
                element.type = PropertyElementType.MinMax;
                element.minMax = new Vector2(min, max);
            }

            while (TryParseShowIf(attributes, out string showIfProperty, out float showIfValue))
            {
                element.showIf ??= new();
                element.showIf[Array.FindIndex(properties, x => x.name == showIfProperty)] = showIfValue;
                //Debug.Log($"{element.referenceName}: {showIfProperty}, {showIfValue}");
            }

            element.folder = TryParseStringParam(attributes, "Folder");

            element.helpBox = attributes.Contains("HelpBox");

            element.maskMapPack = attributes.Contains("PackMaskMap");


            string intent = TryParseStringParam(attributes, "Indent");
            if (!string.IsNullOrEmpty(intent))
            {
                element.indent = int.Parse(intent);
            }

            if (element.type == PropertyElementType.Texture)
            {
                if (attributes.Contains("Linear"))
                {
                    element.linearWarning = true;
                }
                string keyword = TryParseStringParam(attributes, "Toggle");
                if (!string.IsNullOrEmpty(keyword))
                {
                    element.textureToggleKeyword = keyword;
                    element.onValueChange = static (m, e) =>
                    {
                        ToggleKeyword(m, e.textureToggleKeyword, m.GetTexture(e.referenceName));
                    };

                    foreach (Material mat in editor.targets.Cast<Material>())
                    {
                        element.onValueChange.Invoke(mat, element);
                    }
                }

                string extraProperty = TryParseStringParam(attributes, "ExtraProperty");
                if (!string.IsNullOrEmpty(extraProperty))
                {
                    element.extraProperty = Array.FindIndex(properties, x => x.name == extraProperty);
                }
            }

            if (property.name == "_Surface" || property.name == "_Blend" ||
                property.name == "_BlendModePreserveSpecular" || property.name == "_AlphaToMask" ||
                property.name == "_AlphaClip" || property.name == "_TransClipping")
            {
                element.onValueChange = static (m, e) =>
                {
                    SetupSurfaceType(m);
                };
            }

            return element;
        }

        static string TryParseStringParam(List<string> attributes, string prefix)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                string input = attributes[i];
                var match = Regex.Match(input, prefix + @"\s*\(\s*(.*?)\s*\)");

                if (match.Success)
                {
                    string content = match.Groups[1].Value;
                    attributes.RemoveAt(i);
                    return content;
                }
            }

            return string.Empty;
        }
        static bool TryParseMinMax(List<string> attributes, out float min, out float max)
        {
            min = 0f;
            max = 0f;

            for (int i = 0; i < attributes.Count; i++)
            {
                string input = attributes[i];
                var match = Regex.Match(input, @"MinMax\s*\(\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*\)");

                if (!match.Success)
                    return false;

                if (float.TryParse(match.Groups[1].Value, out min) &&
                    float.TryParse(match.Groups[2].Value, out max))
                {
                    attributes.RemoveAt(i);
                    return true;
                }

            }

            return false;
        }

        static bool TryParseShowIf(List<string> attributes, out string propertyName, out float value)
        {
            propertyName = null;
            value = 0f;

            for (int i = 0; i < attributes.Count; i++)
            {
                string input = attributes[i];


                var match = Regex.Match(input, @"ShowIf\s*\(\s*([a-zA-Z_][\w]*)\s*,\s*(-?\d+(?:\.\d+)?)\s*\)");

                if (!match.Success)
                    return false;

                propertyName = match.Groups[1].Value;

                bool success = float.TryParse(match.Groups[2].Value, out value);
                if (success)
                {
                    attributes.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        void OnReset(Material material)
        {
            _reinitialize = true;
            material.shaderKeywords = null;
            SetupSurfaceType(material);
        }

        public void Footer(MaterialEditor editor, MaterialProperty[] properties)
        {
            CoreEditorUtils.DrawSplitter();
            EditorGUILayout.Space();
            Material t = editor.target as Material;

            if (t && t.HasProperty("_EmissionColor"))
            {
                bool emission = t.IsKeywordEnabled("_EMISSION");
                editor.LightmapEmissionFlagsProperty(0, emission);
            }
            editor.RenderQueueField();

            /*EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(properties[_queuePropertyIndex], "Queue Offset");
            if (EditorGUI.EndChangeCheck())
            {

            }*/
            editor.EnableInstancingField();
            editor.DoubleSidedGIField();
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            material.shaderKeywords = null;

            if (oldShader && newShader && oldShader.name == "Lit" && newShader.name == "Graphlit/Lit")
            {
                Debug.Log("Converting lit material");

                ConvertLitMaterial(material, (m) => base.AssignNewShaderToMaterial(m, oldShader, newShader));
            }
            else if (oldShader.FindPropertyIndex("_Mode") >= 0)
            {
                int previousMode = (int)material.GetFloat("_Mode");
                base.AssignNewShaderToMaterial(material, oldShader, newShader);
                UpgradeMode(material, true, false, previousMode);
            }
            else
            {
                base.AssignNewShaderToMaterial(material, oldShader, newShader);
            }

            SetupSurfaceType(material);
        }

        public static void UpgradeMode(Material material, bool preserveQueue, bool log, int previousMode = -1)
        {
            int mode = previousMode >= 0 ? previousMode : (int)material.GetFloat("_Mode");
            if (mode == 0)
            {
                return;
            }

            if (log)
                Debug.Log($"Upgrading Surface Mode {mode} for {material.name} material");

            if (mode > 0 && mode != 1)
            {
                material.SetFloat("_Surface", 1.0f);
                if (mode == 3) material.SetFloat("_BlendModePreserveSpecular", 1.0f);
                else material.SetFloat("_BlendModePreserveSpecular", 0.0f);
            }
            if (mode == 1)
            {
                material.SetFloat("_AlphaClip", 1.0f);
            }

            if (mode == 6)
            {
                material.SetFloat("_TransClipping", 1.0f);
                material.SetFloat("_BlendModePreserveSpecular", 1.0f);
            }

            if (preserveQueue)
            {
                var queue = material.renderQueue;
                SetupSurfaceType(material);
                material.renderQueue = queue;
            }
            else
            {
                SetupSurfaceType(material);
            }

        }

        [MenuItem("Tools/Graphlit/Upgrade Materials")]
        public static void UpgradeAllMaterials()
        {
            var materials = AssetDatabase.FindAssets("t:material")
                .Select(x => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(x)))
                .OfType<Material>();

            foreach (var material in materials)
            {
                bool isGraplitMaterial = material.HasFloat("_GraphlitMaterial");

                if (!isGraplitMaterial)
                {
                    continue;
                }

                UpgradeMode(material, true, true);
            }
        }

        public static void SetupSurfaceType(Material material)
        {
            if (!material.HasFloat("_Surface"))
            {
                return;
            }

            int surfaceType = (int)material.GetFloat("_Surface");
            int surfaceBlend = (int)material.GetFloat("_Blend");
            bool alphaClip = material.GetFloat("_AlphaClip") > 0;
            bool transclipping = material.GetFloat("_TransClipping") > 0;

            bool preserveSpecular = material.HasFloat("_BlendModePreserveSpecular") ?
                material.GetFloat("_BlendModePreserveSpecular") > 0 : false;

            if (transclipping)
            {
                material.SetFloat("_BlendModePreserveSpecular", 1);
                preserveSpecular = true;
            }

            ToggleKeyword(material, "_SURFACE_TYPE_TRANSPARENT", surfaceType > 0);
            ToggleKeyword(material, "_ALPHATEST_ON", alphaClip);
            material.SetFloat("_AlphaToMask", alphaClip ? 1 : 0);

            if (surfaceType > 0)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                material.SetInt("_ZWrite", 0);

                if (surfaceBlend == 0)
                {
                    material.SetInt("_SrcBlend", preserveSpecular ?
                        (int)UnityEngine.Rendering.BlendMode.One :
                        (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }
                else if (surfaceBlend == 1)
                {
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }
                else if (surfaceBlend == 2)
                {
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                }
                else if (surfaceBlend == 3)
                {
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                }
            }

            if (alphaClip && surfaceType == 0)
            {
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                material.SetInt("_ZWrite", 1);

                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            }

            if (surfaceType > 0 && transclipping)
            {
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest + 10;
                material.SetInt("_ZWrite", 1);
            }

            if (!alphaClip && surfaceType == 0)
            {
                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                material.SetInt("_ZWrite", 1);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            }

            // material.renderQueue += (int)material.GetFloat("_QueueOffset");
        }

        public static void ToggleKeyword(Material material, string keyword, bool value)
        {
            if (value)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        public static bool TextureImportWarningBox(string message)
        {
            // Mimics the normal map import warning - written by Orels1
            GUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
            EditorGUILayout.LabelField(message, new GUIStyle(EditorStyles.label) { fontSize = 11, wordWrap = true });
            EditorGUILayout.BeginHorizontal(new GUIStyle() { alignment = TextAnchor.MiddleRight }, GUILayout.Height(24));
            EditorGUILayout.Space();
            var buttonPress = GUILayout.Button("Fix Now", new GUIStyle("button")
            {
                stretchWidth = false,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 8, 0, 0)
            }, GUILayout.Height(22));
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return buttonPress;
        }
        public static void LinearWarning(MaterialProperty tex)
        {
            if (!tex?.textureValue) return;
            var texPath = AssetDatabase.GetAssetPath(tex.textureValue);
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer == null) return;
            const string text = "This texture is marked as sRGB, but should be linear.";
            if (!importer.sRGBTexture || !TextureImportWarningBox(text)) return;
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }
        public static void GlNormalWarning(MaterialProperty tex)
        {
            if (!tex?.textureValue) return;
            if (!tex.textureValue.name.ToLower().EndsWith("dx"))
            {
                return;
            }

            var texPath = AssetDatabase.GetAssetPath(tex.textureValue);
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                return;
            }
            const string text = "DX Normal Map should be converted to GL.";
            if (importer.flipGreenChannel || !TextureImportWarningBox(text)) return;
            importer.flipGreenChannel = true;
            importer.SaveAndReimport();
        }
        public static void TextureProperty(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            float fieldWidth = EditorGUIUtility.fieldWidth;
            float labelWidth = EditorGUIUtility.labelWidth;
            editor.SetDefaultGUIWidths();
            var rect = GetControlRect(property);
            editor.TextureProperty(rect, property, label);
            EditorGUIUtility.fieldWidth = fieldWidth;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public static void Vector4Property(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            MaterialPropertyInternal(property, (rect) =>
            {
                EditorGUI.BeginChangeCheck();
                Vector4 vectorValue = EditorGUI.Vector4Field(rect, label, property.vectorValue);
                if (EditorGUI.EndChangeCheck()) property.vectorValue = vectorValue;
            });
        }
        public static void Vector3Property(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            MaterialPropertyInternal(property, (rect) =>
            {
                EditorGUI.BeginChangeCheck();
                Vector3 vectorValue = EditorGUI.Vector3Field(rect, label, property.vectorValue);
                if (EditorGUI.EndChangeCheck()) property.vectorValue = vectorValue;
            });
        }
        public static void Vector2Property(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            MaterialPropertyInternal(property, (rect) =>
            {
                rect.width *= 1.3f;
                EditorGUI.BeginChangeCheck();
                Vector2 vectorValue = EditorGUI.Vector2Field(rect, label, property.vectorValue);
                if (EditorGUI.EndChangeCheck()) property.vectorValue = vectorValue;
            });
        }
        public static void Vector2MinMaxProperty(MaterialEditor editor, MaterialProperty property, GUIContent label, float min, float max)
        {
            MaterialPropertyInternal(property, (rect) =>
            {
                EditorGUI.BeginChangeCheck();
                float x = property.vectorValue.x;
                float y = property.vectorValue.y;
                EditorGUI.MinMaxSlider(rect, label, ref x, ref y, min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    property.vectorValue = new Vector2(x, y);
                }
            });
        }
        public static Rect GetControlRect(MaterialProperty property)
        {
            float height = property.type switch
            {
                MaterialProperty.PropType.Texture => 70f,
                _ => 18f
            };
            return EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
        }
        public static void MaterialPropertyInternal(MaterialProperty property, Action<Rect> onGui)
        {
            Rect rect = GetControlRect(property);
            MaterialEditor.BeginProperty(rect, property);
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;
            onGui.Invoke(rect);
            EditorGUIUtility.labelWidth = labelWidth;
            MaterialEditor.EndProperty();
        }

        public void OutlinePassToggle(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(property, label);
            foreach (var mat in editor.targets.Cast<Material>())
            {
                mat.SetShaderPassEnabled("ALWAYS", property.floatValue > 0);
            }
        }

        public void GrabpassToggle(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(property, label);
            foreach (var mat in editor.targets.Cast<Material>())
            {
                mat.SetShaderPassEnabled("GrabPass", property.floatValue > 0);
            }
        }

        private void ExtraPropertyAfterTexture(MaterialEditor materialEditor, Rect r, MaterialProperty property)
        {
            if ((property.type == MaterialProperty.PropType.Float || property.type == MaterialProperty.PropType.Color) && r.width > EditorGUIUtility.fieldWidth)
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = r.width - EditorGUIUtility.fieldWidth - 2f;

                var labelRect = new Rect(r.x, r.y, r.width - EditorGUIUtility.fieldWidth - 4f, r.height);
                var style = new GUIStyle("label");
                style.alignment = TextAnchor.MiddleRight;
                EditorGUI.LabelField(labelRect, property.displayName, style);
                materialEditor.ShaderProperty(r, property, " ");
                EditorGUIUtility.labelWidth = labelWidth;
            }
            else
            {
                materialEditor.ShaderProperty(r, property, string.Empty);
            }
        }

        public void TexturePropertySingleLineExtraProp(MaterialEditor editor, GUIContent label, MaterialProperty extraProperty1, MaterialProperty extraProperty2 = null)
        {
            Rect controlRectForSingleLine = GUILayoutUtility.GetLastRect();

            if (controlRectForSingleLine.height > 20)
            {
                // fix offset when there is a normal map fix button
                var pos = controlRectForSingleLine.position;
                controlRectForSingleLine.position = new Vector2(pos.x, pos.y - 42);
            }

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            if (extraProperty1 == null || extraProperty2 == null)
            {
                MaterialProperty materialProperty = extraProperty1 ?? extraProperty2;
                if (materialProperty.type == MaterialProperty.PropType.Color)
                {
                    ExtraPropertyAfterTexture(editor, MaterialEditor.GetLeftAlignedFieldRect(controlRectForSingleLine), materialProperty);
                }
                else if (materialProperty.type == MaterialProperty.PropType.Range)
                {
                    var r = MaterialEditor.GetRectAfterLabelWidth(controlRectForSingleLine);
                    //r.width -= 50;
                    r.position = new Vector2(r.x, r.y);
                    ExtraPropertyAfterTexture(editor, r, materialProperty);
                }
                else
                {
                    var r = MaterialEditor.GetRectAfterLabelWidth(controlRectForSingleLine);
                    r.width -= 50;
                    r.position = new Vector2(r.x + 50, r.y);
                    ExtraPropertyAfterTexture(editor, r, materialProperty);
                }
            }
            else if (extraProperty1.type == MaterialProperty.PropType.Color)
            {
                ExtraPropertyAfterTexture(editor, MaterialEditor.GetFlexibleRectBetweenFieldAndRightEdge(controlRectForSingleLine), extraProperty2);
                ExtraPropertyAfterTexture(editor, MaterialEditor.GetLeftAlignedFieldRect(controlRectForSingleLine), extraProperty1);
            }
            else
            {
                ExtraPropertyAfterTexture(editor, MaterialEditor.GetRightAlignedFieldRect(controlRectForSingleLine), extraProperty2);
                ExtraPropertyAfterTexture(editor, MaterialEditor.GetFlexibleRectBetweenLabelAndField(controlRectForSingleLine), extraProperty1);
            }

            EditorGUI.indentLevel = indentLevel;
        }

        public static void ConvertLitMaterial(Material m, Action<Material> onAssignShader)
        {
            int previousMode = (int)m.GetFloat("_Mode");
            Texture detailAlbedo = m.GetTexture("_DetailAlbedo");
            Vector2 detailAlbedoScale = m.GetTextureScale("_DetailAlbedo");
            Vector2 detailAlbedoOffset = m.GetTextureOffset("_DetailAlbedo");
            float detailNormalMapScale = m.GetFloat("_DetailBumpScale");

            Texture detailNormalMap = m.GetTexture("_DetailBumpMap");
            float uvSec = m.GetFloat("_Detail_UV");

            float toggleEmission = m.GetFloat("_EmissionToggle");

            float roughness = m.GetFloat("_Roughness");
            float metallic = m.GetFloat("_Metallic");

            float toggleWind = m.GetFloat("_WindToggle");

            float lightmappedSpecular = m.GetFloat("_LIGHTMAPPED_SPECULAR");
            float monoSH = m.GetFloat("_BAKERY_MONOSH");
            float bicubicLightmap = m.GetFloat("_BICUBIC_LIGHTMAP");

            onAssignShader(m);

            m.SetTexture("_DetailAlbedoMap", detailAlbedo);
            m.SetTextureScale("_DetailAlbedoMap", detailAlbedoScale);
            m.SetTextureOffset("_DetailAlbedoMap", detailAlbedoOffset);
            m.SetFloat("_DetailNormalMapScale", detailNormalMapScale);

            m.SetTexture("_DetailNormalMap", detailNormalMap);
            m.SetFloat("_UVSec", uvSec);

            m.SetFloat("_Toggle_EMISSION", toggleEmission);

            m.SetFloat("_RoughnessScale", roughness);

            m.SetFloat("_MetallicScale", metallic);
            m.SetFloat("_Toggle_WIND", toggleWind);

            m.SetFloat("_LightmappedSpecular", lightmappedSpecular);
            m.SetFloat("_MonoSH", monoSH);
            m.SetFloat("_BicubicLightmap", bicubicLightmap);

            UpgradeMode(m, true, false, previousMode);

            MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        void MaskMapPackingButton(MaterialEditor editor, MaterialProperty property)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (!property.flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset))
            {
                rect.y -= 22;
            }
            rect = MaterialEditor.GetRectAfterLabelWidth(rect);
            rect.width = 50;
            //rect.position = new Vector2(Screen.width / 2, rect.position.y);
            if (editor.targets.Length == 1)
            {
                if (GUI.Button(rect, "Pack"))
                {
                    FreeImagePackingEditor.Init();
                    FreeImagePackingEditor.ChannelR.Channel.DefaultColor = FreeImagePacking.DefaultColor.White;
                    FreeImagePackingEditor.ChannelG.Channel.DefaultColor = FreeImagePacking.DefaultColor.White;
                    FreeImagePackingEditor.ChannelB.Channel.DefaultColor = FreeImagePacking.DefaultColor.Black;
                    FreeImagePackingEditor.ChannelA.Channel.DefaultColor = FreeImagePacking.DefaultColor.Black;

                    FreeImagePackingEditor.ChannelR.DisplayName = "Ambient Occlusion";
                    FreeImagePackingEditor.ChannelG.DisplayName = "Roughness";
                    FreeImagePackingEditor.ChannelG.InvertDisplayName = "Smoothness";
                    FreeImagePackingEditor.ChannelB.DisplayName = "Metallic";
                    FreeImagePackingEditor.ChannelA.DisplayName = "Detail Mask";
                    FreeImagePackingEditor.packingSuffix = "_MaskMap";
                    FreeImagePackingEditor.AddPackingMaterial((Material)editor.target, property);
                }
            }
        }

        static Texture2D _githubIcon;
        static Texture2D _patreonIcon;
        static string _versionText = string.Empty;
        [System.Serializable]
        public struct DummyPackage
        {
            public string version;
        }
        void Links()
        {
            EditorGUILayout.Space(20);

            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight, richText = true };

            if (!_githubIcon)
            {
                _githubIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.z3y.graphlit/Editor/Icons/github-mark-white.png");
            }
            if (!_patreonIcon)
            {
                _patreonIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.z3y.graphlit/Editor/Icons/patreon-white.png");
            }
            if (string.IsNullOrEmpty(_versionText))
            {
                var json = AssetDatabase.LoadAssetAtPath<PackageManifest>("Packages/com.z3y.graphlit/package.json").text;
                _versionText = JsonUtility.FromJson<DummyPackage>(json).version;
            }
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("<b>Graphlit</b>", style, GUILayout.Width(50));
            EditorGUILayout.LabelField(_versionText, style, GUILayout.Width(50));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(1);

            if (GUILayout.Button(new GUIContent(_githubIcon, "github.com/z3y/Graphlit"), GUILayout.Width(36), GUILayout.Height(36)))
            {
                Application.OpenURL("https://github.com/z3y/Graphlit");
            }
            EditorGUILayout.Space(1);
            if (GUILayout.Button(new GUIContent(_patreonIcon, "patreon.com/z3y"), GUILayout.Width(36), GUILayout.Height(36)))
            {
                Application.OpenURL("https://www.patreon.com/z3y");
            }
            EditorGUILayout.Space(35);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}