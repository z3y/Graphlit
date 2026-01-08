#if UNITY_EDITOR && NDMF_INCLUDED
using nadena.dev.ndmf;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.IO;

[assembly: ExportsPlugin(typeof(Graphlit.Optimizer.GraphlitMaterialCombiner))]

namespace Graphlit.Optimizer
{
    public class GraphlitMaterialCombiner : Plugin<GraphlitMaterialCombiner>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run("Add Vertex Stream", ctx =>
            {
                var optimizer = ctx.AvatarRootObject.GetComponent<GraphlitOptimizer>();
                if (optimizer)
                {
                    CombineMaterials(ctx);
                }
            });
        }

        public override string DisplayName => "Graphlit Optimizer";
        public override string QualifiedName => "com.z3y.graphlit.optimizer";

        struct DrawCall
        {
            public Material material;
            public Renderer renderer;
            public Mesh mesh;
            public int submeshIndex;
            public int baseVertex;
            public int vertexCount;
            public bool isSkinned;
        }

        void CombineMaterials(BuildContext ctx)
        {

            var renderers = ctx.AvatarRootObject.GetComponentsInChildren<Renderer>(true);


            var drawCalls = new List<DrawCall>();


            foreach (var renderer in renderers)
            {
                Mesh mesh;

                var filter = renderer.GetComponent<MeshFilter>();
                bool isSkinned = false;
                if (filter)
                {
                    mesh = filter.sharedMesh;
                }
                else
                {
                    var smr = renderer.GetComponent<SkinnedMeshRenderer>();
                    isSkinned = true;
                    mesh = smr.sharedMesh;
                }

                var meshCopy = Object.Instantiate(mesh);
                ctx.AssetSaver.SaveAsset(meshCopy);

                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                {

                    var submesh = mesh.GetSubMesh(submeshIndex);

                    var drawCall = new DrawCall
                    {
                        material = renderer.sharedMaterials[submeshIndex],
                        renderer = renderer,
                        mesh = meshCopy,
                        submeshIndex = submeshIndex,
                        baseVertex = submesh.baseVertex,
                        vertexCount = submesh.vertexCount,
                        isSkinned = isSkinned
                    };

                    drawCalls.Add(drawCall);
                }

            }

            MergeDrawCalls(ctx, drawCalls);

        }

        void MergeDrawCalls(BuildContext ctx, List<DrawCall> drawCalls)
        {
            if (drawCalls.Count < 1)
            {
                return;
            }

            var groups = drawCalls.GroupBy(x => x.material);


            var lockMaterials = new List<Material>();

            int materialID = 0;

            if (drawCalls.Count > 1)
            {
                foreach (var group in groups)
                {
                    // Debug.Log($"Material: {group.Key.name}, ID: {materialID}");

                    lockMaterials.Add(group.Key);

                    List<DrawCall> list = group.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        DrawCall draw = list[i];
                        var meshCopy = draw.mesh;

                        List<Vector3> uvs = new();
                        meshCopy.GetUVs(0, uvs);

                        float idAsFloat = materialID;

                        var indices = meshCopy.GetIndices(draw.submeshIndex);

                        for (int j = 0; j < indices.Length; j++)
                        {
                            Vector3 uv = uvs[indices[j]];
                            uv.z = idAsFloat;
                            uvs[indices[j]] = uv;
                        }

                        meshCopy.SetUVs(0, uvs);
                        ctx.AssetSaver.SaveAsset(meshCopy);
                        if (draw.isSkinned)
                        {
                            var smr = draw.renderer.GetComponent<SkinnedMeshRenderer>();
                            smr.sharedMesh = meshCopy;
                        }
                        else
                        {
                            var filter = draw.renderer.GetComponent<MeshFilter>();
                            filter.sharedMesh = meshCopy;
                        }
                    }

                    materialID++;
                }
            }
            else
            {
                lockMaterials.Add(drawCalls[0].material);
            }

            var shader = drawCalls[0].material.shader;
            var shaderPath = AssetDatabase.GetAssetPath(shader);
            var serializedGraph = GraphlitImporter.ReadGraphData(AssetDatabase.GUIDFromAssetPath(shaderPath).ToString());
            serializedGraph.data.unlocked = false;
            serializedGraph.data.enableLockMaterials = true;
            serializedGraph.data.lockMaterials = lockMaterials;
            // serializedGraph.data.materialIDThresholds = thresholds;

            int referenceCullMode = (int)lockMaterials[0].GetFloat("_Cull");
            for (int i = 1; i < lockMaterials.Count; i++)
            {
                if ((int)lockMaterials[i].GetFloat("_Cull") != referenceCullMode)
                {
                    serializedGraph.data.optimizerMixedCull = true;
                    break;
                }
            }

            var graphView = new ShaderGraphView(null, shaderPath);
            serializedGraph.PopulateGraph(graphView);
            graphView.UpdateCachedNodesForBuilder();

            var shaderNodes = graphView.cachedNodesForBuilder;
            var template = shaderNodes.OfType<TemplateOutput>().First();

            var builder = new ShaderBuilder(GenerationMode.Final, graphView);
            builder.BuildTemplate(template);

            var shaderString = builder.ToString();


            // GUIUtility.systemCopyBuffer = shaderString;
            // AssetDatabase.CreateAsset(new TextAsset(shaderString), "Assets/OptimizedShader.asset");
            File.WriteAllText("Assets/test.shader", shaderString);

            var optimizedShader = ShaderUtil.CreateShaderAsset(shaderString, false);

            template.ApplyDefaultTextures(builder, optimizedShader);

            ctx.AssetSaver.SaveAsset(optimizedShader);

            int renderQueue = drawCalls[0].material.renderQueue;

            var materialCopy = Object.Instantiate(drawCalls[0].material);
            materialCopy.shader = optimizedShader;

            materialCopy.enabledKeywords = new UnityEngine.Rendering.LocalKeyword[0];
            materialCopy.renderQueue = renderQueue;

            if (serializedGraph.data.optimizerMixedCull)
            {
                materialCopy.SetFloat("_Cull", 0);
            }
            ctx.AssetSaver.SaveAsset(materialCopy);

            materialCopy.name = "Graphlit Optimizer Material";

            foreach (var draw in drawCalls)
            {
                var sharedMats = draw.renderer.sharedMaterials;
                sharedMats[draw.submeshIndex] = materialCopy;
                draw.renderer.sharedMaterials = sharedMats;
            }


        }
    }
}
#endif