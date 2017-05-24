#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float SketchThreshold;
float SketchBrightness;

// Randomly offsets the sketch overlay pattern to create a hand-drawn animation effect.
float2 SketchJitter;

texture SceneTexture;

sampler SceneSampler : register(s0) = sampler_state
{
    Texture = (SceneTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

texture SketchTexture;

sampler SketchSampler : register(s2) = sampler_state
{
    Texture = (SketchTexture);

    AddressU = Wrap;
    AddressV = Wrap;
};

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original color from the main scene.
    float3 scene = tex2D(SceneSampler, texCoord);

    // Adjust the scene color to remove very dark values and increase the contrast.
    float3 saturatedScene = saturate((scene - SketchThreshold) * 2);
        
    // Look up into the sketch pattern overlay texture.
    float3 sketchPattern = tex2D(SketchSampler, texCoord + SketchJitter);
    
    // Convert into negative color space, and combine the scene color with the
    // sketch pattern. We need to do this multiply in negative space to get good
    // looking results, because pencil sketching works by drawing black ink
    // over an initially white page, rather than adding light to an initially
    // black background as would be more common in computer graphics.
    float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
        
    // Convert the result into a positive color space greyscale value.
    float sketchResult = dot(1 - negativeSketch, SketchBrightness);
        
    // Apply the sketch result to the main scene color.
    scene *= sketchResult;

    return float4(scene, 1);
}

technique SketchRain
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
};
