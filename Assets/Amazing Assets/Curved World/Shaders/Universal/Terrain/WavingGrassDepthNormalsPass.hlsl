// Curved World <http://u3d.as/1W8h>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
#ifndef UNIVERSAL_WAVING_GRASS_DEPTH_NORMAL_PASSE_INCLUDED
#define UNIVERSAL_WAVING_GRASS_DEPTH_NORMAL_PASSE_INCLUDED


#include "../../../Core/CurvedWorldTransform.cginc" 


struct GrassVertexDepthNormalInput
{
    float4 vertex       : POSITION;
    float3 normal       : NORMAL;
    float4 tangent      : TANGENT;
    half4 color         : COLOR;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GrassVertexDepthNormalOutput
{
    float2 uv           : TEXCOORD0;
    half3 normal        : TEXCOORD1;
    half4 color         : TEXCOORD2;
    float3 viewDirWS    : TEXCOORD3;
    float4 clipPos      : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

GrassVertexDepthNormalOutput DepthNormalOnlyVertex(GrassVertexDepthNormalInput v)
{
    GrassVertexDepthNormalOutput o = (GrassVertexDepthNormalOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // MeshGrass v.color.a: 1 on top vertices, 0 on bottom vertices
    // _WaveAndDistance.z == 0 for MeshLit
    float waveAmount = v.color.a * _WaveAndDistance.z;
    o.color = TerrainWaveGrass(v.vertex, waveAmount, v.color);


#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
      CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
#endif


    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

    o.uv = v.texcoord;
    o.normal = TransformObjectToWorldNormal(v.normal);
    o.clipPos = vertexInput.positionCS;
    o.viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    return o;
}

GrassVertexDepthNormalOutput DepthNormalOnlyBillboardVertex(GrassVertexDepthNormalInput v)
{
    GrassVertexDepthNormalOutput o = (GrassVertexDepthNormalOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
      CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
#endif

    TerrainBillboardGrass (v.vertex, v.tangent.xy);

    // wave amount defined by the grass height
    float waveAmount = v.tangent.y;
    o.color = TerrainWaveGrass(v.vertex, waveAmount, v.color);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

    o.uv = v.texcoord;
    o.normal = TransformObjectToWorldNormal(v.normal);
    o.clipPos = vertexInput.positionCS;
    o.viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    return o;
}

half4 DepthNormalOnlyFragment(GrassVertexDepthNormalOutput input) : SV_TARGET
{
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a, input.color, _Cutoff);
    #if defined(_GBUFFER_NORMALS_OCT)
        float3 normalWS = NormalizeNormalPerPixel(input.normal);
        float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on Nintendo Switch.
        float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
        half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
        return half4(packedNormalWS, 0.0);
    #else
        return half4(NormalizeNormalPerPixel(input.normal), 0.0);
    #endif
}

#endif
