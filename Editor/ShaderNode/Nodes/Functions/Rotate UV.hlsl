void RotateUVNode(out float2 Out, float2 UV, float2 pivot, float degrees)
{
	float angle = radians(degrees);
    float2 translatedUV = UV - pivot;
    
    float cosAngle;
    float sinAngle;
	sincos(angle, sinAngle, cosAngle);

    float2x2 rotationMatrix = float2x2(
        cosAngle, -sinAngle,
        sinAngle,  cosAngle
    );

    float2 rotatedUV = mul(translatedUV, rotationMatrix);
    
    rotatedUV += pivot;
    
    Out = rotatedUV;
}