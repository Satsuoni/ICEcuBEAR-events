Shader "Unlit/TestShader" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag      
#include "UnityCG.cginc"
	

			struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
			float2 uv3 : TEXCOORD2;
			float2 uv4 : TEXCOORD3;
			float2 uv5 : TEXCOORD4;
			float2 uv6 : TEXCOORD5;
			float3 normal : NORMAL;
		};

		struct v2f
		{
			float4 uv : TEXCOORD0;
			float4 uv1 : TEXCOORD1;
			float4 uv2 : TEXCOORD2;
			float4 vertex : SV_POSITION;
			float4 object : POSITION1;
			half3 worldNormal : TEXCOORD3;
			float3 worldPos : TEXCOORD4;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 xvec;
		float4 yvec;
	

	v2f vert(appdata i) {
		v2f o; 
		o.worldPos = mul(unity_ObjectToWorld, i.vertex);
		o.vertex = UnityObjectToClipPos(i.vertex);
		o.object = i.vertex;
		o.uv.xy = i.uv;
		o.uv.zw = i.uv4;
		o.uv1.xy = i.uv2;
		o.uv1.zw = i.uv5;
		o.uv2.xy = i.uv3;
		o.uv2.zw = i.uv6;

		o.worldNormal = UnityObjectToWorldNormal(i.normal);
		return o;
	}
	float3 colorFromDist(float3 coords, float3 vert, float2 clr)
	{
		//clr.x is flux and clr.y is potentially hue/redshift, nothing now...
		float df = dot(coords, normalize(vert));
		float angl = 2 * (1 - df);
		float maxangl = 0.01*(1 + 0.5*clr.x);
		maxangl = maxangl * maxangl;
		const float3 base = float3(0.9, 0.9, 1);
		return step(angl, maxangl)*base*(1 - angl / maxangl);
	}

	fixed4 frag(v2f i) : SV_Target{
		//fixed4 c = tex2D(_MainTex, i.uv);

		float3 coords = normalize(i.object.rgb);// +float4(0.5,0.5,0.5,0.5);

		float3 v1;
		float deg = step(0.5, i.uv.y);
		v1.x = i.uv.x;
		v1.y = 0;
		v1.z = (i.uv.y - deg * 0.5) * 4 - 1;
		v1.y = (1 - deg * 2)*sqrt(1 - dot(v1.xz, v1.xz));

		float3 v2;
		deg = step(0.5, i.uv1.y);
		v2.x = i.uv1.x;
		v2.y = 0;
		v2.z = (i.uv1.y - deg * 0.5) * 4 - 1;
		v2.y = (1 - deg * 2)*sqrt(1 - dot(v2.xz, v2.xz));

		float3 v3;
		deg = step(0.5, i.uv2.y);
		v3.x = i.uv2.x;
		v3.y = 0;
		v3.z = (i.uv2.y - deg * 0.5) * 4 - 1;
		v3.y = (1 - deg * 2)*sqrt(1 - dot(v3.xz, v3.xz));


		float3 clr1 = colorFromDist(coords, v1, i.uv.zw);
		float3 clr2 = colorFromDist(coords, v2, i.uv1.zw);
		float3 clr3 = colorFromDist(coords, v3, i.uv2.zw);
		float4 c;
		c.rgb= max(clr1, max(clr2, clr3));
		c.a = 1;
	//c = c * step(0.1, length(c.rgb)/3.0);
	//c.rgb = 1;// clamp(dot(i.worldNormal, normalize(_WorldSpaceCameraPos - i.worldPos)), 0, 1);//mul(UNITY_MATRIX_V, i.worldNormal);
	float3 ptr = normalize(_WorldSpaceCameraPos - i.worldPos);
	float ca =  clamp(dot(normalize(i.worldNormal), ptr), 0, 1.0);
	ca = ca * ca;
	//if (ca > 0.5) ca = 0;
	c.a = ca;
	return c;
	}
		ENDCG
	}
	}
}
