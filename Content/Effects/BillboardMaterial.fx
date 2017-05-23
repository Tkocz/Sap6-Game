extern uniform float4x4 Model;
extern uniform float4x4 Proj;
extern uniform float4x4 View;

extern uniform float3 CamPos;

extern uniform texture BillboardTex;

// TODO: Should be uniforms
static const float FogStart  = 35.0;
static const float FogEnd    = 100.0;
static const float3 FogColor = float3(0.4, 0.6, 0.8);

sampler billboardTex = sampler_state { Texture = <BillboardTex>; mipfilter = LINEAR; };

struct PSOutput {
  float4 color : SV_TARGET;
};

struct VSInput {
  float4 pos      : POSITION0;
  float2 texCoord : TEXCOORD0;
  float3 normal   : NORMAL0;
};

struct VSOutput {
  float4 screenPos : POSITION0;
  float2 texCoord  : TEXCOORD0;
  float4 worldPos : TEXCOORD4;
};

void ps_main(in VSOutput x, out PSOutput r) {
  r.color = tex2D(billboardTex, x.texCoord);
  if (r.color.a <= 1.0/255.0) {
    discard;
  }
  else {
    float fogFac = min(max(0.0, length(x.worldPos.xyz - CamPos) - FogStart) / (FogEnd - FogStart), 1.0);
    r.color = (1.0-fogFac)*r.color + fogFac*float4(FogColor, 1.0);
  }
}

void vs_main(in VSInput x, out VSOutput r) {
  float4 modelPos = mul(x.pos   , Model);
  float4 viewPos  = mul(modelPos, View );

  r.screenPos = mul(viewPos, Proj);
  r.worldPos  = modelPos;
  r.texCoord  = x.texCoord;
}

technique Technique1 {
  pass Pass0 {
    PixelShader  = compile ps_3_0 ps_main();
    VertexShader = compile vs_3_0 vs_main();
  }
}
