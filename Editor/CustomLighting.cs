using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditorInternal;
using UnityEngine;

namespace Graphlit
{
    [ScriptedImporter(2, EXTENSION, 0)]
    public class CustomLighting : ScriptedImporter
    {
        const string EXTENSION = "graphlitc";

        public List<PropertyDescriptor> properties = new();

        [Serializable]
        public class CustomPort
        {
            public string name;
            public Vector4 value;
            [Range(1, 4)] public int dimension;
            public int id;
            public PortBinding binding;

            public string ValueToString()
            {
                string x, y, z, w;
                x = value.x.ToString(CultureInfo.InvariantCulture);
                y = value.y.ToString(CultureInfo.InvariantCulture);
                z = value.z.ToString(CultureInfo.InvariantCulture);
                w = value.w.ToString(CultureInfo.InvariantCulture);

                return dimension switch
                {
                    1 => $"float({x})",
                    2 => $"float2({x}, {y})",
                    3 => $"float3({x}, {y}, {z})",
                    4 or _ => $"float4({x}, {y}, {z}, {w})",
                };
            }
        }

        public List<CustomPort> outputs = new();

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var file = ScriptableObject.CreateInstance<CustomLightingAsset>();

            file.properties = properties;
            file.outputs = outputs;

            ctx.AddObjectToAsset("main", file);
        }

        [MenuItem("Assets/Create/Graphlit/Custom Lighting Asset")]
        public static void CreateCustomLightingAsset()
        {
            var text = File.ReadAllText("Packages/com.z3y.graphlit/Shaders/Custom Lighting/Example.graphlitc");
            ProjectWindowUtil.CreateAssetWithContent($"New Custom Lighting.{EXTENSION}", text);
        }
    }


    [CustomEditor(typeof(CustomLighting))]
    public class CustomLightingEditor : ScriptedImporterEditor
    {
        private ReorderableList _reorderableList;
        private SerializedProperty _outputs;

        //static Dictionary<int, int> _previousSelection = new ();
        public override void OnEnable()
        {
            base.OnEnable();



            var t = (CustomLighting)target;

            _outputs = serializedObject.FindProperty("outputs");

            _reorderableList = PropertyDescriptor.CreateReordableList(t.properties, null);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var t = (CustomLighting)target;

            if (_reorderableList.list != t.properties)
            {
                _reorderableList = PropertyDescriptor.CreateReordableList(t.properties, null);
            }

            EditorGUI.BeginChangeCheck();

            _reorderableList.DoLayoutList();

            EditorGUILayout.PropertyField(_outputs);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);

                serializedObject.ApplyModifiedProperties();
            }

            ApplyRevertGUI();
        }
    }
}
