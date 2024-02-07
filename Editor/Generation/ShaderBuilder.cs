
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZSG.Nodes.PortType;

namespace ZSG
{
    public class ShaderBuilder
    {


        public ShaderBuilder(GenerationMode generationMode, ShaderGraphView shaderGraphView, BuildTarget target = BuildTarget.StandaloneWindows64)
        {
            GenerationMode = generationMode;
            ShaderGraphView = shaderGraphView;

            var data = shaderGraphView.graphData;
            shaderName = data.shaderName;
            BuildTarget = target;
        }

        private ShaderStringBuilder _sb;

        public BuildTarget BuildTarget { get; private set; }
        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public List<PassBuilder> passBuilders = new();

        private HashSet<string> visitedNodes = new HashSet<string>();
        public List<PropertyDescriptor> properties = new();

        public GenerationMode GenerationMode { get; }
        public SerializableGraph SerializableGraph { get; }
        public ShaderGraphView ShaderGraphView { get; }

        public Dictionary<string, Texture> _nonModifiableTextures = new();
        public Dictionary<string, Texture> _defaultTextures = new();
        public HashSet<string> dependencies = new();

        public void AddPass(PassBuilder passBuilder)
        {
            passBuilder.generationMode = GenerationMode;
            passBuilders.Add(passBuilder);
        }

        public void BuildTemplate()
        {
            //ShaderNode.UniqueVariableID = 0; parallel import might not work with this

            var shaderNodes = ShaderGraphView.graphElements.Where(x => x is ShaderNode).Cast<ShaderNode>().ToList();
            var target = (TemplateOutput)ShaderGraphView.graphElements.First(x => x is TemplateOutput);

            var graphData = ShaderGraphView.graphData;
            customEditor = graphData.customEditor;
            fallback = graphData.fallback;


            target.OnBeforeBuild(this);

            for (int i = 0; i < passBuilders.Count; i++)
            {
                int passIndex = i;

                var pass = passBuilders[i];
                var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex);
                var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, passIndex);

                int[] portsMask = pass.Ports;
                var vertexPorts = portsMask.Intersect(target.VertexPorts).ToArray();
                var fragmentPorts = portsMask.Intersect(target.FragmentPorts).ToArray();

                visitedNodes.Clear();
                TraverseGraph(target, vertexVisitor, vertexPorts);
                visitedNodes.Clear();
                TraverseGraph(target, fragmentVisitor, fragmentPorts);

                target.BuilderVisit(vertexVisitor, vertexPorts);
                target.BuilderVisit(fragmentVisitor, fragmentPorts);

                target.VisitTemplate(vertexVisitor, vertexPorts);
                target.VisitTemplate(fragmentVisitor, fragmentPorts);
            }

            target.OnAfterBuild(this);
        }

        public void Build(ShaderNode shaderNode)
        {
            //ShaderNode.UniqueVariableID = 0;
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
                    bool previewCapable = shaderNode.PortData[id].Type is Float;
                    if (previewCapable)
                    {
                        var cast = shaderNode.Cast(id, 4, false);
                        sb.Add("output.Color = " + cast.Name + ";");
                    }
                    else
                    {
                        sb.Add("output.Color = 0;");
                    }

                    break; // only first port for preview
                }
            }

        }

        const string VertexPreview = "Packages/com.z3y.myshadergraph/Editor/Targets/Preview/Vertex.hlsl";
        const string FragmentPreview = "Packages/com.z3y.myshadergraph/Editor/Targets/Preview/Fragment.hlsl";

        public static void GeneratePreview(ShaderGraphView graphView, ShaderNode shaderNode, bool log = false)
        {
            var shaderBuilder = new ShaderBuilder(GenerationMode.Preview, graphView);
            shaderBuilder.shaderName = "Hidden/ZSGPreviews/" + shaderNode.viewDataKey;
            var pass = new PassBuilder("FORWARD", VertexPreview, FragmentPreview);

            shaderBuilder.AddPass(pass);
            pass.varyings.RequirePositionCS();

            pass.pragmas.Add("#define PREVIEW");
            pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");

            var tags = shaderBuilder.subshaderTags;
            tags.Add("Queue", "Transparent");
            tags.Add("RenderType", "Transparent");
            var states = pass.renderStates;
            states.Add("ZWrite", "Off");
            states.Add("Blend", "SrcAlpha OneMinusSrcAlpha");

            PortBindings.GetBindingString(pass, ShaderStage.Fragment, 2, PortBinding.UV0);


            //shaderBuilder.passBuilders[0].renderStates.Add("Cull", "Off");
            shaderBuilder.Build(shaderNode);

            if (shaderNode._inheritedPreview == PreviewType.Preview3D)
            {
                pass.pragmas.Insert(0, "#define PREVIEW3D");
            }

            if (shaderNode.DisablePreview)
            {
                return;
            }

            if (!shaderNode._previewDisabled)
            {
                string result = shaderBuilder.ToString();
                shaderNode.previewDrawer?.SetShader(result);
                shaderNode.UpdatePreviewMaterial();
                if (log) Debug.Log(shaderBuilder);
            }
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
            _sb.AppendLine(string.IsNullOrEmpty(customEditor) ? "CustomEditor \"ZSG.DefaultInspector\"" : "CustomEditor \"" + customEditor + "\"");

            _sb.UnIndent();

            // UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].surfaceDescription) == string.Join(' ', passBuilders[1].surfaceDescription));
            // UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].vertexDescription) == string.Join(' ', passBuilders[1].vertexDescription));

            return _sb.ToString();
        }

        private void AppendProperties()
        {
            if (GenerationMode == GenerationMode.Preview)
            {
                var allProperties = passBuilders.SelectMany(x => x.properties);

                foreach (var property in allProperties)
                {
                    if (property.ShouldDeclare())
                        _sb.AppendLine(property.GetPropertyDeclaration(GenerationMode.Preview));
                }
            }
            else
            {
                foreach (var property in properties.Union(ShaderGraphView.graphData.properties))
                {
                    if (property.ShouldDeclare())
                        _sb.AppendLine(property.GetPropertyDeclaration(GenerationMode.Final));

                    if (property.IsTextureType && property.DefaultTextureValue is Texture texture)
                    {
                        var referenceName = property.GetReferenceName(GenerationMode.Final);
                        if (property.defaultAttributes.HasFlag(MaterialPropertyAttribute.NonModifiableTextureData))
                        {
                            _nonModifiableTextures.Add(referenceName, texture);
                        }
                        else
                        {
                            _defaultTextures[referenceName] = texture;
                        }
                    }
                }
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
                pass.pragmas.AddRange(ShaderGraphView.graphData.include.Split('\n'));
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
