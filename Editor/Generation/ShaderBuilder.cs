using System.Collections.Generic;
using z3y.ShaderGraph.Nodes;
using static UnityEngine.EventSystems.StandaloneInputModule;

namespace z3y.ShaderGraph
{
    public class ShaderBuilder
    {
        public ShaderBuilder(GenerationMode generationMode, SerializableGraph serializableGraph)
        {
            GenerationMode = generationMode;
            SerializableGraph = serializableGraph;
            DeserializeAndMapGuids();
            FillConnections();
            visitors.Add(new PropertyVisitor(this));
        }

        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public HashSet<string> properties = new();
        public List<PassBuilder> passBuilders = new();
        public List<NodeVisitor> visitors = new();

        private ShaderStringBuilder _sb;

        public GenerationMode GenerationMode { get; }
        public SerializableGraph SerializableGraph { get; }

        public void AddPass(PassBuilder passBuilder)
        {
            int passIndex = passBuilders.Count;
            passBuilders.Add(passBuilder);

            //visitors.Add(new DescriptionVisitor(this, ShaderStage.Vertex, passIndex));
            visitors.Add(new DescriptionVisitor(this, ShaderStage.Fragment, passIndex));
            //visitors.Add(new FunctionVisitor(this, passIndex));

        }

        public Dictionary<string, ShaderNode> GuidToNode { get; private set; } = new();
        public Dictionary<ShaderNode, SerializableNode> SerializableNodeToNode { get; private set; } = new();
        public List<ShaderNode> ShaderNodes { get; private set; } = new();

        private void DeserializeAndMapGuids()
        {
            foreach (var serializableNode in SerializableGraph.nodes)
            {
                if (!serializableNode.TryDeserialize(out var shaderNode))
                {
                    continue;
                }
                GuidToNode.Add(serializableNode.guid, shaderNode);
                ShaderNodes.Add(shaderNode);
                SerializableNodeToNode.Add(shaderNode, serializableNode);
            }
        }

        private void FillConnections()
        {
            foreach (var shaderNode in ShaderNodes)
            {
                var serializableNode = SerializableNodeToNode[shaderNode];
                foreach (var connection in serializableNode.connections)
                {
                    connection.MapToNode(GuidToNode[connection.node]);
                    shaderNode.Inputs.Add(connection.GetInputIDForThisNode(), connection);
                }
            }
        }

        public void Build<T>() where T : ShaderNode // later shader target
        {
            ShaderNode.ResetUniqueVariableIDs();
            var t = ShaderNodes.Find(x => x is T);
            TraverseGraph(t);
            t.Visit(GenerationMode, visitors);
        }

        public void TraverseGraph(ShaderNode shaderNode)
        {
            var inputs = shaderNode.Inputs;
            foreach (var input in inputs.Values)
            {
                var inputNode = input.Node;
                if (inputNode.visited)
                {
                    // copy
                    //node.PortNames[input.outID] = inputNode.SetOutputString(input.inID);
                    //var portType = input.inNode.PortsTypes[input.inID];
                    //node.PortsTypes[input.outID] = portType;
                    shaderNode.VariableNames[input.b] = inputNode.VariableNames[input.a];
                    shaderNode.Ports[input.b].Type = inputNode.Ports[input.a].Type;

                    continue;
                }

                TraverseGraph(inputNode);

                inputNode.Visit(GenerationMode, visitors);
                {
                    // copy
                    //node.PortNames[input.outID] = inputNode.SetOutputString(input.inID);
                    //var portType = input.inNode.PortsTypes[input.inID];
                    //node.PortsTypes[input.outID] = portType;
                    shaderNode.VariableNames[input.b] = inputNode.VariableNames[input.a];
                    shaderNode.Ports[input.b].Type = inputNode.Ports[input.a].Type;
                }

                //inputNode.UpdateGraphView();
                inputNode.visited = true;
            }
        }
        
        public override string ToString()
        {
            _sb = new ShaderStringBuilder();
            _sb.AppendLine("Shader \"" + shaderName + '"');

            _sb.Indent();
            {
                _sb.AppendLine("Properties");
                _sb.Indent();
                {
                    AppendProperties();
                }
                _sb.UnIndent();

                _sb.AppendLine("SubShader");
                _sb.Indent();
                {
                    AppendTags(subshaderTags);

                    AppendPasses();
                }
                _sb.UnIndent();
            }

            _sb.AppendLine(string.IsNullOrEmpty(fallback) ? "// Fallback None" : "Fallback \"" + fallback + "\"");
            _sb.AppendLine(string.IsNullOrEmpty(customEditor) ? "// CustomEditor None" : "CustomEditor \"" + customEditor + "\"");

            _sb.UnIndent();

            return _sb.ToString();
        }

        private void AppendProperties()
        {
            foreach (var property in properties)
            {
                _sb.AppendLine(property);
            }
        }

        private void AppendTags(Dictionary<string, string> tags)
        {
            _sb.AppendLine("Tags");

            _sb.Indent();
            foreach (var tag in tags)
            {
                _sb.AppendLine('"' + tag.Key + "\" = \"" + tag.Value + '"');
            }
            _sb.UnIndent();
        }

        private void AppendPasses()
        {
            foreach (var pass in passBuilders)
            {
                _sb.AppendLine("Pass");
                _sb.Indent();
                {
                    AppendPass(pass);
                }
                _sb.UnIndent();
            }
        }

        private void AppendPass(PassBuilder pass)
        {
            _sb.AppendLine("Name \"" + pass.name + "\"");
            AppendTags(pass.tags);

            _sb.AppendLine("// Render States");

            _sb.AppendLine("HLSLPROGRAM");
            AppendPassHLSL(pass);
            _sb.AppendLine("ENDHLSL");
        }

        private void AppendPassHLSL(PassBuilder pass)
        {
            _sb.AppendLine("// Pragmas");

            _sb.AppendLine("struct Attributes");
            _sb.Indent();
            _sb.UnIndent("};");

            _sb.AppendLine("struct Varyings");
            _sb.Indent();
            _sb.UnIndent("};");

            AppendVertexDescription(pass);
            AppendSurfaceDescription(pass);

            _sb.AppendLine("#include \"" + pass.vertexShaderPath + '"');
            _sb.AppendLine("#include \"" + pass.fragmentShaderPath + '"');
        }

        private void AppendSurfaceDescription(PassBuilder pass)
        {
            _sb.AppendLine("SurfaceDescription SurfaceDescriptionFunction(Varyings varyings)");
            _sb.Indent();
            foreach (var line in pass.surfaceDescription)
            {
                _sb.AppendLine(line);
            }
            _sb.UnIndent();

        }

        private void AppendVertexDescription(PassBuilder pass)
        {
            _sb.AppendLine("VertexDescription VertexDescriptionFunction(Attributes attributes)");
            _sb.Indent();
            foreach (var line in pass.vertexDescription)
            {
                _sb.AppendLine(line);
            }
            _sb.UnIndent();
        }
    }
}
