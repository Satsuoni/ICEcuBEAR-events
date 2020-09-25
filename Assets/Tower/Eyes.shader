Shader "Unlit/Eyes"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float pulse1= cos(_Time.y)+ cos(_Time.y*2)+ cos(_Time.y*0.5);
			    float pulse2 = sin(_Time.y*0.5) + cos(_Time.y * 1.2) + sin(_Time.y*1);
				float2 coords = i.uv;
				float2 offset;
				offset.x = pulse2*cos(pulse1*3.14);
				offset.y = pulse2 * sin(pulse1*3.14);

				coords.x = coords.x +0.1*sin(coords.x * (3+sin(_Time.y)))- _Time.x * 3;
				coords.y = coords.y + 0.2*sin(coords.y * (3 + cos(_Time.y)))+ _Time.y*2;
                fixed4 col = tex2D(_MainTex, coords+offset);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
