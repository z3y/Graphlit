using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditorInternal;
namespace Enlit
{

    [CustomEditor(typeof(ShaderGraphImporter))]
    internal class ShaderGraphImporterEditor : ScriptedImporterEditor
    {
        /*public override VisualElement CreateInspectorGUI()
        {
            var importer = (ShaderGraphImporter)serializedObject.targetObject;
            var assetPath = AssetDatabase.GetAssetPath(importer);

            var root = new VisualElement();
            root.Add(new ObjectField());

            var revertGui = new IMGUIContainer(RevertGUI);
            root.Add(revertGui);
            return root;
        }

        private void RevertGUI()
        {
            ApplyRevertGUI();
        }*/

        public override void OnInspectorGUI()
        {
            var importer = (ShaderGraphImporter)serializedObject.targetObject;
            base.OnInspectorGUI();


            if (GUILayout.Button("Edit Shader"))
            {
                ShaderGraphImporter.OpenInGraphView(AssetDatabase.GUIDFromAssetPath(importer.assetPath).ToString());
            }
            if (GUILayout.Button("Show Generated Shader"))
            {
                AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
                string path = "Temp/Enlit.shader";
                File.WriteAllText(path, ShaderGraphImporter._lastImport);
                InternalEditorUtility.OpenFileAtLineExternal(Path.GetFullPath(path), 0);
            }
            if (GUILayout.Button("Copy Shader"))
            {
                AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
                GUIUtility.systemCopyBuffer = ShaderGraphImporter._lastImport;
            }
        }

        /*        protected override bool OnApplyRevertGUI()
                {
                    return false;
                    //return base.OnApplyRevertGUI();
                }*/

        protected override bool needsApplyRevert => false;
    }
}