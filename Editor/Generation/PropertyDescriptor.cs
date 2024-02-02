using System;
using System.Collections.Generic;
using System.Linq;
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
        Texture2DArray = 8,
        TextureCube = 9,
        Texture3D = 10,
        TextureCubeArray = 11,
        Bool = 12,
        KeywordToggle = 13
    }

    [Serializable]
    public enum PropertyDeclaration
    {
        Local = 0,
        Global = 1,
        Instance = 2,
    }

    static class PropertyAttributes
    {
        public static bool Get(List<string> attributes, string name)
        {
            return attributes.Contains(name);
        }

        public static void Set(List<string> attributes, bool value, string name)
        {
            bool contains = attributes.Contains(name);
            if (value && !contains)
            {
                attributes.Add(name);
            }
            else if (!value && contains)
            {
                attributes.Remove(name);
            }
        }
    }

    // thanks pema
    public enum DefaultTextureName
    {
        white,
        black,
        red,
        gray,
        grey,
        linearGray,
        linearGrey,
        grayscaleRamp,
        greyscaleRamp,
        bump,
        blackCube,
        lightmap,
        unity_Lightmap,
        unity_LightmapInd,
        unity_ShadowMask,
        unity_DynamicLightmap,
        unity_DynamicDirectionality,
        unity_DynamicNormal,
        unity_DitherMask,
        _DitherMaskLOD,
        _DitherMaskLOD2D,
        unity_RandomRotation16,
        unity_NHxRoughness,
        unity_SpecCube0,
        unity_SpecCube1,
        none
    }

    [Serializable]
    public class PropertyDescriptor
    {
        [SerializeField] public string guid;
        [SerializeField] public string referenceName;
        [SerializeField] public string displayName;
        [SerializeField] public PropertyType type;
        [SerializeField] public List<string> attributes = new();
        [SerializeField] public float rangeX;
        [SerializeField] public float rangeY;
        [SerializeField] string _value;
        [SerializeField] string _defaultTexture;
        [SerializeField] public PropertyDeclaration declaration = PropertyDeclaration.Local;

        [NonSerialized] public bool useReferenceName = false;
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
        public Texture DefaultTextureValue
        {
            get
            {
                if (string.IsNullOrEmpty(_defaultTexture))
                {
                    return null;
                }
                return Helpers.SerializableReferenceToObject<Texture>(_defaultTexture);
            }
            set
            {
                _defaultTexture = Helpers.AssetSerializableReference(value);
            }
        }
        public Vector2 Range
        {
            get
            {
                return new(rangeX, rangeY);
            }
            set
            {
                rangeX = value.x;
                rangeY = value.y;
            }
        }
        public DefaultTextureName DefaultTextureEnum
        {
            get
            {
                Enum.TryParse(_value, out DefaultTextureName value);
                return value;
            }
            set
            {
                _value = value.ToString();
            }
        }

        public string KeywordName
        {
            get
            {
                string[] split = referenceName.Split(' ');
                if (split.Length < 2)
                {
                    Debug.LogError("Wrong keyword declaration");
                    return "_KEYWORD";
                }
                return split[1];
            }
        }

        public bool IsTextureType => type == PropertyType.Texture2D || type == PropertyType.Texture2DArray || type == PropertyType.TextureCube || type == PropertyType.Texture3D;

        public bool HasRange => rangeX != rangeY;

        public PropertyDescriptor(PropertyType type, string displayName = null, string referenceName = "")
        {
            guid = Guid.NewGuid().ToString();
            this.type = type;
            this.displayName = string.IsNullOrEmpty(displayName) ? guid : displayName;
            this.referenceName = referenceName;
            if (type == PropertyType.Color)
            {
                VectorValue = Vector4.one;
            }
            if (type == PropertyType.KeywordToggle)
            {
                this.referenceName = "shader_feature_local _KEYWORD";
            }
        }

        const string NormalAttribute = "Normal";
        const string HdrAttribute = "HDR";
        const string HideInInspectorAttribute = "HideInInspector";
        const string NoScaleOffsetAttribute = "NoScaleOffset";
        const string NonModifiableTextureAttribute = "NonModifiableTextureData";

        public string GetDefaultValue()
        {
            if (IsTextureType)
            {
                if (DefaultTextureEnum == DefaultTextureName.none)
                {
                    return "\"\" {}";
                }
                return '"' + DefaultTextureEnum.ToString() + '"' + " {}";
            }
            return type switch
            {
                PropertyType.Float => FloatValue.ToString(),
                PropertyType.Float2 => VectorValue.ToString(),
                PropertyType.Float3 => VectorValue.ToString(),
                PropertyType.Float4 => VectorValue.ToString(),
                PropertyType.Color => VectorValue.ToString(),
                PropertyType.Intiger => FloatValue.ToString(),
                PropertyType.Bool => FloatValue.ToString(),
                PropertyType.KeywordToggle => FloatValue.ToString(),
                _ => throw new System.NotImplementedException(),
            };
        }

        public string TypeToString()
        {
            if (type == PropertyType.Float && HasRange)
            {
                var range = Range;
                return $"Range ({range.x:R}, {range.y:R})";
            }

            return type switch
            {
                PropertyType.Float => "Float",
                PropertyType.Float2 => "Vector",
                PropertyType.Float3 => "Vector",
                PropertyType.Float4 => "Vector",
                PropertyType.Color => "Color",
                PropertyType.Intiger => "Intiger",
                PropertyType.Texture2D => "2D",
                PropertyType.Texture3D => "3D",
                PropertyType.TextureCube => "Cube",
                PropertyType.Texture2DArray => "2DArray",
                PropertyType.TextureCubeArray => "CubeArray",
                PropertyType.Bool => "Float",
                PropertyType.KeywordToggle => "Float",
                _ => throw new System.NotImplementedException()
            };
        }

        public string GetFieldDeclaration(GenerationMode generationMode)
        {
            var referenceName = GetReferenceName(generationMode);

            if (type == PropertyType.KeywordToggle)
            {
                if (FloatValue > 0)
                {
                    return "#define " + KeywordName;
                }
                return string.Empty;
            }

            return type switch
            {
                PropertyType.Float => $"float {referenceName};",
                PropertyType.Float2 => $"float2 {referenceName};",
                PropertyType.Float3 => $"float3 {referenceName};",
                PropertyType.Float4 => $"float4 {referenceName};",
                PropertyType.Color => $"float4 {referenceName};",
                PropertyType.Intiger => $"int {referenceName};",
                PropertyType.Bool => $"float {referenceName};",
                PropertyType.KeywordToggle => $"#pragma {referenceName}",
                PropertyType.Texture2D => $"TEXTURE2D({referenceName}); SAMPLER(sampler{referenceName});",
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
                if (string.IsNullOrEmpty(attribute))
                {
                    continue;
                }

                sb.Append("[");
                sb.Append(attribute);
                sb.Append("] ");
            }

            if (type == PropertyType.Bool)
            {
                sb.Append("[ToggleUI]");
            }
            else if (type == PropertyType.KeywordToggle)
            {
                sb.Append($"[Toggle({KeywordName})]");
            }

            return sb.ToString();
        }

        public string GetReferenceName(GenerationMode generationMode)
        {
            if (useReferenceName)
            {
                generationMode = GenerationMode.Final;
            }

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
        public bool ShouldDeclare() => declaration == PropertyDeclaration.Local;
        public string GetPropertyDeclaration(GenerationMode generationMode)
        {
            var referenceName = GetReferenceName(generationMode);
            var type = TypeToString();
            var attributes = AttributesToString();
            var defaultValue = GetDefaultValue();

            if (this.type == PropertyType.KeywordToggle)
            {
                referenceName = KeywordName;
            }

            return $"{attributes} {referenceName} (\"{displayName}\", {type}) = {defaultValue}";
        }

        void OnDefaultGUI()
        {
            // EditorGUILayout.LabelField(guid);

            EditorGUI.BeginChangeCheck();

            displayName = EditorGUILayout.TextField(new GUIContent("Display Name"), displayName);
            referenceName = EditorGUILayout.TextField(new GUIContent("Reference Name"), referenceName);
            if (type != PropertyType.Texture2D && type != PropertyType.TextureCube)
            {
                declaration = (PropertyDeclaration)EditorGUILayout.EnumPopup("Declaration", declaration);
            }

            bool hideInInspector = EditorGUILayout.Toggle("Hide In Inspector", PropertyAttributes.Get(attributes, HideInInspectorAttribute));

            if (EditorGUI.EndChangeCheck())
            {
                PropertyAttributes.Set(attributes, hideInInspector, HideInInspectorAttribute);
            }
        }

        void OnGUIFloat()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Range", GUILayout.Width(149));
            Range = EditorGUILayout.Vector2Field("", Range);
            GUILayout.EndHorizontal();
            float newValue;
            if (HasRange)
            {
                newValue = EditorGUILayout.Slider("Value", FloatValue, rangeX, rangeY);
            }
            else
            {
                newValue = EditorGUILayout.FloatField("Value", FloatValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                FloatValue = newValue;
                UpdatePreviewMaterial();
            }
        }
        void OnGUIBool()
        {
            EditorGUI.BeginChangeCheck();
            bool newValue = GUILayout.Toggle(FloatValue == 1, "Toggle");
            if (EditorGUI.EndChangeCheck())
            {
                FloatValue = newValue ? 1 : 0;
                UpdatePreviewMaterial();
            }
        }
        void OnGUIVector()
        {
            EditorGUI.BeginChangeCheck();
            Vector4 newValue = EditorGUILayout.Vector4Field("", VectorValue);
            if (EditorGUI.EndChangeCheck())
            {
                VectorValue = newValue;
                UpdatePreviewMaterial();
            }
        }

        void OnGUITexture()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Texture", GUILayout.Width(149));
            Texture newValue = (Texture)EditorGUILayout.ObjectField(DefaultTextureValue, typeof(Texture2D), false);
            GUILayout.EndHorizontal();

            bool modifable = EditorGUILayout.Toggle("Non Modifiable", PropertyAttributes.Get(attributes, NonModifiableTextureAttribute));
            bool scaleOffset = EditorGUILayout.Toggle("No Scale Offset", PropertyAttributes.Get(attributes, NoScaleOffsetAttribute));
            bool normal = EditorGUILayout.Toggle("Normal", PropertyAttributes.Get(attributes, NormalAttribute));
            var defaultTex = EditorGUILayout.EnumPopup("Default Texture", DefaultTextureEnum);

            if (EditorGUI.EndChangeCheck())
            {
                PropertyAttributes.Set(attributes, modifable, NonModifiableTextureAttribute);
                PropertyAttributes.Set(attributes, scaleOffset, NoScaleOffsetAttribute);
                PropertyAttributes.Set(attributes, normal, NormalAttribute);

                DefaultTextureValue = newValue;
                DefaultTextureEnum = (DefaultTextureName)defaultTex;
                UpdatePreviewMaterial();
            }
        }

        void OnGUIColor()
        {
            EditorGUI.BeginChangeCheck();
            Color newValue = EditorGUILayout.ColorField("Color", VectorValue);

            bool hdr = EditorGUILayout.Toggle("HDR", PropertyAttributes.Get(attributes, "HDR"));

            if (EditorGUI.EndChangeCheck())
            {
                PropertyAttributes.Set(attributes, hdr, "HDR");

                VectorValue = newValue;
                UpdatePreviewMaterial();
            }
        }

        public void UpdatePreviewMaterial()
        {
            Material m = graphView.PreviewMaterial;
            string name = GetReferenceName(GenerationMode.Preview);
            if (type == PropertyType.Float || type == PropertyType.Bool) m.SetFloat(name, FloatValue);
            else if (type == PropertyType.Float2 || type == PropertyType.Float3 || type == PropertyType.Float4) m.SetVector(name, VectorValue);
            else if (type == PropertyType.Color) m.SetColor(name, VectorValue);
            else if (IsTextureType) m.SetTexture(name, DefaultTextureValue);

            if (type == PropertyType.KeywordToggle)
            {
                foreach (var element in graphView.graphElements)
                {
                    if (element is KeywordPropertyNode keywordProperyNode)
                    {
                        keywordProperyNode.GeneratePreviewForAffectedNodes();
                    }
                }
            }
        }

        [NonSerialized] public ShaderGraphView graphView;

        public VisualElement PropertyEditorGUI()
        {
            var imgui = new IMGUIContainer(OnDefaultGUI); // too much data to bind, easier to just use imgui
            if (type == PropertyType.Float) imgui.onGUIHandler += OnGUIFloat;
            else if (type == PropertyType.Float2 || type == PropertyType.Float3 || type == PropertyType.Float4) imgui.onGUIHandler += OnGUIVector;
            else if (type == PropertyType.Color) imgui.onGUIHandler += OnGUIColor;
            else if (type == PropertyType.Bool || type == PropertyType.KeywordToggle) imgui.onGUIHandler += OnGUIBool;
            else if (type == PropertyType.Texture2D) imgui.onGUIHandler += OnGUITexture;

            var s = imgui.style;
            s.marginLeft = 6;
            s.marginRight = 6;
            s.marginTop = 6;
            s.marginBottom = 6;


            return imgui;
        }

        public Type GetNodeType()
        {
            return type switch
            {
                PropertyType.Float => typeof(FloatPropertyNode),
                PropertyType.Float2 => typeof(Float2PropertyNode),
                PropertyType.Float3 => typeof(Float3PropertyNode),
                PropertyType.Float4 => typeof(Float4PropertyNode),
                PropertyType.Color => typeof(ColorPropertyNode),
                PropertyType.Intiger => throw new NotImplementedException(),
                PropertyType.Bool => typeof(BooleanPropertyNode),
                PropertyType.Texture2D => typeof(Texture2DPropertyNode),
                PropertyType.TextureCube => throw new NotImplementedException(),
                PropertyType.KeywordToggle => typeof(KeywordPropertyNode),

                _ => throw new NotImplementedException(),
            };
        }
    }

}