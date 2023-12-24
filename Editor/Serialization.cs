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
    public struct Connection
    {
        public Connection(int portID, List<ConnectionPorts> ports)
        {
            this.portID = portID;
            this.ports = ports;
        }
        public int portID;
        public List<ConnectionPorts> ports;
    }

    [System.Serializable]
    public struct ConnectionPorts
    {
        public ConnectionPorts(ShaderNode node, int portID)
        {
            this.node = node;
            this.portID = portID;
        }
        [SerializeReference] public ShaderNode node;
        public int portID;
    }
}