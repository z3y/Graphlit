void ContrastNode(half3 In, out half3 Out, half Contrast = 1)
{
    half mid = pow(0.5, 2.2);
    Out = (In - mid) * Contrast + mid;
}