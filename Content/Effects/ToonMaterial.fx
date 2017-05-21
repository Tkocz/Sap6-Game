extern uniform float4x4 Model;
extern uniform float4x4 Proj;
extern uniform float4x4 View;
extern uniform float3 CamPos;
extern uniform float3 Amb;
extern uniform float3 Dif;
extern uniform float3 Spe;
extern uniform float K;

extern uniform bool UseDifTex;
extern uniform texture DifTex;

extern uniform bool UseNormTex;
extern uniform texture NormTex;
extern uniform float NormCoeff;

extern uniform int DifLevels;
extern uniform int SpeLevels;

extern uniform bool UseVertCol;

sampler difTex = sampler_state { Texture = <DifTex>; mipfilter = LINEAR; };
sampler normTex = sampler_state { Texture = <NormTex>; mipfilter = LINEAR; };

// TODO: Should be uniforms
static const float FogStart  = 35.0;
static const float FogEnd    = 100.0;
static const float3 FogColor = float3(0.4, 0.6, 0.8);

static const int NUM_LIGHTS = 3;

// Directional lights!
static const float4 Lights[NUM_LIGHTS] = {
  float4(-1.0, -1.0, -0.5,  0.5),
  float4(-0.5, -1.0, 0.5,  0.5),
  float4( 1.0, -0.5, -1.0,  0.5),
};

struct PSOutput {
  float4 color : SV_TARGET;
};

struct VSInput {
  float4 pos      : POSITION0;
  float2 texCoord : TEXCOORD0;
  float4 color    : COLOR;
  float3 normal   : NORMAL0;
};

struct VSOutput {
  float4 screenPos : POSITION0;
  float3 worldPos  : TEXCOORD1;
  float2 texCoord  : TEXCOORD0;
  float3 normal    : TEXCOORD2;
  float4 color     : COLOR;
};

void ps_main(in VSOutput x, out PSOutput r) {
  r.color = float4(Amb, 1.0);

  if (UseDifTex == true) {
    r.color *= float4(tex2D(difTex, x.texCoord).rgb, 1.0);
  }

  float3 norm = x.normal;
  if (UseNormTex == true) {
    norm += NormCoeff*(tex2D(normTex, x.texCoord).rgb - float3(0.5, 0.5, 0.5));
  }

  float3 n = normalize(norm);
  float3 v   = normalize(CamPos - x.worldPos);
  float3 vl  = length(CamPos - x.worldPos);

  float3 difCol;

  if (UseVertCol) {
    difCol = x.color.rgb;
    r.color *= float4(difCol, 1.0);
  }
  else {
    difCol = Dif;
  }

  for (int i = 0; i < NUM_LIGHTS; i++) {
    float3 l   = normalize(Lights[i].xyz);
    float3 rn  = reflect(l, n);
    float  j   = Lights[i].w;
    float  dif = max(0.0, dot(-l, n));
    float  spe = pow(max(0.0, dot(rn, v)), K);

    // Toon stuff
    dif = int(dif*DifLevels)/float(DifLevels);
    spe = int(spe*SpeLevels)/float(SpeLevels);

    float3 c = 0.0;

    if (UseDifTex == true) {
      c += dif*difCol*tex2D(difTex, x.texCoord).rgb;
    }
    else {
      c += dif*difCol;
    }

    c += spe*Spe;
    c *= j;

    r.color += float4(c, 1.0);
  }

  float fogFac = min(max(0.0, length(x.worldPos - CamPos) - FogStart) / (FogEnd - FogStart), 1.0);
  // not supporting alpha here, w/e
  r.color = float4((1.0-fogFac)*r.color.rgb + fogFac*FogColor, 1.0);
}

void vs_main(in VSInput x, out VSOutput r) {
  float4 modelPos = mul(x.pos   , Model);
  float4 viewPos  = mul(modelPos, View );

  r.screenPos = mul(viewPos, Proj);
  r.worldPos  = modelPos.xyz;
  r.texCoord  = x.texCoord;
  r.normal    = mul(float4(x.normal.xyz, 0.0), Model).xyz;
  r.color     = x.color;
}

technique Technique1 {
  pass Pass0 {
    PixelShader  = compile ps_3_0 ps_main();
    VertexShader = compile vs_3_0 vs_main();
  }
}
