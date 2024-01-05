using System.Collections.Generic;
using UnityEditor.Hardware;
using z3y.ShaderGraph.Nodes;
using static UnityEngine.EventSystems.StandaloneInputModule;

namespace z3y.ShaderGraph
{
    public class ShaderBuilder
    {
        public ShaderBuilder(GenerationMode generationMode, SerializableGraph serializableGraph, ShaderGraphView shaderGraphView)
        {
            GenerationMode = generationMode;
            SerializableGraph = serializableGraph;
            ShaderGraphView = shaderGraphView;
            DeserializeAndMapGuids();
            FillConnections();
            //visitors.Add(new PropertyVisitor(this));
        }

        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public HashSet<string> properties = new();
        public List<PassBuilder> passBuilders = new();
        //public List<NodeVisitor> visitors = new();

        private ShaderStringBuilder _sb;

        public GenerationMode GenerationMode { get; }
        public SerializableGraph SerializableGraph { get; }
        public ShaderGraphView ShaderGraphView { get; }

        public void AddPass(PassBuilder passBuilder)
        {
            int passIndex = passBuilders.Count;
            passBuilders.Add(passBuilder);

            //visitors.Add(new DescriptionVisitor(this, ShaderStage.Vertex, passIndex));
            //visitors.Add(new DescriptionVisitor(this, ShaderStage.Fragment, passIndex));
            //visitors.Add(new FunctionVisitor(this, passIndex));

        }

        public Dictionary<string, ShaderNode> GuidToNode { get; private set; } = new();
        public Dictionary<ShaderNode, SerializableNode> NodeToSerializableNode { get; private set; } = new();
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
                NodeToSerializableNode.Add(shaderNode, serializableNode);
            }
        }

        private void FillConnections()
        {
            foreach (var shaderNode in ShaderNodes)
            {
                var serializableNode = NodeToSerializableNode[shaderNode];
                foreach (var connection in serializableNode.connections)
                {
                    connection.MapToNode(GuidToNode[connection.node]);
                    shaderNode.Inputs.Add(connection.GetInputIDForThisNode(), connection);
                }
            }
        }

        public void Build<T, U>() where T : ShaderNode // later shader target
        {
            ShaderNode.ResetUniqueVariableIDs();
            var t = ShaderNodes.Find(x => x is T);
            var u = ShaderNodes.Find(x => x is U);

            TraverseGraph(t, new PropertyVisitor(this));
            TraverseGraph(u, new PropertyVisitor(this));

            for (int i = 0; i < passBuilders.Count; i++)
            {
                int passIndex = i;
                TraverseDescriptionGraph(u, new DescriptionVisitor(this, ShaderStage.Vertex, passIndex));
                TraverseDescriptionGraph(t, new DescriptionVisitor(this, ShaderStage.Fragment, passIndex));

                foreach (var shaderNode in ShaderNodes)
                {
                    shaderNode.visited = false;
                }

                TraverseGraph(t, new FunctionVisitor(this, passIndex));
                TraverseGraph(u, new FunctionVisitor(this, passIndex));
            }




            if (ShaderGraphView is not null)
            {
                foreach (var shaderNode in ShaderNodes)
                {
                    ShaderGraphView.UpdateGraphView(NodeToSerializableNode[shaderNode].guid, shaderNode);
                }
            }
        }

       private void ResetNodes()
        {
            ShaderNode.ResetUniqueVariableIDs();
            foreach(var shaderNode in ShaderNodes)
            {
                shaderNode.ResetVisit();
            }
        }
        
        public void TraverseDescriptionGraph(ShaderNode shaderNode, NodeVisitor visitor)
        {
            ResetNodes();
            TraverseGraph(shaderNode, visitor);

            shaderNode.Visit(GenerationMode, visitor);
        }
        public void TraverseGraph(ShaderNode shaderNode, NodeVisitor visitor)
        {
            var inputs = shaderNode.Inputs;
            foreach (var input in inputs.Values)
            {
                var inputNode = input.Node;
                if (inputNode.visited)
                {
                    // copy
                    if (visitor is DescriptionVisitor)
                    {
                        shaderNode.VariableNames[input.b] = inputNode.VariableNames[input.a];
                        shaderNode.Ports[input.b].Type = inputNode.Ports[input.a].Type;
                    }
                    continue;
                }

                TraverseGraph(inputNode, visitor);

                inputNode.Visit(GenerationMode, visitor);
                {
                    // copy
                    if (visitor is DescriptionVisitor)
                    {
                        shaderNode.VariableNames[input.b] = inputNode.VariableNames[input.a];
                        shaderNode.Ports[input.b].Type = inputNode.Ports[input.a].Type;
                    }
                }

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

            foreach (var function in pass.functions.Values)
            {
                var lines = function.Split('\n');
                foreach (var line in lines)
                {
                    _sb.AppendLine(line);
                }
            }

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
