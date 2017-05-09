/*-------------------------------------
 * UNIFORMS
 *-----------------------------------*/

uniform extern texture SrcTex;

sampler texSampler = sampler_state {
        Texture   = <SrcTex>;
        mipfilter = LINEAR;
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
     float2 texCoord = float2(vsOut.texCoord.x + 0.2f*sin(vsOut.texCoord.y*3.141592f),
                              vsOut.texCoord.y + 0.2f*cos(vsOut.texCoord.x*3.141592f*2.0f));
     psOut.color = tex2D(texSampler, texCoord).rgba;
}

void vsMain(in VS_INPUT vsIn, out VS_OUTPUT vsOut) {
  vsOut.pos      = vsIn.pos;
  vsOut.texCoord = vsIn.texCoord;
}

technique T1 {
  pass P0 {
    PixelShader = compile ps_3_0 psMain();
    VertexShader = compile vs_3_0 vsMain();
  }
}
