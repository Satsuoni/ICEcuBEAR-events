Shader "Unlit/TrackShader"
{
	Properties
	{
		mytime("mytime", Float) = 0.0
		timeScale("timeScale", Float) = 1.0
		timeWidth("timeWidth", Float) = 0.05
		gradient("Gradient", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		CGPROGRAM
#pragma vertex vert

#pragma fragment frag


		//#include "UnityCG.cginc"
		float mytime;
	float timeScale;
	float timeWidth;
	sampler2D gradient;
	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float4 color: COLOR;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		//float4 color : COLOR;
	};


	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		//o.color = v.color;
		return o;
	}
	//float grad(float tm)
	//{
	//float d
	//return exp(-tm*tm*0.01);
	//	}
	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		//fixed4 col = tex2D(_MainTex, i.uv);
		//float4 cl = i.color;
		float scaledPixelTime = i.uv.x / timeScale;
	    float scaledMyTime = mytime / timeScale;
		float2 timeline = clamp(scaledPixelTime,0,1);
		float4 col = tex2D(gradient, timeline);
		float4 truecol = col * (1 - i.uv.y) + (float4(1, 1, 1, 0) - col)*i.uv.y;
		clip(timeWidth - abs(scaledPixelTime - scaledMyTime + timeWidth));
		return truecol;// i.color*exp(-_Time.x*0.1);
	}
		ENDCG
	}
	}
}