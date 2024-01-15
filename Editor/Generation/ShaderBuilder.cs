
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;


namespace ZSG
{
    public class ShaderBuilder
    {


        public ShaderBuilder(GenerationMode generationMode, ShaderGraphView shaderGraphView)
        {
            GenerationMode = generationMode;
            ShaderGraphView = shaderGraphView;

            var data = shaderGraphView.graphData;
            shaderName = data.shaderName;
        }

        private ShaderStringBuilder _sb;

        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public List<PassBuilder> passBuilders = new();

        private HashSet<string> visitedNodes = new HashSet<string>();

        public GenerationMode GenerationMode { get; }
        public SerializableGraph SerializableGraph { get; }
        public ShaderGraphView ShaderGraphView { get; }

        public void AddPass(PassBuilder passBuilder)
        {
            passBuilders.Add(passBuilder);
        }

        public void Build(BuildTarget target)
        {
            /*
            var v = (TemplateOutput)ShaderGraphView.nodes.Find(x => x.GetType() == target.VertexDescription);
            //var f = (TemplateOutput)ShaderNodes.Find(x => x.GetType() == target.SurfaceDescription);


            for (int i = 0; i < passBuilders.Count; i++)
            {
                int passIndex = i;

                var pass = passBuilders[i];
                var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex, "VertexDescription");
                var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, passIndex, "SurfaceDescription");

                TraverseGraph(v, vertexVisitor);
                //TraverseGraphBegin(f, fragmentVisitor, pass.Ports);
            }
            */
        }

        public void Build(ShaderNode shaderNode)
        {
            ShaderNode.UniqueVariableID = 0;
            //var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex, "VertexDescription");
            var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, 0, "SurfaceDescription");
            TraverseGraph(shaderNode, fragmentVisitor);
            shaderNode.Generate(fragmentVisitor);
            shaderNode.UpdateGraphView();

            if (GenerationMode == GenerationMode.Preview)
            {
                var sb = passBuilders[0].surfaceDescription;
                var str = passBuilders[0].surfaceDescriptionStruct;
                str.Add("float4 Color;");
                foreach (var port in shaderNode.Outputs)
                {
                    int id = port.GetPortID();
                    var cast = shaderNode.Cast(id, 4, false);
                    sb.Add("output.Color = " + cast.Name + ";");
                    break;
                }

                sb.Add("return output;");
            }

        }

        public static void GeneratePreview(ShaderGraphView graphView, ShaderNode shaderNode)
        {
            if (!shaderNode.EnablePreview)
            {
                return;
            }

            var shaderBuilder = new ShaderBuilder(GenerationMode.Preview, graphView);
            shaderBuilder.shaderName = "Hidden/ZSGPreviews/" + shaderNode.viewDataKey;
            var target = new UnlitBuildTarget();
            target.BuilderPassthourgh(shaderBuilder);
            shaderBuilder.Build(shaderNode);

            if (shaderNode.previewDrawer is not null)
            {
                string result = shaderBuilder.ToString();
                var shader = ShaderUtil.CreateShaderAsset(result);
                shaderNode.previewDrawer.Initialize(shader);
            }

            //UnityEngine.Debug.Log(shaderBuilder);
        }

        public static void GenerateAllPreviews(ShaderGraphView graphView)
        {
            foreach (var graphElement in graphView.graphElements)
            {
                if (graphElement is ShaderNode shaderNode)
                {
                    GeneratePreview(graphView, shaderNode);
                }
            }
        }
        
        public static void GeneratePreviewFromEdge(ShaderGraphView graphView, Edge edge, bool toRemove)
        {
            var nodesToGenerate = new HashSet<ShaderNode>();
            GetConnectedNodesFromEdge(nodesToGenerate, edge);

            if (toRemove)
            {
                var input = edge.input;
                var output = edge.output;
                input.Disconnect(edge);
                output.Disconnect(edge);
            }


            foreach (var node in nodesToGenerate)
            {
                GeneratePreview(graphView, node);
            }
        }

        private static void GetConnectedNodesFromEdge(HashSet<ShaderNode> nodes, Edge edge)
        {
            var connectedNode = (ShaderNode)edge.input.node;

            if (!nodes.Contains(connectedNode))
            {
                nodes.Add(connectedNode);
                foreach (var port in connectedNode.Outputs)
                {
                    if (port.connected)
                    {
                        foreach (var connection in port.connections)
                        {
                            GetConnectedNodesFromEdge(nodes, connection);
                        }
                    }
                }
            }
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

        public void TraverseGraph(ShaderNode shaderNode, NodeVisitor visitor)
        {
            var inputPorts = shaderNode.Inputs.ToArray();
            foreach (var port in inputPorts)
            {
                if (!port.connections.Any())
                {
                    continue;
                }
                var input = port.connections.First();

                var inputNode = (ShaderNode)input.output.node;
                if (visitedNodes.Contains(inputNode.viewDataKey))
                {
                    continue;
                }

                TraverseGraph(inputNode, visitor);

                //UnityEngine.Debug.Log("Visiting " + inputNode.viewDataKey);
                inputNode.Generate(visitor);
                visitedNodes.Add(inputNode.viewDataKey);
                inputNode.UpdateGraphView();

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
