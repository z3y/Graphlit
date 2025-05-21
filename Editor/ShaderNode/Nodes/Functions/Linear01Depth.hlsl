void Linear01DepthNode(float Depth, out float Out)
{
    Out = Linear01Depth(Depth, _ZBufferParams);
}