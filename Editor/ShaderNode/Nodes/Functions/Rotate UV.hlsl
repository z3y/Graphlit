void RotateUV(out float2 Out, float2 UV, float2 pivot, float degrees)
{
	float angle = radians(degrees);
    float2 translatedUV = UV - pivot;
    
    float cosAngle = cos(angle);
    float sinAngle = sin(angle);
    float2x2 rotationMatrix = float2x2(
        cosAngle, -sinAngle,
        sinAngle,  cosAngle
    );

    float2 rotatedUV = mul(translatedUV, rotationMatrix);
    
    rotatedUV += pivot;
    
    Out = rotatedUV;
}