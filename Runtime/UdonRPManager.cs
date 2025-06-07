#if UDONSHARP
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Graphlit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), ExecuteInEditMode]
    public class UdonRPManager : UdonSharpBehaviour
    {
        [SerializeField] bool _enableDirectionalCookie;
        [SerializeField] Light _mainDirectionalLight;
        [SerializeField] Texture _directionalCookie;
        [SerializeField] Vector2 _cookieScale = Vector2.one;

        [SerializeField] bool _enableEnvironmentProbe;
        [SerializeField] Cubemap skyprobe;

        void Start()
        {
            SetGlobals();
        }

        void OnValidate()
        {
            SetGlobals();
        }

        void SetGlobals()
        {
            SetDirectionalCookie();
            SetEvnironmentProbe();
        }

        void SetDirectionalCookie()
        {
            bool hasCookie = _mainDirectionalLight && _enableDirectionalCookie && _directionalCookie;
            VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_UdonRPDirectionalCookie"), _enableDirectionalCookie ? _directionalCookie : null);
            if (_mainDirectionalLight)
            {
                var t = _mainDirectionalLight.transform;
                var rotation = Quaternion.LookRotation(t.forward, t.up);

                var lightToWorld = Matrix4x4.TRS(t.position, rotation,
                    new Vector3(_cookieScale.x, _cookieScale.y, 1.0f))
                    .inverse;

                VRCShader.SetGlobalMatrix(VRCShader.PropertyToID("_UdonRPWorldToDirectionalLight"),
                    lightToWorld);
            }

        }

        private void SetEvnironmentProbe()
        {
            bool hasEnvironmentProbe = _enableEnvironmentProbe && skyprobe;
            VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_UdonRPGlossyEnvironmentCubeMap"), _enableEnvironmentProbe ? skyprobe : null);
        }

        void SetKeyword(string name, bool state)
        {
#if !COMPILER_UDONSHARP
            if (state)
            {
                Shader.EnableKeyword(name);
            }
            else
            {
                Shader.DisableKeyword(name);
            }
#else
    // set keyword manually for every material lol
#endif

        }

    }
}
#endif