float _VRChatMirrorMode;
void VRChat_MirrorMode(out bool NotInMirror, out bool MirrorVR, out bool MirrorDesktop, out bool InMirror)
{
    NotInMirror = _VRChatMirrorMode == 0;
    MirrorVR = _VRChatMirrorMode == 1;
    MirrorDesktop = _VRChatMirrorMode == 2;
    InMirror = MirrorVR || MirrorDesktop;
}