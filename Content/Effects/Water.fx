uniform extern matrix World;
uniform extern matrix View;
uniform extern matrix Projection;

uniform extern float4 AmbientColor;
uniform extern float AmbientIntensity;

uniform extern float3 DiffuseLightDirection;
uniform extern float4 DiffuseColor;
uniform extern float DiffuseIntensity;

uniform extern float Shininess;
uniform extern float4 SpecularColor;
uniform extern float SpecularIntensity;
uniform extern float3 ViewVector;

uniform extern float3 CameraPosition;

uniform extern float Time;
uniform extern float Amplitude; // 0.6
uniform extern float Frequency; // 2

uniform extern float BumpConstant;
uniform extern texture NormalMap;

sampler2D bumpSampler = sampler_state {
  Texture = (NormalMap);
  MinFilter = Linear;
  MagFilter = Linear;
  AddressU = Wrap;
  AddressV = Wrap;
};


struct VertexShaderInput {
  float4 Position : POSITION0;
  nointerpolation float3 Normal : NORMAL0;
  float2 TextureCoordinate : TEXCOORD0;
  float4 Color : COLOR0;
};

struct VertexShaderOutput {
  float4 Position : POSITION0;
  float2 TextureCoordinate : TEXCOORD0;
  nointerpolation float3 Normal : TEXCOORD1;
  float4 Color : COLOR0;
  float4 WorldPos : TEXCOORD4;
};

float CalculateHeight(float4 Position) {
  int phase = (Position.x * Position.z) * 0.5;
  float newHeight = (Amplitude * sin(Frequency * Time + phase));

  int phase2 = (0.1f*Position.x * 0.1f*Position.z) * 0.5;
  float newHeight2 = (Amplitude * cos(Frequency * Time*0.7 + phase2));

  return newHeight + newHeight2;

}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
  VertexShaderOutput output;

  float4 worldPosition = mul(input.Position, World);
  float4 viewPosition = mul(worldPosition, View);
  output.Position = mul(viewPosition, Projection);

  float3 a = float3(worldPosition.x, worldPosition.y, worldPosition.z);
  float3 b = float3(worldPosition.x + 5.0f, worldPosition.y, worldPosition.z);
  float3 c = float3(worldPosition.x, worldPosition.y, worldPosition.z + 5.0f);

  output.Position.y += CalculateHeight(worldPosition);

  a.y += 3.0f*CalculateHeight(float4(a, 0.0f));
  b.y += 3.0f*CalculateHeight(float4(b, 0.0f));
  c.y += 3.0f*CalculateHeight(float4(c, 0.0f));

  output.Normal = normalize(cross(a - c, a - b));

  float4 worldNormal = mul(float4(output.Normal.xyz, 0), World);
  float4 viewNormal = mul(float4(worldNormal.xyz, 0), View);
  output.Normal = normalize(viewNormal);
  output.Color = input.Color + (DiffuseIntensity * DiffuseColor);
  output.TextureCoordinate = input.TextureCoordinate;
  output.WorldPos = worldPosition;
  return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
  float3 n = normalize(input.Normal);
  float3 l = -normalize(float3(2.0, -1.0, 0.5));

  float3 v  = normalize(CameraPosition - input.WorldPos);
  float3 rv = normalize(reflect(v, n));

  float3 color = float3(0.2, 0.3, 0.5);

  const float nDif = 4.0;
  const float nSpec = 2.0;
  float ambFac  = 0.5f;
  float difFac  = int(max(0.0, dot(l, n))*nDif)/nDif;
  float specFac = int(max(0.0, pow(dot(l, rv), 90.0))*nSpec)/nSpec;

  float3 amb  = color * ambFac;
  float3 dif  = color * difFac;
  float3 spec = float3(1.0, 1.0, 1.0) * specFac;

  return float4(amb+dif+spec, 0.2+0.4*(difFac+specFac));
}
technique BasicColorDrawing {
  pass Pass1 {
    AlphaBlendEnable=true;
    VertexShader = compile vs_3_0 VertexShaderFunction();
    PixelShader = compile ps_3_0 PixelShaderFunction();
  }
}
