using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditorInternal;
using UnityEngine;

namespace ZSG
{
    [ScriptedImporter(2, "zsgl", 0)]
    public class CustomLighting : ScriptedImporter
    {
        public List<PropertyDescriptor> properties = new();

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var file = ScriptableObject.CreateInstance<CustomLightingAsset>();

            file.properties = properties;

            ctx.AddObjectToAsset("main", file);
        }
    }


    [CustomEditor(typeof(CustomLighting))]
    public class CustomLightingEditor : ScriptedImporterEditor
    {
        private ReorderableList _reorderableList;

        //static Dictionary<int, int> _previousSelection = new ();
        public override void OnEnable()
        {
            base.OnEnable();



            var t = (CustomLighting)target;

            _reorderableList = PropertyDescriptor.CreateReordableList(t.properties, null);

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            _reorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);

                serializedObject.ApplyModifiedProperties();
            }

            ApplyRevertGUI();
        }
    }
}
