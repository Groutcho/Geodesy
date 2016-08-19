Shader "Custom/TransparentSolid" {
    Properties {
        _Color ("Main Color (A=Opacity)", Color) = (1,1,1,1)
    }

    Category {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        SubShader { // Unity chooses the subshader that fits the GPU best
            Pass { // some shaders require multiple passes
                CGPROGRAM // here begins the part in Unity's Cg

                // this specifies the vert function as the vertex shader
                #pragma vertex vert

                // this specifies the frag function as the fragment shader
                #pragma fragment frag

                // vertex shader
                float4 vert(float4 vertexPos : POSITION) : SV_POSITION
                {
                    // this line transforms the vertex input parameter
                    // vertexPos with the built-in matrix UNITY_MATRIX_MVP
                    // and returns it as a nameless vertex output parameter
                    return mul(UNITY_MATRIX_MVP, vertexPos);
                }

                fixed4 _Color;

                float4 frag(void) : COLOR // fragment shader
                {
                    // this fragment shader returns a nameless fragment
                    // output parameter (with semantic COLOR) that is set to
                    // opaque red (red = 1, green = 0, blue = 0, alpha = 1)
                    return _Color;
                }

                ENDCG // here ends the part in Cg
            }
        }
    }
}