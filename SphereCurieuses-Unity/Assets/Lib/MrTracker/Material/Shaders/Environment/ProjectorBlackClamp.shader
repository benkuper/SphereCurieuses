// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Projector/AdditiveAndCorrectClamp" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_ShadowTex("Projected Image", 2D) = "white" {}
	}
		SubShader{
		Pass{
		Blend One One
		// add color of _ShadowTex to the color in the framebuffer 
		ZWrite Off // don't change depths
		Offset -1, -1 // avoid depth fighting

		CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 

		// User-specified properties
		uniform sampler2D _ShadowTex;
	uniform float4 _ShadowTex_ST;
	// Projector-specific uniforms
	uniform float4x4 unity_Projector; // transformation matrix 
									  // from object space to projector space 

	struct vertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
	};
	struct vertexOutput {
		float4 pos : SV_POSITION;
		float4 posProj : TEXCOORD0;
		// position in projector space
	};

	struct fragmentInput
	{
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
	};

	vertexOutput vert(vertexInput input)
	{
		vertexOutput output;
		//output.uv = input.texcoord.xy * _ShadowTex_ST.xy + _ShadowTex_ST.zw;

		output.posProj = mul(unity_Projector, input.vertex);
		output.pos = UnityObjectToClipPos(input.vertex);
		return output;
	}


	float4 frag(vertexOutput input) : COLOR
	{
		if (input.posProj.w > 0.0) // in front of projector?
		{
			//	o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);

			//1 SOlution ? 
			/*float2 uvmasks = min(input.posProj.xy, 1.0 - input.posProj.xy);
			float mask = min(uvmasks.x, uvmasks.y);
			float coeff = mask < 0.1 ? float3(0.0, 0.0, 0.0) : float3(1.0, 1.0, 1.0);*/

			//float2 uvmasks = min(input.posProj.xy, 1.0 - input.posProj.xy);
			//float mask = min(uvmasks.x, uvmasks.y);

			//if(input.posProj.xy.x > 0.1 && input.posProj.xy.x < 0.9 && input.posProj.xy.y > 0.1 && input.posProj.xy.y < 0.9)
			//	return float4(0.0, 0.0, 0.0, 0.0);
			//else

			//Kill pixel outside uv
			clip(input.posProj.xy / input.posProj.w);
			clip(1.0 - input.posProj.xy / input.posProj.w);
				return tex2D(_ShadowTex,
					input.posProj.xy / input.posProj.w);
				

			// alternatively: return tex2Dproj(  
			//    _ShadowTex, input.posProj);
			//return tex2D(_ShadowTex,
			//	input.posProj.xy / input.posProj.w);


			//Rectangle bof
			//float2 uv = input.posProj.xy * _ShadowTex_ST.xy + _ShadowTex_ST.zw;
			//float4 color = float4(1.0, 0.0, 0.0, 1.0);
			//float4 bg = tex2D(_ShadowTex, input.posProj.xy / input.posProj.w);

			//float ratio = _ScreenParams.x / _ScreenParams.y;
			//float4 bgColor = tex2D(_ShadowTex, input.posProj.xy / input.posProj.w);

			//// outer
			//// x1  y1   x2   y2
			//float4 rect = float4(0.1, 0.01, 0.9, 0.99);
			//float2 hv = step(rect.xy, uv) * step(uv, rect.zw);
			//float onOff = hv.x * hv.y;
			//float4 col1 = lerp(float4(0.0, 0.0, 0.0, 0.0), float4(1.0, 0.0, 0.0, 0.0), onOff); // inner is visible

			//																				   // inner
			//float	thickness1 = (10.0 / _ScreenParams.x) * ratio;
			//float	thickness2 = (10.0 / _ScreenParams.y) * ratio;
			//float4 inner = float4(rect[0] + thickness1,
			//	rect[1] + thickness2,
			//	rect[2] - thickness1,
			//	rect[3] - thickness2);
			//hv = step(inner.xy, uv) * step(uv, inner.zw);
			//onOff = hv.x * hv.y;
			//float4 col2 = lerp(float4(0.0, 0.0, 0.0, 0.0), float4(0.0, 0.0, 0.0, 1.0), onOff);
			//bgColor = lerp(col1, bgColor, onOff);

			//return bgColor + (col2 * col1);
		}
		else // behind projector
		{
			return float4(0.0, 0.0, 0.0, 0.0);
		}
	}

		ENDCG
	}
	}
		// The definition of a fallback shader should be commented out 
		// during development:
		// Fallback "Projector/Light"
}