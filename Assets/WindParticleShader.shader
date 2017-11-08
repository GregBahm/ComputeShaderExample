Shader "Unlit/WindParticleShader"
{
	Properties
	{
		_CubeSize("Cube Size", Vector) = (1,1,1)
		_CardSize("Card Size", Float) = 1
		_Offset("Offset", Vector) = (1,1,1,1)
		_Scale("Scale", Vector) = (1,1,1,1)
		_VelocityScale("Velocity Scale", Float) = 1

	}
	SubShader
	{
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert  
			#pragma fragment frag
			#pragma target 5.0
			
			#include "UnityCG.cginc"

			StructuredBuffer<float4> particles;
			StructuredBuffer<float3> quadPoints; 

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 basePos: TEXCOORD0;
				float3 quadPoint : TEXCOORD1;
			};

			sampler3D MapTexture;
			float _CardSize;
            float4x4 masterTransform;
			float _VelocityScale;
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				o.basePos = particles[inst].yxz;
				float velocity = particles[inst].w;
				float3 newPos = saturate(particles[inst].yxz);
				float4 worldPos = mul(masterTransform, float4(newPos, 1));

				o.quadPoint = quadPoints[id];
				float3 finalQuadPoint = o.quadPoint *_CardSize  *  pow(velocity, _VelocityScale);
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, worldPos) + float4(finalQuadPoint, 0));
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
                
				return float4(i.basePos, 1);
			} 
			ENDCG
		}
	}
}
