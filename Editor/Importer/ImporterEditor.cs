using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;
namespace z3y.ShaderGraph
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
                importer.OpenInGraphView();
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