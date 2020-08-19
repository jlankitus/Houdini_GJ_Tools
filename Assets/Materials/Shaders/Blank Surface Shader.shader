// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blank Surface Shader"
{
	Properties
	{
		_Globalopacity1("Global opacity", Range( 0 , 1)) = 1
		_Maintexture1("Main texture", 2D) = "black" {}
		_Maintextureintensity1("Main texture intensity", Float) = 1
		_Mainpanningspeed1("Main panning speed", Vector) = (0,0,0,0)
		[Toggle]_Invertmaintexture1("Invert main texture", Range( 0 , 1)) = 0
		[HDR]_Maincolor1("Main color", Color) = (0.7941176,0.1284602,0.1284602,0.666)
		[HDR]_Edgecolor1("Edge color", Color) = (0.7941176,0.1284602,0.1284602,0.666)
		_Bias1("Bias", Float) = 0
		_Scale1("Scale", Float) = 1
		_Power1("Power", Range( 0 , 5)) = 2
		_Innerfresnelintensity1("Inner fresnel intensity", Range( 0 , 1)) = 0
		_Outerfresnelintensity1("Outer fresnel intensity", Range( 0 , 1)) = 1
		[Toggle]_Enabledistortion1("Enable distortion", Range( 0 , 1)) = 0
		_Distortionscale1("Distortion scale", Range( 0 , 10)) = 1
		_Distortionspeed1("Distortion speed", Range( 0 , 5)) = 1
		_Extraroughness1("Extra roughness", Range( 0 , 10)) = 0
		[Toggle]_Enablepulsation1("Enable pulsation", Range( 0 , 1)) = 0
		_Pulsephase1("Pulse phase", Float) = 0
		_Pulsefrequency1("Pulse frequency", Float) = 3
		_Pulseamplitude1("Pulse amplitude", Float) = 1
		_Pulseoffset1("Pulse offset", Float) = 0
		[Toggle]_Enablenoise1("Enable noise", Range( 0 , 1)) = 0
		_Sharpennoise1("Sharpen noise", Range( 0 , 1)) = 0
		_Noisescale1("Noise scale", Float) = 50
		_Noisespeed1("Noise speed", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Front
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform float _Enabledistortion1;
		uniform float _Extraroughness1;
		uniform float _Distortionspeed1;
		uniform float _Distortionscale1;
		uniform float _Enablepulsation1;
		uniform float _Pulsefrequency1;
		uniform float _Pulsephase1;
		uniform float _Pulseamplitude1;
		uniform float _Pulseoffset1;
		uniform float _Globalopacity1;
		uniform float _Outerfresnelintensity1;
		uniform float _Bias1;
		uniform float _Scale1;
		uniform float _Power1;
		uniform float _Innerfresnelintensity1;
		uniform float4 _Edgecolor1;
		uniform float3 _Noisespeed1;
		uniform float _Noisescale1;
		uniform float _Enablenoise1;
		uniform float _Sharpennoise1;
		uniform float _Maintextureintensity1;
		uniform float _Invertmaintexture1;
		uniform sampler2D _Maintexture1;
		uniform float2 _Mainpanningspeed1;
		uniform float4 _Maincolor1;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float simplePerlin3D91 = snoise( ( ( _Extraroughness1 * ase_vertex3Pos ) + ase_vertexNormal + ( _Distortionspeed1 * _Time.y ) ) );
			float temp_output_75_0 = ( _Distortionscale1 / 100.0 );
			float3 VertexOut113 = ( ( _Enabledistortion1 * ( ase_vertexNormal * (( temp_output_75_0 * -1.0 ) + (simplePerlin3D91 - 0.0) * (temp_output_75_0 - ( temp_output_75_0 * -1.0 )) / (1.0 - 0.0)) ) ) + ( _Enablepulsation1 * ( ase_vertexNormal * (sin( (_Time.y*_Pulsefrequency1 + _Pulsephase1) )*( _Pulseamplitude1 / 10.0 ) + _Pulseoffset1) ) ) );
			v.vertex.xyz += VertexOut113;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV28 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode28 = ( _Bias1 + _Scale1 * pow( 1.0 - fresnelNdotV28, _Power1 ) );
			float clampResult63 = clamp( ( ( _Outerfresnelintensity1 * fresnelNode28 ) + ( ( 1.0 - fresnelNode28 ) * _Innerfresnelintensity1 ) ) , 0.0 , 1.0 );
			float FresnelMask80 = clampResult63;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float simplePerlin3D10 = snoise( ( ( _Noisespeed1 * _Time.y ) + ( _Noisescale1 * ase_vertex3Pos ) ) );
			float temp_output_25_0 = ( ( ( simplePerlin3D10 + _Enablenoise1 ) * _Enablenoise1 ) + ( 1.0 - _Enablenoise1 ) );
			float temp_output_3_0_g1 = ( 1.0 - temp_output_25_0 );
			float Noise72 = ( ( temp_output_25_0 * ( 1.0 - _Sharpennoise1 ) ) + ( _Sharpennoise1 * ( 1.0 - saturate( ( temp_output_3_0_g1 / fwidth( temp_output_3_0_g1 ) ) ) ) ) );
			float4 FresnelOut96 = ( FresnelMask80 * _Edgecolor1 * Noise72 );
			float2 panner30 = ( _Time.y * _Mainpanningspeed1 + i.uv_texcoord);
			float3 desaturateInitialColor42 = tex2D( _Maintexture1, panner30 ).rgb;
			float desaturateDot42 = dot( desaturateInitialColor42, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar42 = lerp( desaturateInitialColor42, desaturateDot42.xxx, 1.0 );
			float4 MainTexOut98 = ( _Maintextureintensity1 * float4( ( ( _Invertmaintexture1 * ( 1.0 - desaturateVar42 ) ) + ( ( 1.0 - _Invertmaintexture1 ) * desaturateVar42 ) ) , 0.0 ) * _Maincolor1 );
			float4 EmissionOut111 = ( FresnelOut96 + Noise72 + MainTexOut98 );
			float4 temp_output_122_0 = ( _Globalopacity1 * EmissionOut111 );
			o.Albedo = temp_output_122_0.rgb;
			o.Emission = temp_output_122_0.rgb;
			o.Alpha = ( _Globalopacity1 * FresnelMask80 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18300
436;73;1012;1402;-232.2678;473.2869;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;1;-2018.342,465.734;Inherit;False;2246.313;660.4777;Noise;22;72;59;44;41;40;38;32;31;25;18;13;12;11;10;9;8;7;6;5;4;3;2;Noise;0.7782524,0.5607843,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;2;-1968.342,866.2759;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;3;-1949.977,665.9628;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;4;-1950.783,515.7339;Float;False;Property;_Noisespeed1;Noise speed;30;0;Create;True;0;0;False;0;False;0,0,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;5;-1970.757,777.3714;Float;False;Property;_Noisescale1;Noise scale;29;0;Create;True;0;0;False;0;False;50;50;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1762.805,779.4714;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1754.221,608.3987;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-1594.164,658.9426;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;10;-1473.468,654.111;Inherit;False;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1547.233,766.5715;Float;False;Property;_Enablenoise1;Enable noise;26;1;[Toggle];Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1273.33,658.0076;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;12;-1218.014,804.8351;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;14;-1853.484,-1143.88;Inherit;False;2077.09;620.5684;Main texture;16;98;90;81;77;76;67;66;60;52;45;42;36;30;22;21;19;Main texture;1,0.8264706,0.5661765,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;16;300.73,727.7817;Inherit;False;1761.348;598.0276;Fresnel;16;96;87;86;80;78;63;50;48;47;35;34;33;28;24;20;17;Fresnel;0.4632353,0.7334687,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1155.6,681.4957;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;469.252,887.334;Float;False;Property;_Bias1;Bias;7;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-1007.263,781.6184;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-985.0271,1015.619;Float;False;Constant;_Float2;Float 1;30;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;19;-1774.12,-654.2756;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;461.626,975.763;Float;False;Property;_Scale1;Scale;8;0;Create;True;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;358.453,1065.798;Float;False;Property;_Power1;Power;9;0;Create;True;0;0;False;0;False;2;1.2;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;22;-1800.326,-920.5112;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;21;-1803.484,-790.0911;Float;False;Property;_Mainpanningspeed1;Main panning speed;3;0;Create;True;0;0;False;0;False;0,0;0,-1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;31;-756.3801,914.488;Float;False;Property;_Sharpennoise1;Sharpen noise;27;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;30;-1560.704,-809.9007;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;32;-802.396,1000.97;Inherit;False;Step Antialiasing;-1;;1;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;28;662.0859,893.7471;Inherit;True;Standard;TangentNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;34;957.366,943.5885;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;677.407,1124.078;Float;False;Property;_Innerfresnelintensity1;Inner fresnel intensity;10;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;38;-583.6279,999.7506;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;36;-1325.521,-805.949;Inherit;True;Property;_Maintexture1;Main texture;1;0;Create;True;0;0;False;0;False;-1;None;05e1fe41cc7b13949a096d6310138337;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;33;653.963,792.7622;Float;False;Property;_Outerfresnelintensity1;Outer fresnel intensity;11;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;39;367.226,-1361.517;Inherit;False;2186.53;1179.224;Vertex manipulation;8;113;112;110;108;106;102;53;49;Vertex manipulation;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;40;-490.1111,841.6548;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;49;401.233,-1242.09;Inherit;False;1259.065;659.0585;Distortion;15;107;100;97;93;91;75;74;70;69;64;62;57;56;55;54;;1,0.3975925,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-270.395,783.8503;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;961.803,838.7546;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;42;-1034.805,-803.9863;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1043.512,-1056;Float;False;Property;_Invertmaintexture1;Invert main texture;4;1;[Toggle];Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-279.3911,921.0502;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;1113.703,957.2896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-113.0959,843.6504;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;55;444.816,-724.0029;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;50;1255.92,898.428;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;60;-766.8391,-957.5961;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;54;415.8409,-1106.909;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;421.674,-805.8946;Float;False;Property;_Distortionspeed1;Distortion speed;19;0;Create;True;0;0;False;0;False;1;0.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;52;-767.304,-881.9962;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;53;412.198,-544.767;Inherit;False;1437.686;328.0779;Pulsation;10;104;95;94;92;89;84;83;82;65;61;;0.990566,0.9615986,0.2663314,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;57;417.9769,-1189.515;Float;False;Property;_Extraroughness1;Extra roughness;20;0;Create;True;0;0;False;0;False;0;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;669.183,-684.6638;Float;False;Property;_Distortionscale1;Distortion scale;18;0;Create;True;0;0;False;0;False;1;2.03;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;444.464,-371.224;Float;False;Property;_Pulsephase1;Pulse phase;22;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;63;1391.239,893.1946;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-543.4221,-995.9642;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-0.7919922,840.8512;Float;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;751.094,-1008.959;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;709.306,-795.4771;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;444.16,-453.7971;Float;False;Property;_Pulsefrequency1;Pulse frequency;23;0;Create;True;0;0;False;0;False;3;9.79;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-540.822,-894.5641;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;64;417.005,-958.2612;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;82;910.236,-397.6466;Float;False;Property;_Pulseamplitude1;Pulse amplitude;24;0;Create;True;0;0;False;0;False;1;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;1413.475,1181.369;Inherit;False;72;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;77;-757.915,-730.3131;Float;False;Property;_Maincolor1;Main color;5;1;[HDR];Create;True;0;0;False;0;False;0.7941176,0.1284602,0.1284602,0.666;1,0.009792074,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;1542.789,896.6152;Float;False;FresnelMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;909.902,-944.2952;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;83;954.131,-311.3322;Float;False;Constant;_Float1;Float 0;24;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;75;964.83,-680.4425;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-394.373,-955.0982;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;84;700.5969,-468.6059;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;81;-551.7439,-1093.881;Float;False;Property;_Maintextureintensity1;Main texture intensity;2;0;Create;True;0;0;False;0;False;1;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;86;1061.487,1097.092;Float;False;Property;_Edgecolor1;Edge color;6;1;[HDR];Create;True;0;0;False;0;False;0.7941176,0.1284602,0.1284602,0.666;1,0,0.009601116,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-228.1389,-983.0612;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;1607.167,1077.46;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;94;1118.067,-297.6463;Float;False;Property;_Pulseoffset1;Pulse offset;25;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;91;1034.823,-949.1472;Inherit;True;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;92;1112.131,-394.3322;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;1176.795,-727.3737;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;89;896.8469,-467.0471;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;97;1246.241,-934.6782;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;96;1811.791,1070.959;Float;False;FresnelOut;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;98;-19.39404,-986.9542;Float;False;MainTexOut;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;95;1326.309,-459.7687;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;100;1237.186,-1088.901;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;1635.698,-491.6758;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;1303.559,175.8512;Inherit;False;72;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;1349.929,280.0884;Inherit;False;98;MainTexOut;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;102;1708.36,-1061.81;Float;False;Property;_Enabledistortion1;Enable distortion;17;1;[Toggle];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;1682.263,-646.2493;Float;False;Property;_Enablepulsation1;Enable pulsation;21;1;[Toggle];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;1436.594,-999.8342;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;1391.162,79.85236;Inherit;False;96;FresnelOut;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;1583.576,123.7531;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;2023.754,-509.2661;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;2011.598,-1018.236;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;111;1783.545,120.6037;Float;False;EmissionOut;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;112;2197.853,-792.856;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;564.7,265.6749;Inherit;False;80;FresnelMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;114;341.343,-1866.058;Inherit;False;1030.406;372.375;Experimental;7;126;125;124;123;121;120;118;Experimental;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;566.377,149.9732;Inherit;False;111;EmissionOut;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;15;-1856.171,-297.0171;Inherit;False;2100.483;666.5062;Secondary texture;16;99;88;85;79;73;71;68;58;51;46;43;37;29;27;26;23;Secondary texture;0.4411765,1,0.5837727,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;2323.841,-797.7869;Float;False;VertexOut;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;117;497.371,67.78266;Float;False;Property;_Globalopacity1;Global opacity;0;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;767.5709,360.8268;Inherit;False;113;VertexOut;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;29;-1533.366,49.85582;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;817.8469,72.89046;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;118;391.343,-1816.058;Float;False;Constant;_Float3;Float 2;32;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;596.8989,-1707.511;Float;False;Constant;_Float4;Float 3;30;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;0.06787109,-124.535;Float;False;SecondaryTexOut;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;27;-1727.855,215.4905;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;124;534.323,-1815.354;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;126;932.626,-1746.073;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;125;1106.749,-1794.229;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-520.4351,-154.2511;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StepOpNode;121;740.005,-1731.263;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;37;-1292.839,20.28822;Inherit;True;Property;_Secondarytexture1;Secondary texture;12;0;Create;True;0;0;False;0;False;-1;None;693d3cca90d55c741b729cf055d4f0a6;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;796.244,249.7809;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1020.342,-210.6121;Float;False;Property;_Invertsecondarytexture1;Invert secondary texture;15;1;[Toggle];Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;46;-1002.849,11.40993;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;51;-744.3171,-40.28297;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;-1822.17,-30.75697;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;58;-743.8521,-115.883;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;26;-1829.647,88.9215;Float;False;Property;_Secondarypanningspeed1;Secondary panning speed;14;0;Create;True;0;0;False;0;False;0,0;0,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-371.386,-113.385;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;73;-529.334,53.24632;Float;False;Property;_Secondarycolor1;Secondary color;16;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;1,0.2627451,0.5851504,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;-213.2109,-119.2019;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-540.8701,-236.4355;Float;False;Property;_Secondarytextureintensity1;Secondary texture intensity;13;0;Create;True;0;0;False;0;False;1;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;812.6949,-1608.683;Float;False;Property;_Sharpennn1;Sharpennn;28;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-517.835,-52.85107;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1029.754,62.1889;Float;False;True;-1;7;ASEMaterialInspector;0;0;Standard;Blank Surface Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Front;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;0;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;5;0
WireConnection;7;1;2;0
WireConnection;6;0;4;0
WireConnection;6;1;3;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;10;0;8;0
WireConnection;11;0;10;0
WireConnection;11;1;9;0
WireConnection;12;0;9;0
WireConnection;13;0;11;0
WireConnection;13;1;9;0
WireConnection;25;0;13;0
WireConnection;25;1;12;0
WireConnection;30;0;22;0
WireConnection;30;2;21;0
WireConnection;30;1;19;0
WireConnection;32;1;25;0
WireConnection;32;2;18;0
WireConnection;28;1;17;0
WireConnection;28;2;24;0
WireConnection;28;3;20;0
WireConnection;34;0;28;0
WireConnection;38;0;32;0
WireConnection;36;1;30;0
WireConnection;40;0;31;0
WireConnection;41;0;25;0
WireConnection;41;1;40;0
WireConnection;47;0;33;0
WireConnection;47;1;28;0
WireConnection;42;0;36;0
WireConnection;44;0;31;0
WireConnection;44;1;38;0
WireConnection;48;0;34;0
WireConnection;48;1;35;0
WireConnection;59;0;41;0
WireConnection;59;1;44;0
WireConnection;50;0;47;0
WireConnection;50;1;48;0
WireConnection;60;0;45;0
WireConnection;52;0;42;0
WireConnection;63;0;50;0
WireConnection;67;0;45;0
WireConnection;67;1;52;0
WireConnection;72;0;59;0
WireConnection;69;0;57;0
WireConnection;69;1;54;0
WireConnection;70;0;56;0
WireConnection;70;1;55;0
WireConnection;66;0;60;0
WireConnection;66;1;42;0
WireConnection;80;0;63;0
WireConnection;74;0;69;0
WireConnection;74;1;64;0
WireConnection;74;2;70;0
WireConnection;75;0;62;0
WireConnection;76;0;67;0
WireConnection;76;1;66;0
WireConnection;84;0;55;0
WireConnection;84;1;61;0
WireConnection;84;2;65;0
WireConnection;90;0;81;0
WireConnection;90;1;76;0
WireConnection;90;2;77;0
WireConnection;87;0;80;0
WireConnection;87;1;86;0
WireConnection;87;2;78;0
WireConnection;91;0;74;0
WireConnection;92;0;82;0
WireConnection;92;1;83;0
WireConnection;93;0;75;0
WireConnection;89;0;84;0
WireConnection;97;0;91;0
WireConnection;97;3;93;0
WireConnection;97;4;75;0
WireConnection;96;0;87;0
WireConnection;98;0;90;0
WireConnection;95;0;89;0
WireConnection;95;1;92;0
WireConnection;95;2;94;0
WireConnection;104;0;64;0
WireConnection;104;1;95;0
WireConnection;107;0;100;0
WireConnection;107;1;97;0
WireConnection;109;0;105;0
WireConnection;109;1;103;0
WireConnection;109;2;101;0
WireConnection;108;0;106;0
WireConnection;108;1;104;0
WireConnection;110;0;102;0
WireConnection;110;1;107;0
WireConnection;111;0;109;0
WireConnection;112;0;110;0
WireConnection;112;1;108;0
WireConnection;113;0;112;0
WireConnection;29;0;23;0
WireConnection;29;2;26;0
WireConnection;29;1;27;0
WireConnection;122;0;117;0
WireConnection;122;1;116;0
WireConnection;99;0;88;0
WireConnection;124;0;118;0
WireConnection;124;1;91;0
WireConnection;126;0;121;0
WireConnection;125;0;124;0
WireConnection;125;1;126;0
WireConnection;125;2;120;0
WireConnection;68;0;43;0
WireConnection;68;1;51;0
WireConnection;121;0;124;0
WireConnection;121;1;123;0
WireConnection;37;1;29;0
WireConnection;127;0;117;0
WireConnection;127;1;115;0
WireConnection;46;0;37;0
WireConnection;51;0;46;0
WireConnection;58;0;43;0
WireConnection;79;0;68;0
WireConnection;79;1;71;0
WireConnection;88;0;85;0
WireConnection;88;1;79;0
WireConnection;88;2;73;0
WireConnection;71;0;58;0
WireConnection;71;1;46;0
WireConnection;0;0;122;0
WireConnection;0;2;122;0
WireConnection;0;9;127;0
WireConnection;0;11;119;0
ASEEND*/
//CHKSM=91346252DB402A7B5150BFE974C1C6F86AEAAF14