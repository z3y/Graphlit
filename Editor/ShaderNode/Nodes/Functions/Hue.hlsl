void HueShiftNode(half3 In, out half3 Out, float Offset = 0)
{
    half3 hsv = RgbToHsv(In);
    half hue = hsv.x + Offset;
    hsv.x = RotateHue(hue, 0, 1);
    Out = HsvToRgb(hsv);
}