using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Graphlit
{
    [InitializeOnLoad]
    sealed class GraphlitLightmapVisualization
    {
        //static readonly SceneView.CameraMode lightVolumeL0Mode;

        static readonly HashSet<SceneView> setupSceneViews = new HashSet<SceneView>();

        static GraphlitLightmapVisualization()
        {

            SceneView.beforeSceneGui += view =>
            {
                if (setupSceneViews.Add(view))
                {
                    view.onCameraModeChanged += cameraMode =>
                    {

                        if (view.cameraMode.drawMode == DrawCameraMode.BakedAlbedo)
                        {
                            Shader.SetGlobalInteger("graplhit_MetaControl", 1);
                        }
                        else if (view.cameraMode.drawMode == DrawCameraMode.BakedEmissive)
                        {
                            Shader.SetGlobalInteger("graplhit_MetaControl", 2);
                        }
                        else
                        {
                            Shader.SetGlobalInteger("graplhit_MetaControl", 0);
                        }
                    };
                }
            };
        }
    }
}