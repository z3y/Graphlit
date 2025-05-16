#pragma once

float _VRChatCameraMode;
float3 _VRChatMirrorCameraPos;
float _VRChatMirrorMode;

#ifdef _MIRROR
TEXTURE2D(_ReflectionTex0);
TEXTURE2D(_ReflectionTex1);
#endif