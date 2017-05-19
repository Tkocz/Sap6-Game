#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

/*-------------------------------------
 * UNIFORMS
 *-----------------------------------*/

uniform extern float Phase;
uniform extern texture SrcTex;

sampler texSampler = sampler_state {
  Texture   = <SrcTex>;
  MinFilter = Linear;
  MagFilter = Linear;
  AddressU = Clamp;
  AddressV = Clamp;
};

/*-------------------------------------
 * STRUCTS
 *-----------------------------------*/

struct PS_OUTPUT {
  float4 color : SV_TARGET;
};

struct VS_INPUT {
  float4 pos      : POSITION0;
  float2 texCoord : TEXCOORD0;
};

struct VS_OUTPUT {
  float4 pos : POSITION0;
  float2 texCoord  : TEXCOORD0;
};

/*-------------------------------------
 * FUNCTIONS
 *-----------------------------------*/

void psMain(in VS_OUTPUT vsOut, out PS_OUTPUT psOut) {
  float2 texCoord = float2(vsOut.texCoord.x + 0.02f*sin(Phase+vsOut.texCoord.y*3.141592f),
                           vsOut.texCoord.y + 0.02f*cos(Phase+vsOut.texCoord.x*3.141592f*2.0f));

  float4 a = tex2D(texSampler, texCoord).rgba;
  float4 b = float4(0.2, 0.3, 0.5, 1.0);
  psOut.color = lerp(a, b, 0.5);
}

void vsMain(in VS_INPUT vsIn, out VS_OUTPUT vsOut) {
  vsOut.pos      = vsIn.pos;
  vsOut.texCoord = vsIn.texCoord;
}

technique T1 {
  pass P0 {
    PixelShader = compile PS_SHADERMODEL psMain();
    VertexShader = compile VS_SHADERMODEL vsMain();
  }
}
