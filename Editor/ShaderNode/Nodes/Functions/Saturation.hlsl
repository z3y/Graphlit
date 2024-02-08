void SaturationNode(half3 In, out half3 Out, half Saturation = 1)
{
    half3 grayscale = half3(0.2126729, 0.7151522, 0.0721750);
    Out = lerp(dot(In, grayscale), In, Saturation);
}