using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Graphlit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CapsuleShadows : UdonSharpBehaviour
    {
        public float radius = 0.5f;
        public float lightRadius = 0.07f;
        public float shadowDistance = 1.0f;

        Vector4[] _points = new Vector4[32];
        Vector4[] _data = new Vector4[32];

        Vector4 _tmp = new Vector4();

        int _UdonCapsuleShadowsPointsID;
        int _UdonCapsuleShadowsDataID;
        int _UdonCapsuleShadowsParamsID;
        int _UdonCapsuleShadowsPointsCountID;
        VRCPlayerApi _player;

        [Header("Scale Multiplier")]
        public float head = 0.13f;
        public float chest = 0.12f;
        public float upperArm = 0.04f;
        public float lowerArm = 0.03f;
        public float hand = 0.025f;
        public float upperLeg = 0.04f;
        public float lowerLeg = 0.02f;
        public float feet = 0.035f;

        void Start()
        {
            InitializeShaderIds();
            _player = Networking.LocalPlayer;

            InitializeConstants();
        }

        float _height = 1.5f;
        void InitializeShaderIds()
        {
            _UdonCapsuleShadowsPointsID = VRCShader.PropertyToID("_UdonCapsuleShadowsPoints");
            _UdonCapsuleShadowsParamsID = VRCShader.PropertyToID("_UdonCapsuleShadowsParams");
            _UdonCapsuleShadowsPointsCountID = VRCShader.PropertyToID("_UdonCapsuleShadowsPointsCount");
            _UdonCapsuleShadowsDataID = VRCShader.PropertyToID("_UdonCapsuleShadowsData");
        }

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
        {
            if (player != _player)
            {
                return;
            }

            _height = player.GetAvatarEyeHeightAsMeters();
            InitializeConstants();
        }

        void InitializeConstants()
        {
            _height = _player.GetAvatarEyeHeightAsMeters();

            VRCShader.SetGlobalVector(_UdonCapsuleShadowsParamsID, new Vector4(
                radius,
                lightRadius * _height,
                shadowDistance * _height
                ));
            VRCShader.SetGlobalInteger(_UdonCapsuleShadowsPointsCountID, 14 * 2);

            int i = 0;
            float scale = radius * _height;

            _data[i] = new Vector4(head * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.Head);
            i += 2;
            _data[i] = new Vector4(lowerLeg * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            i += 2;
            _data[i] = new Vector4(lowerLeg * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.RightLowerLeg);
            i += 2;
            _data[i] = new Vector4(upperLeg * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            i += 2;
            _data[i] = new Vector4(upperLeg * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.RightLowerLeg);
            i += 2;
            _data[i] = new Vector4(chest * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.Neck);
            i += 2;
            _data[i] = new Vector4(upperArm * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerArm);
            i += 2;
            _data[i] = new Vector4(upperArm * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.RightLowerArm);
            i += 2;
            _data[i] = new Vector4(lowerArm * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.RightHand);
            i += 2;
            _data[i] = new Vector4(lowerArm * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.LeftHand);
            i += 2;
            _data[i] = new Vector4(feet * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.RightToes);
            i += 2;
            _data[i] = new Vector4(feet * scale, 0, 0, 0);
            //_data[i++] = p.GetBonePosition(HumanBodyBones.LeftToes);

            i += 2;
            _data[i] = new Vector4(hand * scale, 0, 0, 0);

            i += 2;
            _data[i] = new Vector4(hand * scale, 0, 0, 0);

            VRCShader.SetGlobalVectorArray(_UdonCapsuleShadowsDataID, _data);

        }

        public override void PostLateUpdate()
        {
            var v = _tmp;
            var p = _player;
            int i = 0;

            var headP = p.GetBonePosition(HumanBodyBones.Head);
            var neck = p.GetBonePosition(HumanBodyBones.Neck);
            _points[i++] = headP;
            _points[i++] = headP + ((headP - neck) * 0.5f);

            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftFoot);
            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerLeg);

            _points[i++] = p.GetBonePosition(HumanBodyBones.RightFoot);
            _points[i++] = p.GetBonePosition(HumanBodyBones.RightLowerLeg);

            var leftUpperLeg = p.GetBonePosition(HumanBodyBones.LeftUpperLeg);
            _points[i++] = leftUpperLeg;
            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerLeg);

            var rightUpperLeg = p.GetBonePosition(HumanBodyBones.RightUpperLeg);
            _points[i++] = rightUpperLeg;
            _points[i++] = p.GetBonePosition(HumanBodyBones.RightLowerLeg);

            var hips = (leftUpperLeg + rightUpperLeg) / 2.0f;
            hips = (hips + p.GetBonePosition(HumanBodyBones.Hips)) / 2.0f;
            _points[i++] = hips;
            _points[i++] = p.GetBonePosition(HumanBodyBones.Chest);

            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftShoulder);
            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerArm);

            _points[i++] = p.GetBonePosition(HumanBodyBones.RightShoulder);
            _points[i++] = p.GetBonePosition(HumanBodyBones.RightLowerArm);

            _points[i++] = p.GetBonePosition(HumanBodyBones.RightLowerArm);
            _points[i++] = p.GetBonePosition(HumanBodyBones.RightHand);

            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftLowerArm);
            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftHand);

            _points[i++] = p.GetBonePosition(HumanBodyBones.RightFoot);
            _points[i++] = p.GetBonePosition(HumanBodyBones.RightToes);

            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftFoot);
            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftToes);

            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftHand);
            _points[i++] = p.GetBonePosition(HumanBodyBones.LeftMiddleDistal);

            _points[i++] = p.GetBonePosition(HumanBodyBones.RightHand);
            _points[i++] = p.GetBonePosition(HumanBodyBones.RightMiddleDistal);

            VRCShader.SetGlobalVectorArray(_UdonCapsuleShadowsPointsID, _points);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            InitializeConstants();
        }

        private void OnDrawGizmosSelected()
        {
            if (_points == null || _data == null) return;

            Gizmos.color = Color.green;

            for (int i = 0; i < _points.Length; i += 2)
            {
                Vector3 p0 = _points[i];
                Vector3 p1 = _points[i + 1];
                float radius = _data[i].x;

                DrawCapsuleApprox(p0, p1, radius);
            }
        }

        // Approximate capsule with 3 circles + connecting lines
        void DrawCapsuleApprox(Vector3 p0, Vector3 p1, float radius)
        {
            Vector3 up = (p1 - p0).normalized;
            float height = Vector3.Distance(p0, p1);

            if (height < 0.0001f)
            {
                Gizmos.DrawSphere(p0, radius);
                return;
            }

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, up);

            int segments = 12;
            float angleStep = 360f / segments;

            // Draw spheres at ends
            Gizmos.DrawWireSphere(p0, radius);
            Gizmos.DrawWireSphere(p1, radius);

            // Draw circle at mid height to approximate the cylindrical body
            Vector3 mid = (p0 + p1) * 0.5f;
            for (int i = 0; i < segments; i++)
            {
                float angle0 = i * angleStep * Mathf.Deg2Rad;
                float angle1 = ((i + 1) % segments) * angleStep * Mathf.Deg2Rad;

                Vector3 offset0 = new Vector3(Mathf.Cos(angle0), 0, Mathf.Sin(angle0)) * radius;
                Vector3 offset1 = new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;

                // Mid circle
                Gizmos.DrawLine(mid + rot * offset0, mid + rot * offset1);

                // Connect sides
                Gizmos.DrawLine(p0 + rot * offset0, p1 + rot * offset0);
            }
        }
#endif
    }
}