
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.ObjectChangeEventStream;
using static ZSG.UnlitBuildTarget;

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
            ShaderNode.UniqueVariableID = 0;

            var shaderNodes = ShaderGraphView.graphElements.Where(x => x is ShaderNode).Cast<ShaderNode>().ToList();
            var v = (TemplateOutput)shaderNodes.Find(x => x.GetType() == target.VertexDescription);
            var f = (TemplateOutput)shaderNodes.Find(x => x.GetType() == target.SurfaceDescription);

            for (int i = 0; i < passBuilders.Count; i++)
            {
                int passIndex = i;

                var pass = passBuilders[i];
                var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex);
                var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, passIndex);

                int[] portsMask = pass.Ports;
                TraverseGraph(v, vertexVisitor, portsMask);
                TraverseGraph(f, fragmentVisitor, portsMask);

                v.BuilderVisit(vertexVisitor);
                f.BuilderVisit(fragmentVisitor);

                v.VisitTemplate(vertexVisitor, portsMask);
                f.VisitTemplate(fragmentVisitor, portsMask);
            }

        }

        public void Build(ShaderNode shaderNode)
        {
            ShaderNode.UniqueVariableID = 0;
            //var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex);
            var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, 0);
            TraverseGraph(shaderNode, fragmentVisitor);
            shaderNode.BuilderVisit(fragmentVisitor);

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

        public static void GeneratePreview(ShaderGraphView graphView, ShaderNode shaderNode, bool log = false)
        {
            if (!shaderNode.EnablePreview)
            {
                return;
            }

            var shaderBuilder = new ShaderBuilder(GenerationMode.Preview, graphView);
            shaderBuilder.shaderName = "Hidden/ZSGPreviews/" + shaderNode.viewDataKey;
            var pass = new PassBuilder("FORWARD", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitVertex.hlsl", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitFragment.hlsl",
                UnlitVertexDescription.POSITION,
                UnlitSurfaceDescription.COLOR
                );
            shaderBuilder.AddPass(pass);
            pass.varyings.RequirePositionCS();
            //pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");
            //pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
            //pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

            pass.pragmas.Add("#define PREVIEW");
            pass.varyings.RequireCustom(1, "float(1)");

            pass.pragmas.Add("#include \"UnityShaderVariables.cginc\"");
                //basePass.pragmas.Add("#define _WorldSpaceCameraPos float3(0, 0, -2.23)");
            pass.pragmas.Add("#define PREVIEW");

            if (shaderNode.preview3D)
            {
                pass.pragmas.Add("#define PREVIEW3D");
            }
            pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/Editor/Targets/Graph.hlsl\"");
            pass.pragmas.Add("#include \"UnityCG.cginc\"");

            var tags = shaderBuilder.subshaderTags;
            tags.Add("Queue", "Transparent");
            tags.Add("RenderType", "Transparent");
            var states = pass.renderStates;
            states.Add("ZWrite", "Off");
            states.Add("Blend", "SrcAlpha OneMinusSrcAlpha");

            PortBindings.GetBindingString(pass, ShaderStage.Fragment, 2, PortBinding.UV0);


            //shaderBuilder.passBuilders[0].renderStates.Add("Cull", "Off");
            shaderBuilder.Build(shaderNode);

            string result = shaderBuilder.ToString();

            var shader = ShaderUtil.CreateShaderAsset(result);
            shaderNode.previewDrawer?.SetShader(shader);

            if (log) UnityEngine.Debug.Log(shaderBuilder);
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

        public void TraverseGraph(ShaderNode shaderNode, NodeVisitor visitor, int[] ports = null)
        {
            foreach (var port in shaderNode.Inputs)
            {
                if (ports is not null && !ports.Contains(port.GetPortID()))
                {
                    continue;
                }

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
                inputNode.BuilderVisit(visitor);
                visitedNodes.Add(inputNode.viewDataKey);

                if (GenerationMode == GenerationMode.Preview)
                {
                    inputNode.UpdateGraphView();
                }
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
