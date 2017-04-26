///////////////////////////////////////////////////////////////////////////////
// Filename: water.fx
////////////////////////////////////////////////////////////////////////////////

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

/////////////
// GLOBALS //
/////////////
matrix worldMatrix;
matrix viewMatrix;
matrix projectionMatrix;
//Just like the reflection tutorial the water shader will require a reflection matrix.

matrix reflectionMatrix;
//The water shader will need three textures.A reflection texture for the scene reflection.A refraction texture for the refraction of the scene.And finally a normal map texture for simulating water ripples.

Texture2D reflectionTexture;
Texture2D refractionTexture;
Texture2D normalTexture;
//The water translation variable will be used for simulating water motion by translating the texture sampling coordinates each frame.

float waterTranslation;
//The reflectRefractScale variable is used for controlling the size of the water ripples in relation to the normal map.Some normal maps will be slightly different in how drastic the normals rise and fall.Having a variable to control this becomes very useful.

float reflectRefractScale;


///////////////////
// SAMPLE STATES //
///////////////////
SamplerState SampleType
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};


//////////////
// TYPEDEFS //
//////////////
struct VertexInputType
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
};
//The PixelInputType has two extra texture coordinate inputs for the reflection texture coordinates and the refraction texture coordinates.

struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
	float4 reflectionPosition : TEXCOORD1;
	float4 refractionPosition : TEXCOORD2;
};


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType WaterVertexShader(VertexInputType input)
{
	PixelInputType output;
	matrix reflectProjectWorld;
	matrix viewProjectWorld;


	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(input.position, worldMatrix);
	output.position = mul(output.position, viewMatrix);
	output.position = mul(output.position, projectionMatrix);

	// Store the texture coordinates for the pixel shader.
	output.tex = input.tex;
	//Create the reflection projection world matrix just like the reflection tutorial and calculate the reflection coordinates from it.

		// Create the reflection projection world matrix.
	reflectProjectWorld = mul(reflectionMatrix, projectionMatrix);
	reflectProjectWorld = mul(worldMatrix, reflectProjectWorld);

	// Calculate the input position against the reflectProjectWorld matrix.
	output.reflectionPosition = mul(input.position, reflectProjectWorld);
	//Refraction coordinates are calculated in the same way as the reflection coordinates except that we use a view projection world matrix for them.

		// Create the view projection world matrix for refraction.
	viewProjectWorld = mul(viewMatrix, projectionMatrix);
	viewProjectWorld = mul(worldMatrix, viewProjectWorld);

	// Calculate the input position against the viewProjectWorld matrix.
	output.refractionPosition = mul(input.position, viewProjectWorld);

	return output;
}



////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 WaterPixelShader(PixelInputType input) : SV_Target
{
	float2 reflectTexCoord;
	float2 refractTexCoord;
	float4 normalMap;
	float3 normal;
	float4 reflectionColor;
	float4 refractionColor;
	float4 color;
	//Just like the translate shader tutorial we use a translation variable updated each frame to move the water normal map texture along the Y axis to simulate motion.

	// Move the position the water normal is sampled from to simulate moving water.	
	input.tex.y += waterTranslation;
	//Convert both the reflection and refraction coordinates into texture coordinates in the - 1 to + 1 range.

	// Calculate the projected reflection texture coordinates.
	reflectTexCoord.x = input.reflectionPosition.x / input.reflectionPosition.w / 2.0f + 0.5f;
	reflectTexCoord.y = -input.reflectionPosition.y / input.reflectionPosition.w / 2.0f + 0.5f;

	// Calculate the projected refraction texture coordinates.
	refractTexCoord.x = input.refractionPosition.x / input.refractionPosition.w / 2.0f + 0.5f;
	refractTexCoord.y = -input.refractionPosition.y / input.refractionPosition.w / 2.0f + 0.5f;
	//Sample the normal for this pixel from the normal map and expand the range to be in the - 1 to + 1 range.

	// Sample the normal from the normal map texture.
	normalMap = normalTexture.Sample(SampleType, input.tex);

	// Expand the range of the normal from (0,1) to (-1,+1).
	normal = (normalMap.xyz * 2.0f) - 1.0f;
	//Now distort the reflection and refraction coordinates by the normal map value.This creates the rippling effect by using the normal transitioning from - 1 to + 1 to distort our view just as water waves distort light.The normal map value is multiplied by the reflectRefractScale to make the ripples less pronounced and more natural looking.

	// Re-position the texture coordinate sampling position by the normal map value to simulate the rippling wave effect.
	reflectTexCoord = reflectTexCoord + (normal.xy * reflectRefractScale);
	refractTexCoord = refractTexCoord + (normal.xy * reflectRefractScale);
	//Now sample the reflection and refraction pixel based on the updated texture sampling coordinates.

	// Sample the texture pixels from the textures using the updated texture coordinates.
	reflectionColor = reflectionTexture.Sample(SampleType, reflectTexCoord);
	refractionColor = refractionTexture.Sample(SampleType, refractTexCoord);
	//Finally combine the reflection and refraction pixel using a linear interpolation.

	// Combine the reflection and refraction results for the final color.
	color = lerp(reflectionColor, refractionColor, 0.6f);

	return color;
}


////////////////////////////////////////////////////////////////////////////////
// Technique
////////////////////////////////////////////////////////////////////////////////
technique10 WaterTechnique
{
	pass pass0
	{
		SetVertexShader(CompileShader(VS_SHADERMODEL, WaterVertexShader()));
		SetPixelShader(CompileShader(PS_SHADERMODEL, WaterPixelShader()));
		SetGeometryShader(NULL);
	}
}
