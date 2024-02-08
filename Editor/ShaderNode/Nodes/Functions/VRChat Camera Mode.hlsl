float _VRChatCameraMode;
void VRChat_CameraMode(out bool Normal, out bool VRHandheld, out bool DesktopHandheld, out bool Screenshot)
{
    Normal = _VRChatCameraMode == 0;
    VRHandheld = _VRChatCameraMode == 1;
    DesktopHandheld = _VRChatCameraMode == 2;
    Screenshot = _VRChatCameraMode == 3;
}