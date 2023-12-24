using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [System.Serializable]
    public class SerializedGraphData
    {
        [SerializeReference] public List<ShaderNode> shaderNodes;
        [SerializeField] public string shaderName;
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