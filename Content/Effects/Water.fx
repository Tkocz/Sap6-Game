#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

extern uniform matrix World;
extern uniform matrix View;
extern uniform matrix Projection;

extern uniform float Time;

const float PI = 3.14159;
static const float WaterHeight = 20;
//static const float time;
static const int NumWaves = 1;
static const float Amplitude = 0.2;
static const float Frequency = 2;
static const float Bias = 0.5;
static const float Wavelength = 1;
static const float Speed = 1;
static const float2 Direction = (1, 1);

/*float amplitude = 0.2;
float frequency = 2;
float bias = 0.5;*/

//extern uniform float3 CameraPosition;

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float Phase : TEXCOORD0;
};

float CalculateHeight(float4 Position) {
    int phase = (Position.x + Position.z % 2) * 2;
    float amplitude = 0.2;
    float frequency = 2;
    float bias = 0.5;
    float newHeight = (amplitude * sin(frequency * Time + phase) + bias);
    
    return newHeight * 10;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, mul(World, mul(View, Projection)));
    output.Position.y += CalculateHeight(input.Position);
    
    float3 normal = input.Normal;
    normal = mul(normal, World);
    output.Color = float4(0.5 * normal + 0.5, 1);
    float lightIntensity = saturate(dot(normal, float3(1, 1, 0)));
    output.Color = saturate(input.Color + (float4(1, 1, 1, 1) * 0.3 * lightIntensity));

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = input.Color;
    //color.a = 0.15;
    return color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};