
using System.Collections.Generic;
using System.Linq;

namespace ZSG
{
    public class ShaderBuilder
    {

        public ShaderBuilder(GenerationMode generationMode, SerializableGraph serializableGraph, ShaderGraphView shaderGraphView)
        {
            GenerationMode = generationMode;
            SerializableGraph = serializableGraph;
            ShaderGraphView = shaderGraphView;
            DeserializeAndMapGuids();
            //FillConnections();

            var data = serializableGraph.data;
            shaderName = data.shaderName;
        }

        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
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

/*        private void FillConnections()
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
        }*/

        public void Build(BuildTarget target)
        {
            //var v = (TemplateOutput)ShaderNodes.Find(x => x.GetType() == target.VertexDescription);
            //var f = (TemplateOutput)ShaderNodes.Find(x => x.GetType() == target.SurfaceDescription);

/*
            for (int i = 0; i < passBuilders.Count; i++)
            {
                ResetNodes();
                int passIndex = i;

                var pass = passBuilders[i];
                var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex, "VertexDescription");
                var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, passIndex, "SurfaceDescription");

                TraverseGraphBegin(v, vertexVisitor, pass.Ports);
                TraverseGraphBegin(f, fragmentVisitor, pass.Ports);
            }

            UpdateAllPreviews();*/
        }

        public void BuildPreview(string guid)
        {
           /* ResetNodes();

            var targetNode = GuidToNode[guid];

            var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, 0, "SurfaceDescription");

            TraverseGraph(targetNode, fragmentVisitor);

            targetNode.Visit(fragmentVisitor);

            if (ShaderGraphView is not null)
            {
                ShaderGraphView.UpdateGraphView(targetNode);
            }

            var sb = passBuilders[0].surfaceDescription;
            var str = passBuilders[0].surfaceDescriptionStruct;
            str.Add("float4 Color;");
            foreach (var port in targetNode.Ports)
            {
                if (port.Direction == PortDirection.Output)
                {
                    var t = (Float)targetNode.Ports.GetByID(port.ID).Type;
                    var cast = targetNode.Cast(t.components, "float", 4, targetNode.VariableNames[port.ID]);
                    sb.Add("output.Color = " + cast + ";");
                    break;
                }
            }

            sb.Add("return output;");

            if (ShaderGraphView is not null)
            {
                ShaderGraphView.UpdateGraphView(targetNode);
            }*/
        }

/*        private void ResetNodes()
        {
            ShaderNode.ResetUniqueVariableIDs();
            foreach (var shaderNode in ShaderNodes)
            {
                shaderNode.ResetVisit();
            }
        }*/

/*        public void UpdateAllPreviews()
        {
            if (ShaderGraphView is null)
            {
                return;
            }

            foreach (var shaderNode in ShaderNodes)
            {
                UpdatePreview(SerializableGraph, NodeToSerializableNode[shaderNode]);
            }
        }*/
/*
        public void UpdatePreview(SerializableGraph serializableGraph, SerializableNode targetNode)
        {
            var node = (ShaderNodeVisualElement)ShaderGraphView.GetNodeByGuid(targetNode.guid);
            var builder = new ShaderBuilder(GenerationMode.Preview, serializableGraph, ShaderGraphView);
            builder.shaderName = "Hidden/SGPreview/" + targetNode.guid;
            var target = new UnlitBuildTarget();
            target.BuilderPassthourgh(builder);
            builder.BuildPreview(targetNode.guid);

            //UnityEngine.Debug.Log(builder.ToString());

            var shader = ShaderUtil.CreateShaderAsset(builder.ToString());

            node.previewDrawer.Initialize(shader);
            node.UpdatePreview();
        }*/

/*        private void CopyPort(ShaderNode shaderNode, ShaderNode inputNode, NodeConnection input)
        {
            shaderNode.VariableNames[input.b] = inputNode.VariableNames[input.a];
            shaderNode.Ports.GetByID(input.b).Type = inputNode.Ports.GetByID(input.a).Type;
        }*/
/*
        public void TraverseGraphBegin(TemplateOutput templateOutput, NodeVisitor visitor, int[] ports)
        {
            var inputs = templateOutput.Inputs;
            foreach (var input in inputs.Values)
            {
                if (!Array.Exists(ports, x => x == input.GetInputIDForThisNode()))
                {
                    continue;
                }

                var inputNode = input.Node;

                if (inputNode.visited)
                {
                    CopyPort(templateOutput, inputNode, input);
                    continue;
                }

                TraverseGraph(inputNode, visitor);

                inputNode.Visit(visitor);
                {
                    CopyPort(templateOutput, inputNode, input);
                }

                inputNode.visited = true;
            }

            templateOutput.VisitTemplate(visitor, ports);
        }*/

/*        public void TraverseGraph(ShaderNode shaderNode, NodeVisitor visitor)
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

                TraverseGraph(inputNode, visitor);

                inputNode.Visit(visitor);
                {
                    CopyPort(shaderNode, inputNode, input);
                }

                inputNode.visited = true;

                ShaderGraphView?.UpdateGraphView(inputNode);
            }
        }*/

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
                    AppendTags(_sb, subshaderTags);

                    AppendPasses();
                }
                _sb.UnIndent();
            }

            _sb.AppendLine(string.IsNullOrEmpty(fallback) ? "// Fallback None" : "Fallback \"" + fallback + "\"");
            _sb.AppendLine(string.IsNullOrEmpty(customEditor) ? "// CustomEditor None" : "CustomEditor \"" + customEditor + "\"");

            _sb.UnIndent();

            // UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].surfaceDescription) == string.Join(' ', passBuilders[1].surfaceDescription));
            // UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].vertexDescription) == string.Join(' ', passBuilders[1].vertexDescription));

            return _sb.ToString();
        }

        private void AppendProperties()
        {
            var allProperties = passBuilders.SelectMany(x => x.properties);

            foreach (var property in allProperties)
            {
                _sb.AppendLine(property.ToString());
            }
        }

        public static void AppendTags(ShaderStringBuilder sb, Dictionary<string, string> tags)
        {
            sb.AppendLine("Tags");

            sb.Indent();
            foreach (var tag in tags)
            {
                sb.AppendLine('"' + tag.Key + "\" = \"" + tag.Value + '"');
            }
            sb.UnIndent();
        }

        private void AppendPasses()
        {
            foreach (var pass in passBuilders)
            {
                _sb.AppendLine("Pass");
                _sb.Indent();
                {
                    pass.AppendPass(_sb);
                }
                _sb.UnIndent();
            }
        }

    }
}
