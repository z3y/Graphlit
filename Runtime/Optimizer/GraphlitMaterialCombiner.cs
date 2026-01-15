#if UNITY_EDITOR && NDMF_INCLUDED
using nadena.dev.ndmf;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.IO;
using nadena.dev.ndmf.vrchat;
using Unity.Mathematics;

[assembly: ExportsPlugin(typeof(Graphlit.Optimizer.GraphlitMaterialCombiner))]

namespace Graphlit.Optimizer
{
    public class GraphlitMaterialCombiner : Plugin<GraphlitMaterialCombiner>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing).Run("Graphlit Material Combiner", ctx =>
            {
                TryCombineMaterials(ctx);
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
            public int rendererId;
        }

        struct AnimatedProperty
        {
            public Renderer renderer;
            public string referenceName;
        }

        static List<AnimatedProperty> GetAnimatedProperties(BuildContext ctx)
        {
            List<AnimatedProperty> animatedProperties = new();
            var avatarDescriptor = ctx.VRChatAvatarDescriptor();

            var root = avatarDescriptor.transform;

            var clips = avatarDescriptor.baseAnimationLayers.SelectMany(x => x.animatorController.animationClips);

            foreach (var clip in clips)
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);

                foreach (var binding in bindings)
                {

                    var propertyName = binding.propertyName;
                    if (propertyName.StartsWith("material.", System.StringComparison.Ordinal))
                    {
                        var anp = new AnimatedProperty
                        {
                            referenceName = propertyName["material.".Length..].Trim()
                        };

                        var animTarget = root.Find(binding.path);
                        if (animTarget)
                        {
                            anp.renderer = animTarget.GetComponent<Renderer>();
                        }
                        if (anp.renderer)
                        {
                            animatedProperties.Add(anp);
                        }
                    }
                }

            }

            return animatedProperties;
        }

        public static void TryCombineMaterials(BuildContext ctx)
        {
            var optimizer = ctx.AvatarRootObject.GetComponent<GraphlitOptimizer>();
            if (!optimizer)
            {
                return;
            }

            if (!optimizer.applyOnBuild)
            {
                return;
            }

            // todo check if parent is editor only
            var renderers = ctx.AvatarRootObject.GetComponentsInChildren<Renderer>(true).Where(x => !x.CompareTag("EditorOnly"));


            var drawCallsMap = new Dictionary<int, List<DrawCall>>();

            int rendererId = 0;
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

                Mesh meshCopy = null;

                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                {

                    var submesh = mesh.GetSubMesh(submeshIndex);

                    var mat = renderer.sharedMaterials[submeshIndex];

                    if (optimizer.excludedMaterials.Contains(mat))
                    {
                        continue;
                    }

                    if (!mat.HasFloat("_GraphlitMaterial"))
                    {
                        continue;
                    }

                    if (!meshCopy)
                    {
                        meshCopy = Object.Instantiate(mesh);
                        ctx.AssetSaver.SaveAsset(meshCopy);
                    }

                    var drawCall = new DrawCall
                    {
                        material = renderer.sharedMaterials[submeshIndex],
                        renderer = renderer,
                        mesh = meshCopy,
                        submeshIndex = submeshIndex,
                        baseVertex = submesh.baseVertex,
                        vertexCount = submesh.vertexCount,
                        isSkinned = isSkinned,
                        rendererId = rendererId
                    };

                    rendererId++;

                    int hash = GenerateMaterialHash(mat);

                    if (drawCallsMap.TryGetValue(hash, out var drawCalls))
                    {
                        drawCalls.Add(drawCall);
                    }
                    else
                    {
                        var newDrawCalls = new List<DrawCall>
                        {
                            drawCall
                        };

                        drawCallsMap[hash] = newDrawCalls;
                    }

                }
            }

            var animatedProps = GetAnimatedProperties(ctx);

            foreach (var drawCallGroup in drawCallsMap)
            {
                var drawCalls = drawCallGroup.Value;
                int maxPerBatch = optimizer.maxMaterialsPerBatch;

                if (drawCalls.Count > maxPerBatch)
                {
                    for (int i = 0; i < drawCalls.Count; i += maxPerBatch)
                    {
                        int count = Mathf.Min(maxPerBatch, drawCalls.Count - i);
                        MergeDrawCalls(ctx, optimizer, drawCalls.GetRange(i, count), animatedProps);
                    }
                }
                else
                {
                    MergeDrawCalls(ctx, optimizer, drawCallGroup.Value, animatedProps);
                }
            }


        }

        static int GenerateMaterialHash(Material mat)
        {
            var hash = new System.HashCode();

            var keywords = mat.enabledKeywords;

            var shader = mat.shader;

            hash.Add(shader);

            hash.Add(mat.renderQueue);

            for (int i = 0; i < keywords.Length; i++)
            {
                hash.Add(keywords[i].name);
            }

            hash.Add(mat.GetFloat("_ZTest"));
            hash.Add(mat.GetFloat("_ZWrite"));
            hash.Add(mat.GetFloat("_SrcBlend"));
            hash.Add(mat.GetFloat("_DstBlend"));
            hash.Add(mat.GetFloat("_AlphaToMask"));

            return hash.ToHashCode();
        }

        static void MergeDrawCalls(BuildContext ctx, GraphlitOptimizer optimizer, List<DrawCall> drawCalls, List<AnimatedProperty> allAnimatedProps)
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

                        float uvz = math.asfloat(materialID + (draw.rendererId << 12));

                        var indices = meshCopy.GetIndices(draw.submeshIndex);

                        for (int j = 0; j < indices.Length; j++)
                        {
                            Vector3 uv = uvs[indices[j]];
                            uv.z = uvz;
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

            var shader = lockMaterials[0].shader;
            var shaderPath = AssetDatabase.GetAssetPath(shader);
            var serializedGraph = GraphlitImporter.ReadGraphData(AssetDatabase.GUIDFromAssetPath(shaderPath).ToString());
            serializedGraph.data.unlocked = false;
            serializedGraph.data.enableLockMaterials = true;
            serializedGraph.data.lockMaterials = lockMaterials;

            int referenceCullMode = (int)lockMaterials[0].GetFloat("_Cull");
            for (int i = 1; i < lockMaterials.Count; i++)
            {
                if ((int)lockMaterials[i].GetFloat("_Cull") != referenceCullMode)
                {
                    serializedGraph.data.optimizerMixedCull = true;
                    break;
                }
            }

            var mergedMaterialName = string.Join(" ", lockMaterials.Select(x => x.name));
            if (mergedMaterialName.Length > 100)
            {
                mergedMaterialName = mergedMaterialName[0..100];
            }

            var graphView = new ShaderGraphView(null, shaderPath);
            serializedGraph.PopulateGraph(graphView);
            graphView.UpdateCachedNodesForBuilder();

            var shaderNodes = graphView.cachedNodesForBuilder;
            var template = shaderNodes.OfType<TemplateOutput>().First();

            var builder = new ShaderBuilder(GenerationMode.Final, graphView);
            builder.shaderName = "Hidden/GraphlitOptimizer/" + mergedMaterialName;

            var batchAnimatedProps = allAnimatedProps.Where(animatedProp => drawCalls.Any(drawCall => drawCall.renderer == animatedProp.renderer));

            var builderProps = graphView.graphData.properties;
            foreach (var anp in batchAnimatedProps)
            {
                var index = builderProps.FindIndex(x => x.GetReferenceName(GenerationMode.Final) == anp.referenceName);
                if (index >= 0)
                {
                    builderProps[index].animatable = true;
                    Debug.Log($"Setting {anp.referenceName} as animatable on {anp.renderer.name}");
                }
            }


            builder.BuildTemplate(template);

            var shaderString = builder.ToString();

            // GUIUtility.systemCopyBuffer = shaderString;
            // AssetDatabase.CreateAsset(new TextAsset(shaderString), "Assets/OptimizedShader.asset");
            File.WriteAllText($"Logs/{mergedMaterialName}.shader", shaderString);

            var optimizedShader = ShaderUtil.CreateShaderAsset(shaderString, false);

            template.ApplyDefaultTextures(builder, optimizedShader);

            ctx.AssetSaver.SaveAsset(optimizedShader);

            Material referenceMaterial = lockMaterials[0];
            int renderQueue = referenceMaterial.renderQueue;

            var materialCopy = new Material(optimizedShader);
            materialCopy.CopyMatchingPropertiesFromMaterial(referenceMaterial);
            materialCopy.renderQueue = renderQueue;

            // remove main tex so fallback doesnt have random texture
            if (lockMaterials.Count > 1)
            {
                if (materialCopy.HasProperty("_MainTex"))
                {
                    materialCopy.SetTexture("_MainTex", optimizer.fallbackMainTex);
                    materialCopy.SetTextureScale("_MainTex", new Vector2(optimizer.fallbackMainTexScaleOffset.x, optimizer.fallbackMainTexScaleOffset.y));
                    materialCopy.SetTextureOffset("_MainTex", new Vector2(optimizer.fallbackMainTexScaleOffset.z, optimizer.fallbackMainTexScaleOffset.w));
                }
                if (materialCopy.HasProperty("_Color")) materialCopy.SetColor("_Color", Color.white);
            }

            if (serializedGraph.data.optimizerMixedCull)
            {
                materialCopy.SetFloat("_Cull", 0);
            }
            ctx.AssetSaver.SaveAsset(materialCopy);

            materialCopy.name = mergedMaterialName;

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