using UnityEditor;
using UnityEngine;

namespace ZSG
{
    public class SerializedGraphDataSo : ScriptableObject
    {
        public void Init()
        {
            Undo.undoRedoPerformed -= graphView.OnUndoPerformed;
            Undo.undoRedoPerformed += graphView.OnUndoPerformed;
        }
        public ShaderGraphView graphView;
    }
}