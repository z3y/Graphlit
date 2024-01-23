using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZSG
{
    [Serializable]
    public enum PropertyType
    {
        Float = 1,
        Float2 = 2,
        Float3 = 3,
        Float4 = 4,
        Color = 5,
        Intiger = 6,
        Texture2D = 7,
        TextureCube = 8,
    }

    public enum PropertyDeclaration
    {
        Constant,
        Property
    }

    [Serializable]
    public class PropertyDescriptor
    {
        [SerializeField] public string guid;
        [SerializeField] public string referenceName;
        [SerializeField] public string displayName;
        [SerializeField] public PropertyType type;
        [SerializeField] public List<string> attributes;
        [SerializeField] public Vector2 range;
        [SerializeField] string _value;
        [SerializeField] bool tileOffset;
        public float FloatValue
        {
            get
            {
                float.TryParse(_value, out float value);
                return value;
            }
            set
            {
                _value = value.ToString();
            }
        }
        public Vector4 VectorValue
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    return Vector4.zero;
                }

                string withoutParens = _value.Replace(")", "").Replace("(", "");
                string[] split = withoutParens.Split(',');
                float.TryParse(split[0], out float x);
                float.TryParse(split[1], out float y);
                float.TryParse(split[2], out float z);
                float.TryParse(split[3], out float w);
                return new Vector4(x, y, z , w);
            }
            set
            {
                _value = value.ToString();
            }
        }
        public Texture DefaultTexture
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    return null;
                }
                return Helpers.SerializableReferenceToObject<Texture>(_value);
            }
            set
            {
                _value = Helpers.AssetSerializableReference(value);
            }
        }


        public PropertyDescriptor(PropertyType type, string displayName = "", string referenceName = "", List<string> attributes = null)
        {
            guid = Guid.NewGuid().ToString();
            this.type = type;
            this.displayName = string.IsNullOrEmpty(displayName) ? guid : displayName;
            this.attributes = attributes;
            this.referenceName = referenceName;
        }

        public string GetDefaultValue()
        {
            return type switch
            {
                PropertyType.Float => FloatValue.ToString(),
                PropertyType.Float2 => VectorValue.ToString(),
                PropertyType.Float3 => VectorValue.ToString(),
                PropertyType.Float4 => VectorValue.ToString(),
                PropertyType.Color => VectorValue.ToString(),
                PropertyType.Intiger => FloatValue.ToString(),
                PropertyType.Texture2D => "\"white\" {}",
                PropertyType.TextureCube => "\"white\" {}",
                _ => throw new System.NotImplementedException(),
            };
        }

/*
        public void SetPreviewName(SerializableNode serializableNode)
        {
            referenceName = "_" + serializableNode.guid;
        }*/

        public string TypeToString()
        {
            return type switch
            {
                PropertyType.Float => "Float",
                PropertyType.Float2 => "Vector",
                PropertyType.Float3 => "Vector",
                PropertyType.Float4 => "Vector",
                //PropertyType.Range => $"Range ({range.x.ToString("R")}, {range.y.ToString("R")})",
                PropertyType.Color => "Color",
                PropertyType.Intiger => "Intiger",
                PropertyType.Texture2D => "2D",
                PropertyType.TextureCube => "Cube",
                _ => throw new System.NotImplementedException()
            };
        }

        public string GetFieldDeclaration(GenerationMode generationMode)
        {
            var referenceName = GetReferenceName(generationMode);

            return type switch
            {
                PropertyType.Float => $"float {referenceName};",
                PropertyType.Float2 => $"float2 {referenceName};",
                PropertyType.Float3 => $"float3 {referenceName};",
                PropertyType.Float4 => $"float4 {referenceName};",
                PropertyType.Color => $"float4 {referenceName};",
                PropertyType.Intiger => $"int {referenceName};",
                PropertyType.Texture2D => $"Texture2D {referenceName}; SamplerState sampler{referenceName};",
                PropertyType.TextureCube => $"TextureCube {referenceName}; SamplerState sampler{referenceName};",
                _ => throw new System.NotImplementedException()
            };
        }

        public string AttributesToString()
        {
            if (attributes is null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var attribute in attributes)
            {
                sb.Append("[");
                sb.Append(attribute.ToString());
                sb.Append("] ");
            }
            return sb.ToString();
        }

        public string GetReferenceName(GenerationMode generationMode)
        {
            if (generationMode == GenerationMode.Preview)
            {
                return "_" + guid.RemoveWhitespace().Replace("-", "_");
            }
            if (!string.IsNullOrEmpty(referenceName))
            {
                return referenceName;
            }

            return "_" + displayName.RemoveWhitespace().Replace("-", "_");
        }

        public string GetPropertyDeclaration(GenerationMode generationMode)
        {
            var referenceName = GetReferenceName(generationMode);
            var type = TypeToString();
            var attributes = AttributesToString();
            var defaultValue = GetDefaultValue();

            return $"{attributes} {referenceName} (\"{displayName}\", {type}) = {defaultValue}";
        }

        void OnDefaultGUI()
        {
            EditorGUILayout.LabelField(guid);
            EditorGUILayout.LabelField(type.ToString());

            displayName = EditorGUILayout.TextField(new GUIContent("Display Name"), displayName);
            referenceName = EditorGUILayout.TextField(new GUIContent("Reference Name"), referenceName);
        }

        void OnGUIFloat()
        {
            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUILayout.FloatField("Default Value", FloatValue);
            if (EditorGUI.EndChangeCheck())
            {
                FloatValue = newValue;
                graphView.PreviewMaterial.SetFloat(GetReferenceName(GenerationMode.Preview), newValue);
            }
        }

        [NonSerialized] public ShaderGraphView graphView;

        public VisualElement PropertyEditorGUI()
        {
            var imgui = new IMGUIContainer(OnDefaultGUI); // too much data to bind, easier to just use imgui
            //imgui.onGUIHandler += OnDefaultGUI;
            if (type == PropertyType.Float) imgui.onGUIHandler += OnGUIFloat;

            return imgui;
        }
    }
}