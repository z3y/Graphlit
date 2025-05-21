void LinearEyeDepthNode(float Depth, out float Out)
{
    Out = LinearEyeDepth(Depth, _ZBufferParams);
}