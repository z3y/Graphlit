using System.Collections.Generic;
using UnityEngine;
using static Graphlit.SubgraphOutputNode;

namespace Graphlit
{
    public class Subgraph : ScriptableObject
    {
        public string function = "";
        public string functionName = "";
        public List<SerializablePortDescriptor> outputs = new();
        public List<SerializablePortDescriptor> inputs = new();
    }
}