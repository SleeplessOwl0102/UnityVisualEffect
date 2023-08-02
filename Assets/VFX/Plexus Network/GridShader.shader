Shader "Custom/Grid"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_PointColor("PointColor", Color) = (1, 1, 1, 1)
		_Thickness("Thickness", Float) = 1
		_MaxDistance("Max Distance", Float) = 1
	}

	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Transparent" 
			"Queue"="Transparent" 
		}

		Pass
		{
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend One One 
			ZTest Always
			ZWrite Off
			
			BlendOp Add, Max

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"

			float4 _Color;
			float _Thickness;
			float _MaxDistance;
			int3 _Dimensions;

			StructuredBuffer<float3> _Positions;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2g
			{
				float4 vertex : POSITION;
			};

			struct g2f 
			{
				float4 vertex : POSITION;
				float4 data : TEXCOORD0; // x,y : uv;  z : distance
			};

			v2g vert (appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				return o;
			}

			void addPoint(float3 source,float alpha, inout TriangleStream<g2f> stream)
			{
				float4 p1 = UnityObjectToClipPos(float4(source, 1));

				float size = 0.005 + 0.01 * alpha;
				float ratial = _ScreenParams.y / _ScreenParams.x;
				float x = size * ratial;
				float y = size ;

				float4 o0 = p1 + float4(-x, -y, 0, 0);
				float4 o1 = p1 + float4(x, -y, 0, 0);
				float4 o2 = p1 + float4(-x, y, 0, 0);
				float4 o3 = p1 + float4(x, y, 0, 0);
				
				alpha = smoothstep(1, 0.6, alpha);
				g2f g[4];

				g[0].vertex = o0;
				g[1].vertex = o1;
				g[2].vertex = o2;
				g[3].vertex = o3;

				g[0].data.xy = float2(-1, -1);
				g[1].data.xy = float2(1, -1);
				g[2].data.xy = float2(-1, 1);
				g[3].data.xy = float2(1, 1);
				
				g[0].data.z = alpha;
				g[1].data.z = alpha;
				g[2].data.z = alpha;
				g[3].data.z = alpha;

				g[0].data.w = 1;
				g[1].data.w = 1;
				g[2].data.w = 1;
				g[3].data.w = 1;

				stream.Append(g[0]);
				stream.Append(g[1]);
				stream.Append(g[2]);
				stream.Append(g[3]);
				stream.RestartStrip();
			}

			float addLine(float3 source, float4 destination, inout TriangleStream<g2f> stream) 
			{
				float4 p1 = UnityObjectToClipPos(float4(source, 1));
				float4 p2 = UnityObjectToClipPos(float4(destination.xyz, 1));

				float2 dir = normalize(p2.xy - p1.xy);
				float2 normal = float2(-dir.y, dir.x);


				float distance = length(source - destination);
				distance = (_MaxDistance - min(distance, _MaxDistance)) / _MaxDistance;
				distance = pow(distance, 7);
				float dis2 = distance;

				_Thickness = _Thickness + _Thickness * distance;

				float4 offset1 = float4(normal * p1.w * _Thickness, 0, 0);
				float4 offset2 = float4(normal * p2.w * _Thickness, 0, 0);

				float4 o1 = p1 + offset1;
				float4 o2 = p1 - offset1;
				float4 o3 = p2 + offset2;
				float4 o4 = p2 - offset2;

				g2f g[4];

				g[0].vertex = o1;
				g[0].data.xy = float2(1, 0);
				g[0].data.z = distance;

				g[1].vertex = o2;
				g[1].data.xy = float2(-1, 0);
				g[1].data.z = distance;

				g[2].vertex = o3;
				g[2].data.xy = float2(1, 0);
				g[2].data.z = distance;

				g[3].vertex = o4;
				g[3].data.xy = float2(-1, 0);
				g[3].data.z = distance;

				g[0].data.w = 0;
				g[1].data.w = 0;
				g[2].data.w = 0;
				g[3].data.w = 0;

				stream.Append(g[0]);
				stream.Append(g[1]);
				stream.Append(g[2]);
				stream.Append(g[3]);
				stream.RestartStrip();

				return dis2;
			}

			float3 positionForCell(int3 cell) 
			{
				int index = cell.x * _Dimensions.z * _Dimensions.y + cell.y * _Dimensions.z + cell.z;
				return _Positions[index];
			}

			float4 positionForCellOffset(int3 cell, int x, int y, int z) 
			{
				int3 offset = int3(x, y, z);
				int3 target = cell + offset;
				bool valid = 
					target.x >= 0 && target.x < _Dimensions.x && 
					target.y >= 0 && target.y < _Dimensions.y && 
					target.z >= 0 && target.z < _Dimensions.z;
				target = cell + offset ;
				return float4( positionForCell(target),valid);
			}

			// Construct lines connecting this cell to each of its neighbors on
			// half of the hemisphere, to prevent duplicate connections.
			[maxvertexcount(4 * 7)]
			void geom(point v2g input[1], inout TriangleStream<g2f> stream)
			{
				float4 vertex = input[0].vertex;
				int3 cell = (int3)vertex.xyz;
				float3 position = positionForCell(cell);

				float pAlpha = 0;
				/*pAlpha += addLine(position, positionForCellOffset(cell, 1, 0, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, -1, 0, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, 1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, -1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, 0, 1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, 0, -1), stream);*/

				pAlpha += addLine(position, positionForCellOffset(cell, 1, 1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 0, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, -1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, -1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 1, 1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 0, 1), stream);

				/*pAlpha += addLine(position, positionForCellOffset(cell, 1, 1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 0, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, -1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, -1, 0), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 1, 1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 0, 1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, -1, 1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, -1, 1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 1, -1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, 0, -1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 1, -1, -1), stream);
				pAlpha += addLine(position, positionForCellOffset(cell, 0, -1, -1), stream);*/
				addPoint(position, pAlpha , stream);
			}

			float4 _PointColor;

			fixed4 frag (g2f i) : SV_Target
			{
				float distance = i.data.z;
				float circle = smoothstep(.9, .3, length(i.data.xy));
			//利用UV来画圆，通过_Radius来调节大小，_CircleFade来调节边缘虚化程序。
				if (i.data.w == 0)
				{
					return _Color * circle * distance;
				}
				else
				{
					return _PointColor * circle * distance;
				}
			}
			ENDCG
		}
	}
}
