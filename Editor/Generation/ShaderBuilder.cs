using System.Collections.Generic;
using z3y.ShaderGraph.Nodes;

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
        }

        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public HashSet<string> properties = new();
        public List<PassBuilder> passBuilders = new();

        private ShaderStringBuilder _sb;

        public GenerationMode GenerationMode { get; }
        public SerializableGraph SerializableGraph { get; }
        public ShaderGraphView ShaderGraphView { get; }

        public void AddPass(PassBuilder passBuilder)
        {
            passBuilders.Add(passBuilder);
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

        public void Build(BuildTarget target)
        {
            ShaderNode.ResetUniqueVariableIDs();

            var v = (TemplateOutput)ShaderNodes.Find(x => x.GetType() == target.VertexDescription);
            var f = (TemplateOutput)ShaderNodes.Find(x => x.GetType() == target.SurfaceDescription);

            ResetNodes();

            var vertexVisitors = new List<NodeVisitor>
            {
                new PropertyVisitor(this)
            };

            var fragmentVisitors = new List<NodeVisitor>
            {
                new PropertyVisitor(this)
            };

            for (int i = 0; i < passBuilders.Count; i++)
            {
                int passIndex = i;
                vertexVisitors.Add(new DescriptionVisitor(this, ShaderStage.Vertex, passIndex));
                fragmentVisitors.Add(new DescriptionVisitor(this, ShaderStage.Fragment, passIndex));
                vertexVisitors.Add(new FunctionVisitor(this, passIndex));
                fragmentVisitors.Add(new FunctionVisitor(this, passIndex));
            }

            TraverseGraph(v, vertexVisitors);
            v.Visit(vertexVisitors);

            TraverseGraph(f, fragmentVisitors);
            f.Visit(fragmentVisitors);


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

        private void CopyPort(ShaderNode shaderNode, ShaderNode inputNode, NodeConnection input)
        {
            shaderNode.VariableNames[input.b] = inputNode.VariableNames[input.a];
            shaderNode.Ports[input.b].Type = inputNode.Ports[input.a].Type;
        }

        public void TraverseGraph(ShaderNode shaderNode, IEnumerable<NodeVisitor> visitors)
        {
            var inputs = shaderNode.Inputs;
            foreach (var input in inputs.Values)
            {
                var inputNode = input.Node;
                if (inputNode.visited)
                {
                    CopyPort(shaderNode, inputNode, input);
                    continue;
                }

                TraverseGraph(inputNode, visitors);

                inputNode.Visit(visitors);
                {
                    CopyPort(shaderNode, inputNode, input);
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

            UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].surfaceDescription) == string.Join(' ', passBuilders[1].surfaceDescription));
            UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].vertexDescription) == string.Join(' ', passBuilders[1].vertexDescription));

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

            _sb.AppendLine("// CBUFFER");
            foreach (var property in properties)
            {
                string propertyName = property;
               _sb.AppendLine(propertyName);
            }
            _sb.AppendLine("// CBUFFER END");
            _sb.AppendLine();

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
