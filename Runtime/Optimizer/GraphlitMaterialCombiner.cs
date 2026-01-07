#if UNITY_EDITOR && NDMF_INCLUDED
using nadena.dev.ndmf;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: ExportsPlugin(typeof(Graphlit.Optimizer.GraphlitMaterialCombiner))]

namespace Graphlit.Optimizer
{
    public class GraphlitMaterialCombiner : Plugin<GraphlitMaterialCombiner>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run("Add Vertex Stream", ctx =>
            {
                CombineMaterials(ctx);
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

            var renderers = ctx.AvatarRootObject.GetComponentsInChildren<Renderer>(false);


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

                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                {

                    var submesh = mesh.GetSubMesh(submeshIndex);

                    var drawCall = new DrawCall
                    {
                        material = renderer.sharedMaterials[submeshIndex],
                        renderer = renderer,
                        mesh = mesh,
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
            if (drawCalls.Count < 2)
            {
                return;
            }


            var groups = drawCalls.GroupBy(x => x.material);

            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();

            int vertexOffset = 0;

            List<int> thresholds = new();

            foreach (var group in groups)
            {
                int materialID = 0;

                Debug.Log($"Material: {group.Key.name}, ID: {materialID}, baseVertex {vertexOffset}");
                thresholds.Add(vertexOffset);
                materialID++;

                foreach (var draw in group)
                {
                    var mesh = draw.mesh;

                    var toAdd = mesh.vertices[draw.baseVertex..draw.vertexCount];
                    var transform = draw.renderer.transform;

                    transform.TransformPoints(toAdd);

                    transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    vertices.AddRange(toAdd);
                    uv.AddRange(mesh.uv[draw.baseVertex..draw.vertexCount]);
                    normals.AddRange(mesh.normals[draw.baseVertex..draw.vertexCount]);

                    var indices = mesh.GetIndices(draw.submeshIndex);

                    for (int j = 0; j < indices.Length; j++)
                    {
                        indices[j] = (indices[j] - draw.baseVertex) + vertexOffset;
                    }

                    triangles.AddRange(indices);


                    vertexOffset = vertices.Count;

                }
            }

            var mergedMesh = new Mesh();
            mergedMesh.name = "Graphlit Merged Mesh";

            mergedMesh.indexFormat = vertices.Count >= 65536 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

            mergedMesh.SetVertices(vertices);
            mergedMesh.SetUVs(0, uv);
            mergedMesh.SetNormals(normals);
            mergedMesh.SetTriangles(triangles, 0);

            ctx.AssetSaver.SaveAsset(mergedMesh);


            for (int i = 0; i < drawCalls.Count; i++)
            {
                drawCalls[i].renderer.gameObject.SetActive(false);
                // Object.Destroy(drawCalls[i].renderer.gameObject);
            }

            DrawCall firstDraw = drawCalls[0];

            Debug.Log($"firstDraw: {firstDraw.renderer.gameObject.name}");

            if (firstDraw.isSkinned)
            {
                var smr = firstDraw.renderer.GetComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = mergedMesh;
            }
            else
            {
                var filter = firstDraw.renderer.GetComponent<MeshFilter>();
                filter.sharedMesh = mergedMesh;
            }

            var sb = new StringBuilder();

            sb.Append("uint thresholds[");
            sb.Append((thresholds.Count - 1).ToString());
            sb.Append("] = {");

            for (int i = 1; i < thresholds.Count; i++)
            {
                int threshold = thresholds[i];
                sb.Append($"{threshold}");
                if (i != thresholds.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append("};");

            Debug.Log(sb.ToString());
        }
    }
}
#endif