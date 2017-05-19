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
uniform extern float Bias; // 0.5

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
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput {
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float4 Color : COLOR0;
    float4 WorldPos : TEXCOORD4;
};

float CalculateHeight(float4 Position) {
    int phase = (Position.x + Position.z % 2) * 2;
    float newHeight = (Amplitude * sin(Frequency * Time + phase) + Bias);

    return newHeight * 10;

}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Position.y += CalculateHeight(input.Position);

    float4 worldNormal = mul(float4(input.Normal.xyz, 0), World);
    float4 viewNormal = mul(float4(worldNormal.xyz, 0), View);
    output.Normal = normalize(viewNormal);
    output.Color = input.Color + (DiffuseIntensity * DiffuseColor);
    output.TextureCoordinate = input.TextureCoordinate;
    output.WorldPos = worldPosition;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
    float3 bump = BumpConstant * ((tex2D(bumpSampler, input.TextureCoordinate * 10 + float2(cos(Time * 0.02), sin(Time * 0.02))) - (0.5, 0.5, 0.5)));
    float3 bumpNormal = input.Normal + bump;
    bumpNormal = normalize(bumpNormal);

    float3 light = normalize(DiffuseLightDirection);
    float diffuseIntensity = dot(light, bumpNormal);
    if (diffuseIntensity < 0)
        diffuseIntensity = 0;

    float3 r = normalize(reflect(light, bumpNormal));
    float3 v = normalize(CameraPosition - input.WorldPos);
    float dotProduct = dot(r, v);

    float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * diffuseIntensity;

    return float4 (saturate(input.Color.xyz * (diffuseIntensity) + AmbientColor.xyz * AmbientIntensity + specular.xyz), 0.5);
}
technique BasicColorDrawing {
    pass Pass1 {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
