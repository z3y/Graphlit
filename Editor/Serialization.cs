using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [System.Serializable]
    public class SerializedGraphData
    {
        [SerializeReference] public ShaderNode[] shaderNodes;
        [SerializeField] public string shaderName;
    }
    
    public class SerializedGraphDataSo : ScriptableObject
    {
        public void Init()
        {
            Undo.undoRedoPerformed -= graphView.OnUndoPerformed;
            Undo.undoRedoPerformed += graphView.OnUndoPerformed;
        }
        public ShaderGraphView graphView;
    }

    [System.Serializable]
    public struct NodeConnection
    {
        public NodeConnection(int outID, int inID, ShaderNode outNode)
        {
            this.outID = outID;
            this.inID = inID;
            this.inNode = outNode;
        }
        public int outID;
        public int inID;
        [SerializeReference] public ShaderNode inNode;
    }
}