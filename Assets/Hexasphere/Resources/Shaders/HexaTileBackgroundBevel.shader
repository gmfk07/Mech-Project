﻿Shader "Hexasphere/HexaTileBackgroundBevel" {
    Properties {
        _MainTex ("Main Texture Array", 2DArray) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AmbientColor ("Ambient Color", Color) = (0,0,0)
        _MinimumLight ("Minimum Light", Float) = 0
        _GradientIntensity ("Gradient Intensity", float) = 0.75
        _ExtrusionMultiplier("Extrusion Multiplier", float) = 1.0
        _Center ("Sphere Center", Vector) = (0,0,0)
        _TileAlpha ("Tile Alpha", Float) = 1
        _BumpMask ("Bump Mask (RGB)y", 2D) = "white" {}
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }


        SubShader{
            Tags { "Queue" = "Geometry-2" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Pass {

                PackageRequirements {
                    "com.unity.render-pipelines.universal"
                }
				Tags { "LightMode" = "UniversalForward" }
                    Offset 2, 2

                        HLSLPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #pragma geometry geom
                        #pragma fragmentoption ARB_precision_hint_fastest
                        #pragma exclude_renderers gles
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
	        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_local _ HEXA_LIT
            #pragma multi_compile_local _ HEXA_ALPHA

				#if UNITY_VERSION < 202100
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
				#else
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
				#endif
                #pragma multi_compile _ _ADDITIONAL_LIGHTS
				#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
				#if UNITY_VERSION >= 202200
					#pragma multi_compile _ _FORWARD_PLUS
				#endif

            #pragma target 4.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "URPCommon.hlsl"

            TEXTURE2D_ARRAY(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMask);
            SAMPLER(sampler_BumpMask);

            half _GradientIntensity;
            float _ExtrusionMultiplier;
            half4 _Color;
            half3 _AmbientColor;
            float _MinimumLight;
            float3 _Center;
            half _TileAlpha;

            struct appdata {
                float4 vertex   : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 gPos     : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g {
                float4 vertex : POSITION;
                float4 uv     : TEXCOORD0;
                half4 color : COLOR;
                float4 gPos     : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f {
                float4 pos   : SV_POSITION;
                float3 uv    : TEXCOORD0;
                float3 norm     : TEXCOORD1;
                float3 axisNorm : TEXCOORD2;
                half4 color : COLOR;
                #if HEXA_LIT
                    float3 wpos  : TEXCOORD3;
                    float4 scrPos: TEXCOORD4;
                #endif
            };

            struct VertexInfo {
                float4 vertex;
            };


            float GetLightAttenuation(float3 wpos) {
	            float4 shadowCoord = TransformWorldToShadowCoord(wpos);
	            float atten = MainLightRealtimeShadow(shadowCoord);
                return atten;
            }


            v2g vert(appdata v) {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = v.vertex;
                o.uv = v.texcoord;
                half4 color = v.color * _Color;
                color.a *= _TileAlpha;
                o.color = color;
                o.gPos = v.gPos;
                return o;
            }


            #if HEXA_LIT
                #define OUTPUT_WPOS(tri, vertex) tri.wpos = mul(unity_ObjectToWorld, vertex).xyz; tri.scrPos = ComputeScreenPos(tri.pos);
            #else
                #define OUTPUT_WPOS(tri, vertex)
            #endif


            void Extrude(v2g p0, v2g p1, float extrusion, half4 color, float3 norm0, float3 norm1, inout TriangleStream<g2f> outputStream) {
                g2f tri;
                VertexInfo v;

                float3 gPos = p0.gPos.xyz;

                // First side triangle
                v.vertex = p1.vertex;
                v.vertex.xyz *= extrusion; // = p1.pos.xyz * extrusion;
                tri.pos = TransformObjectToHClip(v.vertex.xyz); //float4(v, p1.pos.w));
                tri.uv = float3(0.0, 1.0, p1.uv.z);
                tri.color = color;
                tri.norm = tri.axisNorm = norm1;
                OUTPUT_WPOS(tri, v.vertex);
                outputStream.Append(tri);

                //                	v = p0.pos.xyz * extrusion;
                v.vertex = p0.vertex;
                v.vertex.xyz *= extrusion;
                tri.pos = TransformObjectToHClip(v.vertex.xyz); // float4(v, p0.pos.w));
                tri.uv = float3(1.0, 1.0, p0.uv.z);
                tri.norm = tri.axisNorm = norm0;
                OUTPUT_WPOS(tri, v.vertex);
                outputStream.Append(tri);

                //                	v = p0.pos.xyz;
                v.vertex = p0.vertex;
                tri.pos = TransformObjectToHClip(v.vertex.xyz); // p0.pos);
                tri.uv = float3(1.0, 0.0, p0.uv.z);
                half4 darkerColor = color * _GradientIntensity;
                tri.color = darkerColor;
                tri.norm = tri.axisNorm = TransformObjectToWorldNormal(v.vertex.xyz - gPos);
                OUTPUT_WPOS(tri, v.vertex);
                outputStream.Append(tri);

                // Second side triangle (completes the side quad)
//                	v = p1.pos.xyz;
                v.vertex = p1.vertex;
                outputStream.RestartStrip();
                tri.pos = TransformObjectToHClip(v.vertex.xyz); // p1.pos);
                tri.uv = float3(0.0, 0.0, p1.uv.z);
                tri.norm = tri.axisNorm = TransformObjectToWorldNormal(v.vertex.xyz - gPos);
                OUTPUT_WPOS(tri, v.vertex);
                outputStream.Append(tri);

                //                	v = p1.pos.xyz * extrusion;
                v.vertex = p1.vertex;
                v.vertex.xyz *= extrusion;
                tri.pos = TransformObjectToHClip(v.vertex.xyz); // float4(v, p1.pos.w));
                tri.uv = float3(0.0, 1.0, p1.uv.z);
                tri.color = color;
                tri.norm = tri.axisNorm = norm1;
                OUTPUT_WPOS(tri, v.vertex);
                outputStream.Append(tri);

                //                	v = p0.pos.xyz;
                v.vertex = p0.vertex;
                tri.pos = TransformObjectToHClip(v.vertex.xyz); // p0.pos);
                tri.uv = float3(1.0, 0.0, p0.uv.z);
                tri.color = darkerColor;
                tri.norm = tri.axisNorm = TransformObjectToWorldNormal(v.vertex.xyz - gPos);
                OUTPUT_WPOS(tri, v.vertex);
                outputStream.Append(tri);
                outputStream.RestartStrip();
            }

            [maxvertexcount(21)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> outputStream) {

                float extrusion = 1.0 + input[0].uv.w * _ExtrusionMultiplier;
                float3 gPos = mul(unity_ObjectToWorld, float4(input[0].gPos.xyz * extrusion, 1.0)).xyz;;

#if HEXA_ALPHA
                float3 v1 = gPos - _Center;
                float3 v2 = gPos - _WorldSpaceCameraPos;
                float d = dot(v1, v2);
                if (d > 0) return;
#endif

                g2f topFace;
                half4 color = input[0].color;
                topFace.color = color;


                gPos += normalize(_Center - gPos) * 0.1;

                float3 axisNorm = normalize(gPos - _Center);
                topFace.axisNorm = axisNorm;

                VertexInfo v;
                v.vertex = input[0].vertex;
                v.vertex.xyz *= extrusion;
                topFace.pos = TransformObjectToHClip(v.vertex.xyz);
                topFace.uv = input[0].uv.xyz;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 norm = normalize(worldPos - gPos);
                norm = lerp(norm, axisNorm, input[0].gPos.w);
                float3 norm0 = topFace.norm = norm;
                OUTPUT_WPOS(topFace, v.vertex);

                outputStream.Append(topFace);

                //                    v = input[1].pos;
                //	                v.xyz *= extrusion;
                v.vertex = input[1].vertex;
                v.vertex.xyz *= extrusion;
                topFace.pos = TransformObjectToHClip(v.vertex.xyz);
                topFace.uv = input[1].uv.xyz;
                worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                norm = normalize(worldPos - gPos);
                norm = lerp(norm, axisNorm, input[0].gPos.w);
                float3 norm1 = topFace.norm = norm;
                OUTPUT_WPOS(topFace, v.vertex);
                outputStream.Append(topFace);

                //                    v = input[2].pos;
                //	                v.xyz *= extrusion;
                v.vertex = input[2].vertex;
                v.vertex.xyz *= extrusion;
                topFace.pos = TransformObjectToHClip(v.vertex.xyz);
                topFace.uv = input[2].uv.xyz;
                worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                norm = normalize(worldPos - gPos);
                norm = lerp(norm, axisNorm, input[0].gPos.w);
                topFace.norm = norm;
                OUTPUT_WPOS(topFace, v.vertex);
                outputStream.Append(topFace);

                outputStream.RestartStrip();

                Extrude(input[0], input[1], extrusion, color, norm0, norm1, outputStream);

            }

            half4 frag(g2f i) : SV_Target {

                    half  mask = SAMPLE_TEXTURE2D(_BumpMask, sampler_BumpMask, i.uv.xy).r;
                    float3 norm = normalize(lerp(i.norm, i.axisNorm, mask));
                    half  ndl = max(0, dot(norm, _MainLightPosition.xyz));
                    ndl = max(ndl, _MinimumLight);
                    half4 edgeColor = i.color * _MainLightColor;
                    edgeColor.rgb *= ndl;

                    half4 color = SAMPLE_TEXTURE2D_ARRAY(_MainTex, sampler_MainTex, i.uv.xy, i.uv.z) * edgeColor;
                #if HEXA_LIT
                    half atten = GetLightAttenuation(i.wpos);
                    color.rgb *= atten;
                    color.rgb += GetNativeLightsColor(i.wpos, i.scrPos.xy/i.scrPos.w);
                #endif
                    color.rgb += _AmbientColor;
                    return color;

            }
            ENDHLSL
    }
        }


    SubShader {
    	Tags { "Queue" = "Geometry-2" "RenderType"="Opaque" }

 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	      	Blend [_SrcBlend] [_DstBlend]
	      	ZWrite [_ZWrite]
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma exclude_renderers gles
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#pragma multi_compile_local _ HEXA_ALPHA
				#pragma target 4.0
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 
                sampler2D _BumpMask;
                fixed _GradientIntensity;
                float _ExtrusionMultiplier;
                fixed4 _Color;
                fixed3 _AmbientColor;
                float _MinimumLight;
                fixed _TileAlpha;
				float3 _Center;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 gPos     : TEXCOORD1;
					fixed4 color    : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
    			};

				struct v2g {
	    			float4 vertex   : POSITION;
	    			float4 uv       : TEXCOORD0;
	    			fixed4 color    : COLOR;
                    float4 gPos     : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
				};

				struct g2f {
	    			float4 pos   : SV_POSITION;
	    			float3 uv    : TEXCOORD0;
	    			float3 norm     : TEXCOORD1;
	    			float3 axisNorm : TEXCOORD2;
	    			SHADOW_COORDS(3)
	    			fixed4 color : COLOR;
				};

				struct VertexInfo {
					float4 vertex;
				};

				v2g vert(appdata v) {
    				v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2g, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    				o.vertex = v.vertex;
    				o.uv     = v.texcoord;
					fixed4 color = v.color * _Color;
    				color.a *= _TileAlpha;
    				o.color  = color;
    				o.gPos =  v.gPos;
    				return o;
    			}

    			void Extrude(v2g p0, v2g p1, float extrusion, fixed4 color, float3 norm0, float3 norm1, inout TriangleStream<g2f> outputStream) {
    			    g2f tri;
    			    VertexInfo v;

   	                float3 gPos = p0.gPos.xyz;

    			    // First side triangle
    			    v.vertex = p1.vertex;
    			    v.vertex.xyz *= extrusion; // = p1.pos.xyz * extrusion;
                	tri.pos   = UnityObjectToClipPos(v.vertex); //float4(v, p1.pos.w));
                	tri.uv    = float3(0.0, 1.0, p1.uv.z);
                	tri.color = color;
    	            tri.norm  = tri.axisNorm = norm1;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

//                	v = p0.pos.xyz * extrusion;
					v.vertex = p0.vertex;
					v.vertex.xyz *= extrusion;
                	tri.pos   = UnityObjectToClipPos(v.vertex); // float4(v, p0.pos.w));
                	tri.uv    = float3(1.0, 1.0, p0.uv.z);
    	            tri.norm  = tri.axisNorm = norm0;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

//                	v = p0.pos.xyz;
					v.vertex = p0.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); // p0.pos);
                	tri.uv  = float3(1.0, 0.0, p0.uv.z);
                	fixed4 darkerColor = color * _GradientIntensity;
                	tri.color = darkerColor;
    	            tri.norm  = tri.axisNorm = UnityObjectToWorldNormal(v.vertex.xyz - gPos);
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

                	// Second side triangle (completes the side quad)
//                	v = p1.pos.xyz;
					v.vertex = p1.vertex;
                	outputStream.RestartStrip();
                	tri.pos = UnityObjectToClipPos(v.vertex); // p1.pos);
                	tri.uv  = float3(0.0, 0.0, p1.uv.z);
    	            tri.norm  = tri.axisNorm = UnityObjectToWorldNormal(v.vertex.xyz - gPos);
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

//                	v = p1.pos.xyz * extrusion;
					v.vertex = p1.vertex;
					v.vertex.xyz *= extrusion;
                	tri.pos = UnityObjectToClipPos(v.vertex); // float4(v, p1.pos.w));
                	tri.uv  = float3(0.0, 1.0, p1.uv.z);
                	tri.color = color;
    	            tri.norm  = tri.axisNorm = norm1;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

//                	v = p0.pos.xyz;
					v.vertex = p0.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); // p0.pos);
                	tri.uv  = float3(1.0, 0.0, p0.uv.z);
                	tri.color = darkerColor;
    	            tri.norm  = tri.axisNorm = UnityObjectToWorldNormal(v.vertex.xyz - gPos);
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
                	outputStream.RestartStrip();
				}

				[maxvertexcount(21)]
				void geom(triangle v2g input[3], inout TriangleStream<g2f> outputStream) {

					float extrusion = 1.0 + input[0].uv.w * _ExtrusionMultiplier;
   	                float3 gPos = mul(unity_ObjectToWorld, float4(input[0].gPos.xyz * extrusion, 1.0)).xyz;;

					#if HEXA_ALPHA
    	                float3 v1 = gPos - _Center;
        	            float3 v2 = gPos - _WorldSpaceCameraPos;
            	        float d = dot(v1,v2);
                	    if (d>0) return;
                    #endif

					g2f topFace;
					fixed4 color = input[0].color;
   	                topFace.color = color;

					gPos += normalize(_Center - gPos) * 0.1;

					float3 axisNorm = normalize(gPos - _Center);
    	            topFace.axisNorm = axisNorm;

    	            VertexInfo v;
    	            v.vertex = input[0].vertex;
	                v.vertex.xyz *= extrusion;
	                topFace.pos = UnityObjectToClipPos(v.vertex);
    	            topFace.uv = float4(input[0].uv.xyz, 1.0);
      				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    	            float3 norm = normalize(worldPos - gPos);
    	            norm = lerp(norm, axisNorm, input[0].gPos.w);
    	            float3 norm0 = topFace.norm = norm;
   	                TRANSFER_SHADOW(topFace);
                    outputStream.Append(topFace);

//                    v = input[1].pos;
//	                v.xyz *= extrusion;
					v.vertex = input[1].vertex;
					v.vertex.xyz *= extrusion;
	                topFace.pos = UnityObjectToClipPos(v.vertex);
    	            topFace.uv = float4(input[1].uv.xyz, 1.0);
      				worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    	            norm = normalize(worldPos - gPos);
    	            norm = lerp(norm, axisNorm, input[0].gPos.w);
    	            float3 norm1 = topFace.norm = norm;
    	            TRANSFER_SHADOW(topFace);
                    outputStream.Append(topFace);

//                    v = input[2].pos;
//	                v.xyz *= extrusion;
					v.vertex = input[2].vertex;
					v.vertex.xyz *= extrusion;
	                topFace.pos = UnityObjectToClipPos(v.vertex);
    	            topFace.uv = float4(input[2].uv.xyz, 1.0);
      				worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    	            norm = normalize(worldPos - gPos);
    	            norm = lerp(norm, axisNorm, input[0].gPos.w);
    	            topFace.norm = norm;
    	            TRANSFER_SHADOW(topFace);
                    outputStream.Append(topFace);

                	outputStream.RestartStrip();

                	// Side
                	Extrude(input[0], input[1], extrusion, color, norm0, norm1, outputStream);
				}

    			fixed4 frag (g2f i) : SV_Target {
    				fixed  atten = SHADOW_ATTENUATION(i);
    				fixed  mask = tex2D(_BumpMask, i.uv.xy).r;
    				float3 norm = normalize(lerp(i.norm, i.axisNorm, mask));
    				fixed  ndl = max(0, dot(norm, _WorldSpaceLightPos0.xyz));
                    ndl = max(ndl, _MinimumLight);

    		        fixed4 edgeColor = i.color * _LightColor0;
    		        edgeColor.rgb *= ndl;

    				fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv) * edgeColor;
    				color.rgb *= atten;
    				color.rgb += _AmbientColor;
                    return color;
                }
                ENDCG
		}

 		Pass {
 				Name "ShadowCaster"
				Tags { "LightMode" = "ShadowCaster" }
		        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_shadowcaster
				#pragma target 4.0
				#include "UnityCG.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 

                fixed _GradientIntensity;
                float _ExtrusionMultiplier;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					#if HEXA_ALPHA
					    float3 gPos     : TEXCOORD1;
					#endif
                    UNITY_VERTEX_INPUT_INSTANCE_ID
    			};

				struct v2g {
	    			float4 pos   : SV_POSITION;
	    			float4 uv    : TEXCOORD0;
	    			#if HEXA_ALPHA
                        float3 gPos  : TEXCOORD1;
                    #endif
                    UNITY_VERTEX_OUTPUT_STEREO
                };

				struct g2f {
					V2F_SHADOW_CASTER;
				};

				struct vertexInfo {
					float4 vertex;
				};

				v2g vert(appdata v) {
    				v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2g, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    				o.pos   = v.vertex;
    				o.uv    = v.texcoord;
					#if HEXA_ALPHA
    				o.gPos = mul(unity_ObjectToWorld, float4(v.gPos.xyz, 1.0)).xyz;
    				#endif
    				return o;
    			}

    			void Extrude(v2g p0, v2g p1, float extrusion, inout TriangleStream<g2f> outputStream) {
    				vertexInfo v;
    			    g2f tri;
                	v.vertex = float4(p1.pos.xyz * extrusion, p1.pos.w);
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = float4(p0.pos.xyz * extrusion, p0.pos.w);
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = p0.pos;
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	outputStream.RestartStrip();
                	v.vertex = p1.pos;
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = float4(p1.pos.xyz * extrusion, p1.pos.w);
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = p0.pos;
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	outputStream.RestartStrip();
				}

				[maxvertexcount(21)]
				void geom(triangle v2g input[3], inout TriangleStream<g2f> outputStream) {

					#if HEXA_ALPHA
	    				float3 gPos = input[0].gPos;
    	                float3 v1 = gPos - _Center;
        	            float3 v2 = gPos - _WorldSpaceCameraPos;
            	        float dt = dot(v1, v2);
                	    if (dt>0) return;
                    #endif

					g2f topFace;
					float extrusion = 1.0 + input[0].uv.w * _ExtrusionMultiplier;
	                for(int i = 0; i < 3; i++) {
	                	vertexInfo v;
	                	v.vertex = input[i].pos;
	                	v.vertex.xyz *= extrusion;
    	                TRANSFER_SHADOW_CASTER(topFace);
                    	outputStream.Append(topFace);
                	}
                	outputStream.RestartStrip();
                	Extrude(input[0], input[1], extrusion, outputStream);
				}

    			fixed4 frag (g2f i) : SV_Target {
					SHADOW_CASTER_FRAGMENT(i)
                }
                ENDCG
		}

    }

    SubShader {	// Fallback for SM 3.5
    	Tags { "Queue" = "Geometry-2" "RenderType"="Opaque" }
       	Blend [_SrcBlend] [_DstBlend]
       	ZWrite [_ZWrite]
 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#pragma target 3.5
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 

                float _ExtrusionMultiplier;
                fixed4 _Color;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					fixed4 color    : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
    			};

				struct v2f {
	    			float4 pos   : SV_POSITION;
	    			float4 uv    : TEXCOORD0;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(1)
                    UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v) {
    				v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    				float extrusion = 1.0 + v.texcoord.w * _ExtrusionMultiplier;
    				v.vertex.xyz *= extrusion;
    				o.pos = UnityObjectToClipPos(v.vertex);
    	            o.uv = float4(v.texcoord.xyz, 1.0);
    	            o.color = v.color * _Color;
    	            TRANSFER_SHADOW(o);
    				return o;
    			}

    			fixed4 frag (v2f i) : SV_Target {
                    fixed4 co = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv.xyz) * i.color;
                    fixed atten = SHADOW_ATTENUATION(i);
                    co.rgb *= i.uv.www * atten;
                    return co;
                }
                ENDCG
		}
    }

    SubShader {	// Fallback for old GPUs
    	Tags { "Queue" = "Geometry-2" "RenderType"="Opaque" }
       	Blend [_SrcBlend] [_DstBlend]
       	ZWrite [_ZWrite]
 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

                float _ExtrusionMultiplier;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					fixed4 color    : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
    			};

				struct v2f {
	    			float4 pos   : SV_POSITION;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(0)
                    UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v) {
    				v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float extrusion = 1.0 + v.texcoord.w * _ExtrusionMultiplier;
	                v.vertex.xyz *= extrusion;
	                o.pos = UnityObjectToClipPos(v.vertex);
    				o.color = v.color;
    				TRANSFER_SHADOW(o);
    				return o;
    			}

    			fixed4 frag (v2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
    				return i.color * atten;
                }
                ENDCG
		}
    }

    Fallback "Diffuse"
}