
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Graphlit.Nodes.PortType;
using System.IO;
using UnityEditor.Hardware;

namespace Graphlit
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
        public List<PropertyDescriptor> subgraphProperties = new();

        public GenerationMode GenerationMode { get; private set; }
        public SerializableGraph SerializableGraph { get; }
        public ShaderGraphView ShaderGraphView { get; }

        public Dictionary<string, Texture> _nonModifiableTextures = new();
        public Dictionary<string, Texture> _defaultTextures = new();
        public HashSet<string> dependencies = new();

        bool SupportsGrabpas => BuildTarget != BuildTarget.Android;
        public bool grabpass = false;

        const string StencilProperties = @"
        [Foldout]_StencilProperties(""Stencil"", Float) = 0
        [IntRange]_StencilRef(""Reference"", Range(0, 255)) = 0
        [IntRange]_StencilReadMask(""Read Mask"", Range(0, 255)) = 255
        [IntRange]_StencilWriteMask(""Write Mask"", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilCompBack(""Compare Function Back"", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPassBack(""Pass Back"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFailBack(""Fail Back"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFailBack(""ZFail Back"", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilCompFront(""Compare Function Front"", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPassFront(""Pass Front"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFailFront(""Fail Front"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFailFront(""ZFail Front"", Float) = 0";

        const string StencilPropertiesOutline = @"
        [Foldout]_OutlineStencilProperties(""Outline Stencil"", Float) = 0
        [IntRange]_OutlineStencilRef(""Reference"", Range(0, 255)) = 0
        [IntRange]_OutlineStencilReadMask(""Read Mask"", Range(0, 255)) = 255
        [IntRange]_OutlineStencilWriteMask(""Write Mask"", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)]_OutlineStencilCompBack(""Compare Function Back"", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilPassBack(""Pass Back"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilFailBack(""Fail Back"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilZFailBack(""ZFail Back"", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_OutlineStencilCompFront(""Compare Function Front"", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilPassFront(""Pass Front"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilFailFront(""Fail Front"", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_OutlineStencilZFailFront(""ZFail Front"", Float) = 0";

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

            var vrcTags = graphData.vrcFallbackTags.ToString();
            if (!string.IsNullOrEmpty(vrcTags))
            {
                subshaderTags["VRCFallback"] = vrcTags;
            }


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

      /*  public Dictionary<int, GeneratedPortData> BuildSubgraph(ShaderGraphView subgraphView, NodeVisitor vistor, string assetPath)
        {
            var sb = new ShaderBuilder(GenerationMode.Final, subgraphView, BuildTarget);
            sb.AddPass(passBuilders[vistor.Pass]);
            //var shaderNodes = subgraphView.graphElements.OfType<ShaderNode>();
            var target = (SubgraphOutputNode)subgraphView.graphElements.First(x => x is SubgraphOutputNode);
            subgraphView.graphData.precision = target.DefaultPrecision == Precision.Float ? GraphData.GraphPrecision.Float : GraphData.GraphPrecision.Half;

            var filename = Path.GetFileNameWithoutExtension(assetPath);

            string subgraphName = filename;
            var subgraphRefName = "_Subgraph_" + subgraphName.Replace(" ", "_").Replace("/", "_");
            if (subgraphView.graphData.properties.Count > 0 && !subgraphProperties.Exists(x => x.GetReferenceName(GenerationMode.Final) == subgraphRefName))
            {
                subgraphProperties.Add(new PropertyDescriptor(PropertyType.Float, subgraphName, subgraphRefName) { customAttributes = "[Foldout]"});
            }

            foreach ( var p in subgraphView.graphData.properties)
            {
                var toAdd = p.GetReferenceName(GenerationMode.Final);
                if (!(subgraphProperties.Exists(x => x.GetReferenceName(GenerationMode.Final) == toAdd) ||
                      properties.Exists(x => x.GetReferenceName(GenerationMode.Final) == toAdd)
                    ))
                {
                    subgraphProperties.Add(p);
                }
            }
            //subgraphView.uniqueID = ShaderGraphView.uniqueID;
            sb.Build(target);
            //ShaderGraphView.uniqueID = subgraphView.uniqueID;
            return target.subgraphResults;
        }*/

        public void Build(ShaderNode shaderNode)
        {
            //ShaderNode.UniqueVariableID = 0;
            //var vertexVisitor = new NodeVisitor(this, ShaderStage.Vertex, passIndex);
            var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, 0);
            TraverseGraph(shaderNode, fragmentVisitor);
            shaderNode.BuilderVisit(fragmentVisitor);

            shaderNode.UpdateGraphView();

            if (GenerationMode == GenerationMode.Preview && !shaderNode.DisablePreview)
            {
                var sb = passBuilders[0].surfaceDescription;
                var str = passBuilders[0].surfaceDescriptionStruct;
                str.Add("float4 Color;");
                foreach (var port in shaderNode.Outputs)
                {
                    int id = port.GetPortID();
                    if (shaderNode.PortData[id].Type is Float @float)
                    {
                        var cast = shaderNode.Cast(id, 4, false);
                        sb.Add("output.Color = " + cast.Name + ";");
                        if (@float.dimensions != 4)
                        {
                            sb.Add("output.Color.a = 1;");
                        }
                    }
                    else
                    {
                        sb.Add("output.Color = float4(0,0,0,1);");
                    }

                    break; // only first port for preview
                }
            }

        }

        const string VertexPreview = "Packages/com.z3y.graphlit/Editor/Targets/Preview/Vertex.hlsl";
        const string FragmentPreview = "Packages/com.z3y.graphlit/Editor/Targets/Preview/Fragment.hlsl";

        public static void GeneratePreview(ShaderGraphView graphView, ShaderNode shaderNode, bool log = false)
        {
            var shaderBuilder = new ShaderBuilder(GenerationMode.Preview, graphView);
            shaderBuilder.shaderName = "Hidden/GraphlitPreviews/" + shaderNode.viewDataKey;
            var pass = new PassBuilder("FORWARD", VertexPreview, FragmentPreview);

            shaderBuilder.AddPass(pass);
            pass.varyings.RequirePositionCS();

            pass.pragmas.Add("#pragma skip_optimizations d3d11");
            pass.pragmas.Add("#define PREVIEW");
            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");

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
            if (grabpass && SupportsGrabpas)
            {
                subshaderTags["Queue"] = "Transparent";
            }

            _sb = new ShaderStringBuilder();
            _sb.AppendLine("Shader \"" + shaderName + '"');

            _sb.Indent();
            {
                _sb.AppendLine("Properties");
                _sb.Indent();
                {
                    AppendProperties();
                    if (ShaderGraphView.graphData.stencil)
                    {
                        _sb.AppendLine(StencilProperties);
                        if (ShaderGraphView.graphData.outlinePass != GraphData.OutlinePassMode.Disabled)
                        {
                            _sb.AppendLine(StencilPropertiesOutline);
                        }
                    }
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
            _sb.AppendLine(string.IsNullOrEmpty(customEditor) ? "CustomEditor \"Graphlit.DefaultInspector\"" : "CustomEditor \"" + customEditor + "\"");

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
                foreach (var property in properties.Union(ShaderGraphView.graphData.properties).Union(subgraphProperties))
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
            if (grabpass && SupportsGrabpas)
            {
                _sb.AppendLine("GrabPass { Tags { \"LightMode\" = \"GrabPass\" } \"_CameraOpaqueTexture\" }");
            }
            var outline = ShaderGraphView.graphData.outlinePass;
            for (int i = 0; i < passBuilders.Count; i++)
            {
                PassBuilder pass = passBuilders[i];
                pass.pragmas.AddRange(ShaderGraphView.graphData.include.Split('\n'));
                if (outline == GraphData.OutlinePassMode.EnabledEarly && i == 0)
                {
                    //pass.name = "FORWARD_OUTLINE";
                    string cull = pass.renderStates["Cull"];
                    pass.outlinePass = true;
                    pass.renderStates["Cull"] = "Front";
                    //pass.tags["LightMode"] = "Always";
                    AppendPass(pass);
                    pass.outlinePass = false;
                    pass.renderStates["Cull"] = cull;
                }

                AppendPass(pass);

                if (outline == GraphData.OutlinePassMode.Enabled && i == 0)
                {
                    //pass.name = "FORWARD_OUTLINE";
                    pass.outlinePass = true;
                    pass.renderStates["Cull"] = "Front";
                    //pass.tags["LightMode"] = "Always";
                    AppendPass(pass);
                }
            }
        }

        private void AppendPass(PassBuilder pass)
        {
            _sb.AppendLine("Pass");
            _sb.Indent();
            {
                pass.AppendPass(_sb, ShaderGraphView.graphData.stencil);
            }
            _sb.UnIndent();
        }
    }
}
