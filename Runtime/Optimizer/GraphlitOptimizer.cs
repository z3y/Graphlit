#if UNITY_EDITOR && NDMF_INCLUDED
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Graphlit.Optimizer
{
    public class GraphlitOptimizer : MonoBehaviour, IEditorOnly
    {
        public bool applyOnBuild = true;
        [Tooltip("Maximum number of materials that can be merged into 1 Material.\nUnity has a maxium of 64 texture bindings allowed per shader. Reduce this value if the optimized shader doesn't render.")]
        public int maxMaterialsPerBatch = 64;
        public List<Material> excludedMaterials = new();

        [Tooltip("VRChat fallback shaders can not work properly with the optimizer because multiple main textures get merged into one material. This texture can be used as a fallback instead (default is white).")]
        public Texture2D fallbackMainTex = null;
        public Vector4 fallbackMainTexScaleOffset = new(1, 1, 0, 0);
    }
}
#endif