#pragma once

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
    #define UNITY_SHADER_VARIABLES_INCLUDED
#endif
struct SpsInputs : Attributes
{
};
#define SPS_STRUCT_POSITION_NAME positionOS
#define SPS_STRUCT_NORMAL_NAME normalOS
#define SPS_STRUCT_TANGENT_NAME tangentOS
#define SPS_STRUCT_COLOR_NAME color
#define SPS_STRUCT_SV_VertexID_NAME vertexID
#include "Packages/com.vrcfury.vrcfury/SPS/sps_main.cginc"