Shader "Unlit/I420RGB"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_UTex("U", 2D) = "white" {}
		_VTex("V", 2D) = "white" {}
		//_UVTex ("UV", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
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
			sampler2D _UTex;
			sampler2D _VTex;
			sampler2D _UVTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//不在C#侧做数组的反转，应该在这反转一下uv的y分量即可。
				fixed2 uv = fixed2(i.uv.x,1 - i.uv.y);
				fixed4 ycol = tex2D(_MainTex, uv);
				fixed4 ucol = tex2D(_UTex, uv);
				fixed4 vcol = tex2D(_VTex, uv);
				//fixed4 uvcol = tex2D(_UVTex,uv);

				//如果是使用 Alpha8 的纹理格式写入各分量的值，各分量的值就可以直接取a通道的值
				float r = ycol.a + 1.4022 * vcol.a - 0.7011;
				float g = ycol.a - 0.3456 * ucol.a - 0.7145 * vcol.a + 0.53005;
				float b = ycol.a + 1.771 * ucol.a - 0.8855;


				//如果是使用的RGBA4444的纹理格式写入UV分量，就需要多一道计算
				//才可以得到正确的U V分量的值
				//float yVal = ycol.a;
				//float uVal = (uvcol.r * 15 * 16 + uvcol.g * 15) / 255;
				//float vVal = (uvcol.b * 15 * 16 + uvcol.a * 15) / 255;

				//float r = yVal + 1.4022 * vVal - 0.7011;
				//float g = yVal - 0.3456 * uVal - 0.7145 * vVal + 0.53005;
				//float b = yVal + 1.771 * uVal - 0.8855;

				return fixed4(r,g,b,1);
			}
			ENDCG
		}
	}
}