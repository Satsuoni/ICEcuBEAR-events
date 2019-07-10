// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/ColoredUnsurf" {
	Properties{
		_color("color",Color) = (1,1,1,1)
	}
	SubShader
	{
			Tags { "RenderType" = "Opaque"   }
			LOD 300
	Pass
	{

		Tags{ "LightMode" = "Vertex" }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
       #pragma multi_compile_instancing
        #include "UnityCG.cginc"
		struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

		UNITY_INSTANCING_BUFFER_START(MyProperties)
		UNITY_DEFINE_INSTANCED_PROP(fixed4,_color)
		#define _color_arr MyProperties
		UNITY_INSTANCING_BUFFER_END(MyProperties)

		//ed4 _color;
	

		struct v2f
		{
			float4 pos : SV_POSITION;
			float3 normal : NORMAL;
			float4 c : COLOR;
			float4 posWorld : TEXCOORD1;
		};


		v2f vert(appdata v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			o.posWorld = mul(unity_ObjectToWorld, v.vertex); //Calculate the world position for our point
			o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz); //Calculate the normal
			o.pos = UnityObjectToClipPos(v.vertex); //And the position
			
			//o.pos= float4(1.0, 1.0, 1.0, 1.0);
			o.c = UNITY_ACCESS_INSTANCED_PROP(_color_arr, _color);
			return o;
		}


		fixed4 frag(v2f i) : SV_Target
		{
			
			float3 normalDirection = normalize(i.normal);
			float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
			float rec = 0.2;
			float attenuation = 0.05;
			float3 vert2LightSource = float3(0.0,10.0,0.0);// _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
			float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;
			float4 clr = i.c;
			float4 ambientLighting = (1.0 - rec) * clr; //Ambient component
			float4 diffuseReflection = rec * clr* max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
			float4 specularReflection;
			if (dot(i.normal, lightDirection) < 0.0) //Light on the wrong side - no specular
			{
				specularReflection = float4(0.0, 0.0, 0.0,1.0);
			}
			else
			{
				//Specular component
				specularReflection = attenuation * float4(1.0,1.0,1.0,1.0) * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), 0.05);
			}

			fixed4 color = (ambientLighting + diffuseReflection) + specularReflection; //Texture is not applient on specularReflection
			color.a = 1.0;
			return  color;// float4(0.5, 1.0, 1.0, 1.0);
		  }
		ENDCG
			  }

		
	}
		FallBack Off
}