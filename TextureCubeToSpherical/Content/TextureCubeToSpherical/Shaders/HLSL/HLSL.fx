//#define SHOW_NORMALS
//#define SHOW_FACES

TextureCube cubeTexture 			: register(t0);
SamplerState Sampler			 	: register(s0);

struct PS_IN
{
	float4 Position		: SV_POSITION;
	float4 PositionCS	: TEXCOORD0;
};

PS_IN VS(uint id: SV_VertexID)
{
	PS_IN output = (PS_IN)0;

	float2 tex = float2((id << 1) & 2, id & 2);
	output.Position = float4(tex * float2(2, -2) + float2(-1, 1), 0, 1);
	output.PositionCS = output.Position;

	return output;
}

inline float2 ComputeScreenPosition(float4 pos)
{
	float2 screenPos = pos.xy / pos.w;
	return (0.5f * (float2(screenPos.x, screenPos.y) + 1));
}

float4 PS(PS_IN input) : SV_Target
{
	float2 screenPosition = ComputeScreenPosition(input.PositionCS);
	float theta = -screenPosition.x * 6.28318 - 1.57079;
	float phi = screenPosition.y * 3.1415926 - 1.57079;

	float3 dir;
	dir.x = cos(phi) * cos(theta);
	dir.y = sin(phi);
	dir.z = cos(phi) * sin(theta);
	
	float4 color = cubeTexture.Sample(Sampler, dir);

#ifdef SHOW_NORMALS

	return float4(dir, 1.0);

#else

	#ifdef SHOW_FACES
		float4 faceColors[6];
		faceColors[0] = float4(1.0, 0.0, 0.0, 1.0); //left
		faceColors[1] = float4(0.0, 1.0, 0.0, 1.0); //right
		faceColors[2] = float4(0.0, 0.0, 1.0, 1.0); //bottom
		faceColors[3] = float4(1.0, 1.0, 0.0, 1.0); //top
		faceColors[4] = float4(1.0, 0.0, 1.0, 1.0); //front
		faceColors[5] = float4(0.0, 1.0, 1.0, 1.0); //back

		float ax = abs(dir.x);
		float ay = abs(dir.y);
		float az = abs(dir.z);

		if (ax > ay && ax > az) 
		{
			//x-major
			color *= (dir.x < 0.0 ? faceColors[0] : faceColors[1]);
		}
		else if (ay > ax && ay > az) 
		{
			//y-major
			color *= (dir.y < 0.0 ? faceColors[2] : faceColors[3]);
		}
		else
		{
			//z-major
			color *= (dir.z < 0.0 ? faceColors[4] : faceColors[5]);
		}
	#endif

	return color;
#endif
}
