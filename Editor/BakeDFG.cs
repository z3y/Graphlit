using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Graphlit
{
    public class BakeDFG : MonoBehaviour
    {
        [MenuItem("Tools/Graphlit/Bake DFG")]
        public static void Bake()
        {
            var shader = Shader.Find("Hidden/Graphlit/DFG");
            var mat = new Material(shader);

            int res = 128;
            var desc = new RenderTextureDescriptor
            {
                autoGenerateMips = false,
                width = res,
                height = res,
                useMipMap = false,
                colorFormat = RenderTextureFormat.ARGBFloat,
                sRGB = false,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D
            };

            var rt = new RenderTexture(desc);

            RenderTexture.active = rt;

            Graphics.Blit(Texture2D.blackTexture, rt, mat, 0);

            var tex = new Texture2D(res, res, TextureFormat.RGBAFloat, false, true);
            tex.ReadPixels(new Rect(Vector2.zero, new Vector2(res, res)), 0, 0);

            var bytes = tex.EncodeToEXR();
            DestroyImmediate(tex);
            DestroyImmediate(rt);
            DestroyImmediate(mat);

            var path = "Packages/com.z3y.graphlit/Editor/Targets/Lit/dfg-multiscatter.exr";
            File.WriteAllBytes(path, bytes);
        }
    }
}