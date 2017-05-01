/*-------------------------------------
 * UNIFORMS
 *-----------------------------------*/

uniform extern float4x4 Model;
uniform extern float4x4 Proj;
uniform extern float4x4 View;

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
     psOut.color = float4(1.0f, 0.0f, 1.0f, 0.5f);
}

void vsMain(in VS_INPUT vsIn, out VS_OUTPUT vsOut) {
  float4 worldPos = mul(vsIn.pos, Model);
  float4 viewPos  = mul(worldPos, View);
  vsOut.pos       = mul(viewPos, Proj);
  vsOut.texCoord  = vsIn.texCoord;
}

technique T1 {
  pass P0 {
    PixelShader = compile ps_3_0 psMain();
    VertexShader = compile vs_3_0 vsMain();
  }
}
