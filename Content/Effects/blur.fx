// Parameters
float2 Resolution;

// Texture sampler.
texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = <Texture>;
};

// In/out data.
struct ShaderData
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 Coord : TEXCOORD0;
};

// Blur and darken.
float4 PixelShaderFunction(ShaderData input) : COLOR0
{	
	// Find amount to add/subtract by taking 1 / Resolution
	float2 amount = 1.0f / Resolution;

	// Set to black, with alpha of original pixel.
	float4 color = float4(0, 0, 0, tex2D(TextureSampler, input.Coord.xy).a);
	// Get alpha of each direction.
	float top = tex2D(TextureSampler, input.Coord + float2(0, -amount.y)).a;
	float left = tex2D(TextureSampler, input.Coord + float2(-amount.x, 0)).a;
	float right = tex2D(TextureSampler, input.Coord + float2(0, amount.y)).a;
	float bottom = tex2D(TextureSampler, input.Coord + float2(amount.x, 0)).a;

	// Combine.
	color.a = ((color.a + top + left + right + bottom) / 5) * .25;
	return color;
};

technique MainTechnique
{
	pass MainPass
	{
		// Pixel shader to blur and darken.
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}