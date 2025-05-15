
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Graphlit.Nodes.PortType;
using System;

namespace Graphlit
{
    public class ShaderBuilder
    {

        public ShaderBuilder(GenerationMode generationMode, ShaderGraphView shaderGraphView, BuildTarget target = BuildTarget.StandaloneWindows64, bool unlocked = false)
        {
            GenerationMode = generationMode;
            ShaderGraphView = shaderGraphView;

            var data = shaderGraphView.graphData;
            shaderName = data.shaderName;
            BuildTarget = target;
            this.unlocked = unlocked;
        }

        //private RegisterVariableNode[] autoWireNodes;
        private ShaderStringBuilder _sb;
        public bool unlocked = false;
        public BuildTarget BuildTarget { get; private set; }
        public string shaderName;
        public string fallback;
        public string customEditor;
        public Dictionary<string, string> subshaderTags = new();
        public List<PassBuilder> passBuilders = new();

        private HashSet<string> visitedNodes = new HashSet<string>();
        public List<PropertyDescriptor> properties = new();
        public List<PropertyDescriptor> generatedTextures = new();
        public int generatedTextureResolution = 512;

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

        public void BuildTemplate(TemplateOutput target)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var shaderNodes = ShaderGraphView.cachedNodesForBuilder;

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

            //Debug.Log("Build Tempalte: " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        public void Build(ShaderNode shaderNode)
        {
            var fragmentVisitor = new NodeVisitor(this, ShaderStage.Fragment, 0);
            TraverseGraph(shaderNode, fragmentVisitor);
            shaderNode.BuilderVisit(fragmentVisitor);

            // shaderNode.UpdateGraphView();

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

        public const string VertexPreview = "Packages/com.z3y.graphlit/Editor/Targets/Preview/Vertex.hlsl";
        public const string FragmentPreview = "Packages/com.z3y.graphlit/Editor/Targets/Preview/Fragment.hlsl";

        public static void GenerateUnifiedPreview(ShaderGraphView graphView, ShaderNode node, List<ShaderNode> rightNodes, bool log = false, bool allNodes = false)
        {
            var affectedNodes = (new List<ShaderNode> { node }).Union(rightNodes).ToList();

            var shaderBuilder = new ShaderBuilder(GenerationMode.Preview, graphView)
            {
                shaderName = "Hidden/GraphlitPreview"
            };
            var pass = new PassBuilder("FORWARD", VertexPreview, FragmentPreview);

            shaderBuilder.AddPass(pass);
            pass.varyings.RequirePositionCS();

            pass.pragmas.Add("#pragma skip_optimizations d3d11");
            pass.pragmas.Add("#define PREVIEW");
            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
            pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Preview.hlsl\"");

            var tags = shaderBuilder.subshaderTags;
            tags.Add("Queue", "Transparent");
            tags.Add("RenderType", "Transparent");
            var states = pass.renderStates;
            states.Add("ZWrite", "Off");
            states.Add("Blend", "SrcAlpha OneMinusSrcAlpha");

            states.Add("Stencil", "{Ref 1\nComp Equal}");

            PortBindings.GetBindingString(pass, ShaderStage.Fragment, 2, PortBinding.UV0);


            //shaderBuilder.passBuilders[0].renderStates.Add("Cull", "Off");
            var fragmentVisitor = new NodeVisitor(shaderBuilder, ShaderStage.Fragment, 0);

            shaderBuilder.TraverseGraph(node, fragmentVisitor);
            node.BuilderVisit(fragmentVisitor);
            shaderBuilder.visitedNodes.Add(node.viewDataKey);

            foreach (var rightNode in rightNodes)
            {
                shaderBuilder.TraverseGraph(rightNode, fragmentVisitor);
                if (!shaderBuilder.visitedNodes.Contains(rightNode.viewDataKey))
                {
                    shaderBuilder.visitedNodes.Add(rightNode.viewDataKey);
                    rightNode.BuilderVisit(fragmentVisitor);
                }
            }

            if (allNodes)
            {
                var nodes = graphView.GetElementsGuidDictionary<ShaderNode>();
                foreach (var guid in shaderBuilder.visitedNodes)
                {
                    var n = nodes[guid];
                    if (!affectedNodes.Contains(n))
                    {
                        affectedNodes.Add(n);
                    }
                }
            }


            var sb = pass.surfaceDescription;
            var outputStruct = pass.surfaceDescriptionStruct;
            outputStruct.Add("float4 Color;");

            sb.Add("[forcecase]");
            sb.Add("switch(_PreviewID) {");
            int previewId = 0;
            foreach (var x in affectedNodes)
            {
                if (x._previewDisabled || x.DisablePreview)
                {
                    continue;
                }

                foreach (var port in x.Outputs)
                {
                    int id = port.GetPortID();
                    if (x.PortData[id].Type is Float @float)
                    {
                        var cast = x.Cast(id, 4, false);
                        if (@float.dimensions != 4)
                        {
                            sb.Add($"case {previewId}: output.Color = {cast.Name}; output.Color.a = 1.0; break;");
                        }
                        else
                        {
                            sb.Add($"case {previewId}: output.Color = {cast.Name}; break;");
                        }
                    }
                    else
                    {
                        sb.Add($"case {previewId}: output.Color = float4(0,0,0,1); break;");
                    }

                    break; // only first port for preview
                }

                x.previewDrawer.previewId = previewId;

                previewId++;
            }
            sb.Add("}");


            if (previewId <= 0)
            {
                return;
            }

            string result = shaderBuilder.ToString();

            //Debug.Log(result);

            var shader = ShaderUtil.CreateShaderAsset(result);
            var shaderRef = new ObjectRc<Shader>(shader);

            foreach (var x in affectedNodes)
            {
                if (x._previewDisabled || x.DisablePreview)
                {
                    continue;
                }

                x.previewDrawer.SetShader(shaderRef);
                x.UpdatePreviewMaterial();
            }
        }

        public static void GenerateAllPreviews(ShaderGraphView graphView, List<ShaderNode> nodes = null)
        {
            graphView.UpdateCachedNodesForBuilder();
            var affectedNodes = graphView.cachedNodesForBuilder;
            if (nodes is not null)
            {
                affectedNodes = nodes;
            }

            var endNodes = new List<ShaderNode>();
            foreach (var node in affectedNodes)
            {
                bool include = true;
                foreach (var output in node.Outputs)
                {
                    if (output.connected)
                    {
                        include = false;
                    }
                }
                if (include)
                {
                    endNodes.Add(node);
                }
            }

            foreach (var node in endNodes)
            {
                GenerateUnifiedPreview(graphView, node, new List<ShaderNode>(), false, true);
            }
        }

        public static void GeneratePreviewFromEdge(ShaderGraphView graphView, Edge edge, bool toRemove)
        {
            var node = (ShaderNode)edge.input.node;

            if (toRemove)
            {
                var input = edge.input;
                var output = edge.output;
                input.Disconnect(edge);
                output.Disconnect(edge);
            }

            node.GeneratePreviewForAffectedNodes();
        }

        public static void GetConnectedNodesFromEdgeRight(List<ShaderNode> nodes, Edge edge, HashSet<string> addedNodes)
        {
            var connectedNode = (ShaderNode)edge.input.node;
            var guid = connectedNode.viewDataKey;

            if (!addedNodes.Contains(guid))
            {
                nodes.Add(connectedNode);
                addedNodes.Add(guid);

                foreach (var port in connectedNode.Outputs)
                {
                    foreach (var connection in port.connections)
                    {
                        GetConnectedNodesFromEdgeRight(nodes, connection, addedNodes);
                    }
                }
            }
        }

        public void TraverseGraph(ShaderNode shaderNode, NodeVisitor visitor, int[] ports = null)
        {
            // skip unneded ports for shadowcaster
            if (visitor._shaderBuilder.passBuilders[visitor.Pass].name == "SHADOWCASTER" && shaderNode is BlendFinalColorNode)
            {
                ports = new int[] { BlendFinalColorNode.METALLIC, BlendFinalColorNode.IN_ALPHA };
            }

            foreach (var port in shaderNode.Inputs)
            {
                if (ports is not null && !ports.Contains(port.GetPortID()))
                {
                    continue;
                }

                var connections = port.connections.ToArray();
                if (connections.Length == 0)
                {
                    continue;
                }
                Edge input = connections[0];

                var inputNode = (ShaderNode)input.output.node;

                if (visitedNodes.Contains(inputNode.viewDataKey))
                {
                    continue;
                }

                TraverseGraph(inputNode, visitor);

                //UnityEngine.Debug.Log("Visiting " + inputNode.viewDataKey);
                inputNode.BuilderVisit(visitor);
                visitedNodes.Add(inputNode.viewDataKey);

                /*if (GenerationMode == GenerationMode.Preview && !unlocked)
                {
                    inputNode.UpdateGraphView();
                }*/
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
            _sb.AppendLine(string.IsNullOrEmpty(customEditor) ? "CustomEditor \"Graphlit.ShaderInspector\"" : "CustomEditor \"" + customEditor + "\"");

            _sb.UnIndent();

            // UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].surfaceDescription) == string.Join(' ', passBuilders[1].surfaceDescription));
            // UnityEngine.Debug.Assert(string.Join(' ', passBuilders[0].vertexDescription) == string.Join(' ', passBuilders[1].vertexDescription));

            return _sb.ToString();
        }

        private void AppendProperties()
        {
            if (unlocked)
            {
                _sb.AppendLine("_GraphlitPreviewEnabled(\"LIVE PREVIEW ENABLED\", Float) = 1");
            }
            if (GenerationMode == GenerationMode.Preview)
            {
                var allProperties = passBuilders.SelectMany(x => x.properties).ToList();

                var addedProps = new HashSet<string>();

                if (unlocked)
                {
                    foreach (var property in properties.Union(ShaderGraphView.graphData.properties).Distinct())
                    {
                        if (property.ShouldDeclare())
                        {
                            _sb.AppendLine(property.GetPropertyDeclaration(GenerationMode.Final));
                            addedProps.Add(property.GetReferenceName(GenerationMode.Final));
                        }
                    }
                }

                foreach (var property in allProperties)
                {
                    if (unlocked && property.IsTextureType)
                    {
                        continue;
                    }
                    if (unlocked && addedProps.Contains(property.GetReferenceName(GenerationMode.Preview)))
                    {
                        continue;
                    }
                    if (property.ShouldDeclare())
                    {
                        _sb.AppendLine(property.GetPropertyDeclaration(GenerationMode.Preview));
                        addedProps.Add(property.GetReferenceName(GenerationMode.Preview));
                    }
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

                _sb.AppendLine("[HideInInspector]__reset(\"\", Float) = 1");
                _sb.AppendLine("[HideInInspector]_GraphlitMaterial(\"\", Float) = 1");

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
            var graphData = ShaderGraphView.graphData;
            if (graphData.depthFillPass)
            {
                _sb.AppendLine("Pass { ZWrite On ColorMask 0 }");
            }
            if (grabpass && SupportsGrabpas)
            {
                _sb.AppendLine("GrabPass { Tags { \"LightMode\" = \"GrabPass\" } \"_CameraOpaqueTexture\" }");
            }
            var outline = graphData.outlinePass;

            // property union for srp batcher
            var propertyUnion = new List<PropertyDescriptor>();
            var addedProperties = new HashSet<string>();
            foreach (var pass in passBuilders)
            {
                foreach (var property in pass.properties)
                {
                    var referenceName = property.GetReferenceName(pass.generationMode);

                    if (addedProperties.Contains(referenceName))
                    {
                        continue;
                    }

                    addedProperties.Add(referenceName);
                    propertyUnion.Add(property);
                }
            }

            passBuilders.ForEach(x => x.properties = propertyUnion);


            for (int i = 0; i < passBuilders.Count; i++)
            {
                PassBuilder pass = passBuilders[i];
                pass.pragmas.AddRange(graphData.include.Split('\n'));
                if (outline == GraphData.OutlinePassMode.EnabledEarly && i == 0)
                {
                    //pass.name = "FORWARD_OUTLINE";
                    string cull = pass.renderStates["Cull"];
                    pass.outlinePass = true;
                    pass.renderStates["Cull"] = "Front";
                    //pass.tags["LightMode"] = "Always";
                    AppendPass(pass, graphData);
                    pass.outlinePass = false;
                    pass.renderStates["Cull"] = cull;
                }

                AppendPass(pass, graphData);

                if (outline == GraphData.OutlinePassMode.Enabled && i == 0)
                {
                    //pass.name = "FORWARD_OUTLINE";
                    pass.outlinePass = true;
                    pass.renderStates["Cull"] = "Front";
                    //pass.tags["LightMode"] = "Always";
                    AppendPass(pass, graphData);
                }
            }
        }

        private void AppendPass(PassBuilder pass, GraphData graphData)
        {
            _sb.AppendLine("Pass");
            _sb.Indent();
            {
                pass.AppendPass(_sb, graphData);
            }
            _sb.UnIndent();
        }
    }
}
