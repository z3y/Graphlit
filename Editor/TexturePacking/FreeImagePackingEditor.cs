using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using static Graphlit.FreeImagePacking;

namespace Graphlit
{
    public class FreeImagePackingEditor : EditorWindow
    {
        [MenuItem("Tools/Graphlit/Texture Packing")]
        public static void Init() => Init(true);
        public static void Init(bool resetFields)
        {
            var window = (FreeImagePackingEditor)GetWindow(typeof(FreeImagePackingEditor));
            window.titleContent = new GUIContent("Texture Packing");
            window.Show();
            window.minSize = new Vector2(400, 550);
            if (resetFields)
            {
                ResetFields();
            }
        }

        private static Shader _previewShader;
        private static Material _preview0;
        private static Material _preview1;
        private static Material _preview2;
        private static Material _preview3;
        private static Texture2D whiteTexture;

        public static bool settingsNeedApply = false;
        public static Action onPackingFinished = delegate { };

        private void OnEnable()
        {
            _firstTime = true;
            _previewShader = Shader.Find("Hidden/Graphlit/PackingPreview");
            _preview0 = new Material(_previewShader);
            _preview1 = new Material(_previewShader);
            _preview2 = new Material(_previewShader);
            _preview3 = new Material(_previewShader);
            whiteTexture = Texture2D.whiteTexture;
            LastPackingTime = 0;
        }

        public static void ResetFields()
        {
            _firstTime = true;
            _packingMaterial = null;
            _packingPropertyName = null;
            ChannelR = new PackingField();
            ChannelG = new PackingField();
            ChannelB = new PackingField();
            ChannelA = new PackingField();
            Linear = false;

            ChannelR.DisplayName = "Red";
            ChannelG.DisplayName = "Green";
            ChannelB.DisplayName = "Blue";
            ChannelA.DisplayName = "Alpha";

            ChannelG.Channel.Source = ChannelSource.Green;
            ChannelB.Channel.Source = ChannelSource.Blue;
            ChannelA.Channel.Source = ChannelSource.Red;

            ChannelR.Channel.DefaultColor = DefaultColor.White;
            ChannelG.Channel.DefaultColor = DefaultColor.White;
            ChannelB.Channel.DefaultColor = DefaultColor.White;

            packingSuffix = "";
        }

        public static PackingField ChannelR;
        public static PackingField ChannelG;
        public static PackingField ChannelB;
        public static PackingField ChannelA;

        public static string packingSuffix = "";


        public static Texture2D PackedTexture = null;
        private static Vector2Int _customSize = new Vector2Int(1024, 1024);
        public static bool Linear = false;
        public static TextureSize Size = FreeImagePacking.TextureSize.Default;

        private static Material _packingMaterial = null;
        private static string _packingPropertyName = null;

        public struct PackingField
        {
            public Texture2D UnityTexture;
            public TextureChannel Channel;
            public string DisplayName;
            public string InvertDisplayName;
        }

        private const string _guiStyle = "helpBox";

        private static bool _firstTime = true;
        public void OnGUI()
        {
            if (_packingMaterial)
            {
                EditorGUILayout.BeginVertical(_guiStyle);

                EditorGUILayout.LabelField(new GUIContent($"Material - {_packingMaterial.name}"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent($"Texture - {_packingPropertyName}"));
                if (_packingMaterial.GetTexture(_packingPropertyName) && GUILayout.Button("Modify"))
                {
                    ChannelA.Channel.Source = ChannelSource.Alpha;
                    var texture = _packingMaterial.GetTexture(_packingPropertyName);
                    if (texture is Texture2D texture2D)
                    {
                        ChannelR.UnityTexture = texture2D;
                        ChannelG.UnityTexture = texture2D;
                        ChannelB.UnityTexture = texture2D;
                        ChannelA.UnityTexture = texture2D;
                    }

                    _firstTime = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }







            TexturePackingField(ref ChannelR, Color.red, _preview0, _firstTime);
            TexturePackingField(ref ChannelG, Color.green, _preview1, _firstTime);
            TexturePackingField(ref ChannelB, Color.blue, _preview2, _firstTime);
            TexturePackingField(ref ChannelA, Color.white, _preview3, _firstTime);
            _firstTime = false;

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(_guiStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preset", GUILayout.Width(149));
            if (GUILayout.Button("Mask Map"))
            {
                ChannelR.Channel.DefaultColor = DefaultColor.White;
                ChannelG.Channel.DefaultColor = DefaultColor.White;
                ChannelB.Channel.DefaultColor = DefaultColor.Black;
                ChannelA.Channel.DefaultColor = DefaultColor.White;

                ChannelR.DisplayName = "Ambient Occlusion";
                ChannelG.DisplayName = "Roughness";
                ChannelG.InvertDisplayName = "Smoothness";
                ChannelB.DisplayName = "Metallic";
                ChannelA.DisplayName = "Detail Mask";
                packingSuffix = "_MaskMap";
            }
            if (GUILayout.Button("Metallic Smoothness"))
            {
                ChannelR.Channel.DefaultColor = DefaultColor.Black;
                ChannelG.Channel.DefaultColor = DefaultColor.Black;
                ChannelB.Channel.DefaultColor = DefaultColor.Black;
                ChannelA.Channel.DefaultColor = DefaultColor.White;

                ChannelR.DisplayName = "Metallic";
                ChannelA.DisplayName = "Smoothness";
                ChannelA.InvertDisplayName = "Roughness";
                ChannelB.DisplayName = "";
                ChannelG.DisplayName = "";
                packingSuffix = "_MetallicSmoothness";
            }
            EditorGUILayout.EndHorizontal();

            PackingFormat = (TexturePackingFormat)EditorGUILayout.EnumPopup("Format", PackingFormat);
            ImageFilter = (FreeImage.FREE_IMAGE_FILTER)EditorGUILayout.EnumPopup(new GUIContent("Rescale Filter", "Filter that will be used for rescaling textures to match them to the target size if needed"), ImageFilter);
            Size = (TextureSize)EditorGUILayout.EnumPopup(new GUIContent("Size", "Specify the size of the packed texture"), Size);
            if (Size == TextureSize.Custom)
            {
                EditorGUI.indentLevel++;
                _customSize = EditorGUILayout.Vector2IntField(new GUIContent(), _customSize);
                EditorGUI.indentLevel--;
            }
            //Linear = EditorGUILayout.Toggle(new GUIContent("Linear", "Disable sRGB on texture import, for mask and data textures (Roughness, Occlusion, Metallic etc)"), Linear);

            packingSuffix = EditorGUILayout.TextField(new GUIContent("Suffix"), packingSuffix);


            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset", GUILayout.Height(30)))
            {
                ResetFields();
            }

            if (GUILayout.Button("Pack", GUILayout.Height(30)))
            {
                GetTexturePath(ref ChannelR);
                GetTexturePath(ref ChannelG);
                GetTexturePath(ref ChannelB);
                GetTexturePath(ref ChannelA);


                var referenceTexture = ChannelG.UnityTexture ?? ChannelA.UnityTexture ?? ChannelR.UnityTexture ?? ChannelB.UnityTexture;
                if (referenceTexture == null) return;

                var path = AssetDatabase.GetAssetPath(referenceTexture);
                var fullPath = Path.GetFullPath(path);

                var absolutePath = GetPackedTexturePath(fullPath);
                var unityPath = GetPackedTexturePath(path);

                int width = (int)Size;
                int height = (int)Size;

                if (Size == TextureSize.Default)
                {
                    width = referenceTexture.width;
                    height = referenceTexture.height;
                }
                else if (Size == TextureSize.Custom)
                {
                    width = _customSize.x;
                    height = _customSize.y;
                }

                PackCustom(absolutePath, ChannelR.Channel, ChannelG.Channel, ChannelB.Channel, ChannelA.Channel, (width, height), PackingFormat);

                settingsNeedApply = true;
                AssetDatabase.ImportAsset(unityPath, ImportAssetOptions.ForceUpdate);

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(unityPath);

                EditorGUIUtility.PingObject(texture);

                if (_packingMaterial)
                {
                    _packingMaterial.SetTexture(_packingPropertyName, texture);
                    ShaderInspector.Reinitialize();
                    MaterialEditor.ApplyMaterialPropertyDrawers(_packingMaterial);
                }

                onPackingFinished.Invoke();
            }
            EditorGUILayout.EndHorizontal();

            if (LastPackingTime > 0)
            {
                EditorGUILayout.LabelField($"Packed in {LastPackingTime}ms");
            }

        }

        public static void AddPackingMaterial(Material material, MaterialProperty property)
        {
            _packingPropertyName = property.name;
            _packingMaterial = material;
        }
        public static void AddPackingMaterial(Material material, string propertyName)
        {
            _packingPropertyName = propertyName;
            _packingMaterial = material;
        }

        public static string GetPackedTexturePath(string referencePath)
        {
            var directory = Path.GetDirectoryName(referencePath);
            var fileName = Path.GetFileNameWithoutExtension(referencePath);

            var newPath = directory + @"\" + fileName + 
                (string.IsNullOrEmpty(packingSuffix) ? "_packed" : packingSuffix);
            var extension = PackingFormat.GetExtension();

            newPath = newPath + "." + extension;

            return newPath;
        }



        public static int LastPackingTime = 0;


        private static void GetTexturePath(ref PackingField field)
        {
            if (field.UnityTexture is null) field.Channel.Path = null;

            var path = AssetDatabase.GetAssetPath(field.UnityTexture);
            field.Channel.Path = path;
        }

        private void TexturePackingField(ref PackingField field, Color color, Material previewMaterial, bool firstTime = false)
        {

            EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginVertical(_guiStyle);

            //GUILayout.Space(1);





            GUILayout.BeginHorizontal();
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,

            };

            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Width(80), GUILayout.Height(80));
            EditorGUI.DrawPreviewTexture(previewRect, whiteTexture, previewMaterial, ScaleMode.ScaleToFit);

            var rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y, 2, rect.height), color);
            GUILayout.Space(5);
            GUILayout.BeginVertical();


            field.UnityTexture = (Texture2D)EditorGUILayout.ObjectField(field.UnityTexture, typeof(Texture2D), false);

            GUILayout.BeginHorizontal();

            field.Channel.Source = (ChannelSource)EditorGUILayout.EnumPopup(field.Channel.Source, GUILayout.Width(80));



            GUILayout.Label("➜", GUILayout.Width(15));

            if (field.InvertDisplayName != null && field.Channel.Invert)
            {
                GUILayout.Label($"<b>{field.InvertDisplayName}</b>", style, GUILayout.Width(120));
            }
            else
            {
                GUILayout.Label($"<b>{field.DisplayName}</b>", style, GUILayout.Width(120));
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();


            field.Channel.Invert = GUILayout.Toggle(field.Channel.Invert, "Invert", GUILayout.Width(70));

            GUILayout.Label("Fallback", GUILayout.Width(55));

            field.Channel.DefaultColor = (DefaultColor)EditorGUILayout.EnumPopup(field.Channel.DefaultColor, GUILayout.Width(60));

            GUILayout.EndHorizontal();




            GUILayout.EndVertical();


            GUILayout.EndHorizontal();

            //GUILayout.Space(1);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck() || firstTime)
            {
                previewMaterial.SetTexture("_Texture0", field.UnityTexture);
                previewMaterial.SetFloat("_Texture0Channel", (int)field.Channel.Source);
                previewMaterial.SetFloat("_Texture0Invert", field.Channel.Invert ? 1f : 0f);
                if (!field.UnityTexture)
                {
                    previewMaterial.SetFloat("_Texture0Invert", field.Channel.DefaultColor == DefaultColor.Black ? 1f : 0f);
                }
            }
        }
    }
}