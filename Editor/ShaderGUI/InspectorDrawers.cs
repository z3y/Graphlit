using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZSG
{
    public partial class DefaultInspector
    {
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
        public void RenderingModeProperty(MaterialEditor editor, MaterialProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(property, label);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material target in editor.targets.Cast<Material>())
                {
                    SetupRenderingMode(target);
                }
            }
        }


    }

}