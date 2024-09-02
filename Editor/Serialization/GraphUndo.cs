using UnityEditor;
using UnityEngine;

namespace Enlit
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