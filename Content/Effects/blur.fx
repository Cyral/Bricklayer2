float2 Resolution;
float2 Light;
float4 Color;
texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = <Texture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 Coord : TEXCOORD0;
};

float radius = 256;
float antialias = 24;

// Working point light code.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.Coord);

	float2 pos = Resolution * input.Coord;
	float2 ratio = (input.Coord / Resolution);

	float2 lightDirection = Light - pos;
	float lightRadius = 350;
	//float distance = saturate(1 / length(lightDirection));
	float coneAttenuation = saturate(1 - length(lightDirection) / lightRadius);

	float4 shading = coneAttenuation * Color;
	float minLight = .1;
	color.r = color.r * max(shading.r, minLight);
	color.g = color.g * max(shading.g, minLight);
	color.b = color.b * max(shading.b, minLight);

	
	return color;
	};

technique Technique1
{
	pass Pass1
	{
		//PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}