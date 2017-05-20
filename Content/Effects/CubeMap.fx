

/*-------------------------------------
 * UNIFORMS
 *-----------------------------------*/

uniform extern float4x4 Model;
uniform extern float4x4 Proj;
uniform extern float4x4 View;

// TODO: Would prefer an array here but MonoGame doesn't seem to support it? See EnvMapMaterial.cs
//       etc.
extern uniform texture EnvTex0;
extern uniform texture EnvTex1;
extern uniform texture EnvTex2;
extern uniform texture EnvTex3;
extern uniform texture EnvTex4;
extern uniform texture EnvTex5;

// NOTE: MUST BE 512x512 PIXELS!!
extern uniform texture BumpMap;

sampler envMap0 = sampler_state { Texture = <EnvTex0>; mipfilter = LINEAR; };
sampler envMap1 = sampler_state { Texture = <EnvTex1>; mipfilter = LINEAR; };
sampler envMap2 = sampler_state { Texture = <EnvTex2>; mipfilter = LINEAR; };
sampler envMap3 = sampler_state { Texture = <EnvTex3>; mipfilter = LINEAR; };
sampler envMap4 = sampler_state { Texture = <EnvTex4>; mipfilter = LINEAR; };
sampler envMap5 = sampler_state { Texture = <EnvTex5>; mipfilter = LINEAR; };
sampler bumpMap = sampler_state { Texture = <BumpMap>; mipfilter = LINEAR; };

extern uniform float3 CamPos;

// TODO: These should really be uniforms.
static const float  Shininess = 10.0f;
static const float3 LightPos  = float3(0.0, 10.0, 1.0);

/*-------------------------------------
 * STRUCTS
 *-----------------------------------*/

struct PS_OUTPUT {
    float4 color : SV_TARGET;
};

struct VS_INPUT {
    float4 pos      : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 norm     : NORMAL0;
};

struct VS_OUTPUT {
    float4 screenPos : POSITION0;
    float3 worldPos  : TEXCOORD1;
    float2 texCoord  : TEXCOORD0;
    float3 norm      : TEXCOORD2;
};

/*-------------------------------------
 * FUNCTIONS
 *-----------------------------------*/

float3 bump(float2 uv) {
    return tex2D(bumpMap, uv*2.0).rgb;
    /* float2 a = uv; */
    /* float2 b = uv + float2(1.0f/512.0, 0.0f/512.0); */
    /* float2 c = uv + float2(0.0f/512.0, 1.0f/512.0); */

    /* float sa = tex2D(bumpMap, a).r; */
    /* float sb = tex2D(bumpMap, b).r; */
    /* float sc = tex2D(bumpMap, c).r; */

    /* float3 disp = normalize(float3(sb - sa, sc - sa, 1.0)); */

    /* return disp; */
}

// Map point to uv by mapping it onto the inside of a cube.
float2 cubeMap(float3 p, out int i) {
    float aX = abs(p.x);
    float aY = abs(p.y);
    float aZ = abs(p.z);

    int xP = p.x > 0 ? 1 : 0;
    int yP = p.y > 0 ? 1 : 0;
    int zP = p.z > 0 ? 1 : 0;

    float mA, uc, vc;

    // TODO: Basically, find out which direction the 'ray' is pointing, select the appropriate face
    // (i) and find the UV coordinate for that face. This function needs to be cleaned up.

    if (xP == 1 && aX >= aY && aX >= aZ) {
        mA = aX;
        uc = -p.z;
        vc = p.y;
        i  = 0;
    }

    if (xP == 0 && aX >= aY && aX >= aZ) {
        mA = aX;
        uc = p.z;
        vc = p.y;
        i  = 1;
    }

    if (yP == 1 && aY >= aX && aY >= aZ) {
        mA = aY;
        uc = p.x;
        vc = -p.z;
        i  = 2;
    }

    if (yP == 0 && aY >= aX && aY >= aZ) {
        mA = aY;
        uc = p.x;
        vc = p.z;
        i  = 3;
    }

    if (zP == 1 && aZ >= aX && aZ >= aY) {
        mA = aZ;
        uc = p.x;
        vc = p.y;
        i  = 4;
    }

    if (zP == 0 && aZ >= aX && aZ >= aY) {
        mA = aZ;
        uc = -p.x;
        vc = p.y;
        i  = 5;
    }

    return float2(0.5f * (uc/mA + 1.0), 0.5f * (vc / mA + 1.0));
}

void psMain(in VS_OUTPUT psIn, out PS_OUTPUT psOut) {
    float3 n = normalize(psIn.norm + 0.2f*bump(psIn.texCoord));
    float3 v = normalize(psIn.worldPos.xyz - CamPos);
    float3 h = reflect(v, n);
    float3 l = normalize(LightPos - psIn.worldPos.xyz);
    float3 r = reflect(l, n);

    // Phong specularity
    float3 s = pow(max(0.0, dot(r, v)), Shininess);
    float3 d = max(0.0, dot(l, v));

    int    i  = -1;
    float2 tc = cubeMap(h.xyz, i);

    // Not very neat, but it seems to do the job. :-)
    psOut.color = float4(1.0, 0.0, 1.0, 1.0);
         if (i == 0) psOut.color = tex2D(envMap0, tc).rgba;
    else if (i == 1) psOut.color = tex2D(envMap1, tc).rgba;
    else if (i == 2) psOut.color = tex2D(envMap2, tc).rgba;
    else if (i == 3) psOut.color = tex2D(envMap3, tc).rgba;
    else if (i == 4) psOut.color = tex2D(envMap4, tc).rgba;
    else if (i == 5) psOut.color = tex2D(envMap5, tc).rgba;

    float a = 0.5 - 0.5*s;
    psOut.color = (1.0-a)*psOut.color + a*float4(d*float3(0.5, 0.5, 0.5), 1.0);

    psOut.color += float4(s*float3(1.0, 1.0, 1.0), 0.0);
}

void vsMain(in VS_INPUT vsIn, out VS_OUTPUT vsOut) {
    float4 worldPos = mul(vsIn.pos, Model);
    float4 viewPos  = mul(worldPos, View);
    vsOut.screenPos = mul(viewPos, Proj);
    vsOut.worldPos  = worldPos.xyz;
    vsOut.texCoord  = vsIn.texCoord;
    vsOut.norm      = normalize(mul(float4(vsIn.norm.xyz, 0.0), Model).xyz);
}

technique T1 {
    pass P0 {
        PixelShader = compile ps_3_0 psMain();
        VertexShader = compile vs_3_0 vsMain();
    }
}
