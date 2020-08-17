
Shader "Clayxels/ClayxelDebugShader" {
	Properties {
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType" = "Opaque" }
                   
		Pass {
			Cull back
			ZWrite On
			Lighting Off
			AlphaToMask On

			CGPROGRAM
	      #include "UnityCG.cginc"
	      #pragma target 5.0
			#pragma vertex vert
	      #pragma geometry geom
	      #pragma fragment frag

			StructuredBuffer<float3> debugChunkPoints;

			float4x4 objectMatrix;
			float3 chunkCenter;
			float chunkSize = 0.0;
			float splatRadius = 0.01;

			struct VertexData{
				float4 pos: SV_POSITION;
				float4 col: COLOR;
				// uint solidId: BLENDINDICES;
			};

			struct GeomData {
				float4 pos: SV_POSITION;
				float4 col: COLOR;
				float2 tex: TEXCOORD0;
				// uint solidId: BLENDINDICES;
			};

			struct FragData{
				fixed4 color: SV_TARGET0;
			};

			VertexData vert(uint id : SV_VertexID, uint inst : SV_InstanceID){
				VertexData outVertex;

				float3 p = debugChunkPoints[id];
				
				outVertex.pos = float4(p, 1.0);
				outVertex.col = float4(1.0, 1.0, 1.0, 1.0);
				// outVertex.solidId = 1;

				return outVertex;
			}
			
			[maxvertexcount(4)]
			void geom(point VertexData inVertex[1], inout TriangleStream<GeomData> outGeo){
				GeomData outVertex;
				// outVertex.solidId = inVertex[0].solidId;
				// if(outVertex.solidId == 0){
				// 	return;
				// }

				outVertex.col = inVertex[0].col;
				
				float4 p = inVertex[0].pos;
				float3 vec1 = float3(unity_CameraToWorld[0][1], unity_CameraToWorld[1][1], unity_CameraToWorld[2][1]) * 0.005;
				float3 vec2 = float3(unity_CameraToWorld[0][0], unity_CameraToWorld[1][0], unity_CameraToWorld[2][0]) * 0.005;

				outVertex.pos = UnityObjectToClipPos(p + (-vec1 + vec2));
				outVertex.tex = float2(1.0, 0.0);
				outGeo.Append(outVertex);

				outVertex.pos = UnityObjectToClipPos(p + (-vec1 - vec2));
				outVertex.tex = float2(1.0, 1.0);
				outGeo.Append(outVertex);

				outVertex.pos = UnityObjectToClipPos(p + (vec1 + vec2));
				outVertex.tex = float2(0.0, 0.0);
				outGeo.Append(outVertex);

				outVertex.pos = UnityObjectToClipPos(p + (vec1 - vec2));
				outVertex.tex = float2(0.0, 1.0);
				outGeo.Append(outVertex);
			}

			FragData frag(GeomData inVertex){
				FragData outData;
				outData.color = inVertex.col;

       		return outData;
       	}

			ENDCG
		}
	}
}