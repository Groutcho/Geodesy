Shader "Geodesy/Terrain" {
    Properties {
    }

SubShader {
    Pass {
        CGPROGRAM // here begins the part in Unity's Cg

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        struct vIn
        {
            float4 position : POSITION;
            float3 normal : NORMAL;
            float4 color : COLOR;
        };

        struct vOut
        {
            float4 position : POSITION;
            float3 normal : NORMAL;
            float4 color : COLOR;
        };

        vOut vert(vIn input)
        {
            vOut output;
            output.position = mul(UNITY_MATRIX_MVP, input.position);
            output.normal = input.normal;
            output.color = input.color;
            return output;
        }

        struct fragment
        {
            float4 color : COLOR;
        };

        fragment frag(vOut input)
        {
            fragment output;
            output.color = input.color * input.normal[1];
            return output;
        }

        ENDCG
    }
}
}
