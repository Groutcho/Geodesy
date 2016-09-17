 Shader "Geodesy/Terrain" {
    Properties {
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf SimpleSpecular

      half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
              half3 h = normalize (lightDir + viewDir);

              half diff = max (0, dot (s.Normal, lightDir));

              float nh = max (0, dot (s.Normal, h));
              float spec = pow (nh, 100.0);

              half4 c;
              c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
              c.a = s.Alpha;
              return c;
          }

      struct Input {
          float4 color : COLOR;
      };

      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = IN.color.rgb;
      }
      ENDCG
    }
    Fallback "Diffuse"
}