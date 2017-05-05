uniform extern matrix World;
uniform extern matrix View;
uniform extern matrix Projection;

uniform extern float4 AmbientColor;
uniform extern float AmbientIntensity;

uniform extern matrix WorldInverseTranspose;

uniform extern float3 DiffuseLightDirection;
uniform extern float4 DiffuseColor;
uniform extern float DiffuseIntensity;

uniform extern float Shininess;
uniform extern float4 SpecularColor;
uniform extern float SpecularIntensity;
uniform extern float3 ViewVector;

uniform extern float3 CameraPosition;

uniform extern float Time;
/*
texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};*/

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
    float amplitude = 0.2;
    float frequency = 2;
    float bias = 0.5;
    float newHeight = (amplitude * sin(frequency * Time + phase) + bias);
     
    return newHeight * 10; 
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Position.y += CalculateHeight(input.Position);
    float4 worldNormal = mul(input.Normal, World);
    output.Normal = normalize(mul(View, worldNormal));
    output.Color = input.Color + (DiffuseIntensity * DiffuseColor);
    output.TextureCoordinate = input.TextureCoordinate;
    output.WorldPos = viewPosition;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
    // Calculate the normal, including the information in the bump map
    float3 bump = BumpConstant * (tex2D(bumpSampler, input.TextureCoordinate * 5) - (0.5, 0.5, 0.5));
    float3 bumpNormal = input.Normal + bump;
    bumpNormal = normalize(bumpNormal);

    // Calculate the diffuse light component with the bump map normal
    float3 light = normalize(DiffuseLightDirection);
    float diffuseIntensity = dot(light, bumpNormal);
    if (diffuseIntensity < 0)
        diffuseIntensity = 0;

    // Calculate the specular light component with the bump map normal
    float3 r = normalize(reflect(light, bumpNormal));
    float3 v = normalize(CameraPosition - input.WorldPos);
    float dotProduct = dot(r, v);

    float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * diffuseIntensity;

    // Calculate the texture color
    //float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    //textureColor.a = 1;

    // Combine all of these values into one (including the ambient light)
    return float4 (saturate(input.Color.xyz * (diffuseIntensity) + AmbientColor.xyz * AmbientIntensity + specular.xyz), 0.5);
}
technique BasicColorDrawing {
    pass Pass1 {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
