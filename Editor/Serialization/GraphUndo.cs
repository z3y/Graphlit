/*using System;
using UnityEditor;
using UnityEngine;

namespace Graphlit
{
    public class SerializedGraphDataSo : ScriptableObject
    {
        public void Init()
        {
            Undo.undoRedoEvent -= graphView.OnUndoRedoPerformed;
            Undo.undoRedoEvent += graphView.OnUndoRedoPerformed;
        }
        [NonSerialized] public ShaderGraphView graphView;
    }
}*/