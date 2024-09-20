void ObjectNode(out float3 Position, out float3 Scale)
{
    Position = UNITY_MATRIX_M._m03_m13_m23;
    Scale = float3(
        length(UNITY_MATRIX_M._m00_m10_m20),
        length(UNITY_MATRIX_M._m01_m11_m21),
        length(UNITY_MATRIX_M._m02_m12_m22)
    );
}