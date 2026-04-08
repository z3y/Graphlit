using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditorInternal;
using System.Linq;
namespace Graphlit
{

    [CustomEditor(typeof(GraphlitImporter))]
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

        public string GetShaderSource(string assetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            var textAsset = assets.OfType<TextAsset>().FirstOrDefault(a => a.name == "Shader Source");

            return textAsset != null ? textAsset.text : string.Empty;
        }

        public override void OnInspectorGUI()
        {
            var importer = (GraphlitImporter)serializedObject.targetObject;
            base.OnInspectorGUI();


            if (GUILayout.Button("Edit Shader"))
            {
                GraphlitImporter.OpenInGraphView(AssetDatabase.GUIDFromAssetPath(importer.assetPath).ToString());
            }
            if (GUILayout.Button("Show Generated Shader"))
            {
                string tempPath = "Temp/Graphlit.shader";
                File.WriteAllText(tempPath, GetShaderSource(importer.assetPath));
                InternalEditorUtility.OpenFileAtLineExternal(Path.GetFullPath(tempPath), 0);
            }
            if (GUILayout.Button("Copy Shader"))
            {
                GUIUtility.systemCopyBuffer = GetShaderSource(importer.assetPath);
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