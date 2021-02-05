// This shader fills the mesh shape with a color predefined in the code.
Shader "ARTEMIS/DHA"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _dMax("d Max", Float) = 10.0
        _hMax("h Max", Float) = 10.0
        _hMin("h Min", Float) = 2
        _FOV("FOV", Float) = 60

    }

        // The SubShader block containing the Shader code. 
        SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
        Cull Off
        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"   
            #include "UnityCG.cginc"

            float _dMax;
            float _hMax;
            float _hMin;
            float _FOV;


            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
            };

            struct Varyings {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS : SV_POSITION;
                float4 relativePos : POSITION1;
            };

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN) {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The UnityObjectToClipPos function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = UnityObjectToClipPos(IN.positionOS.xyz);
                //OUT.relativePos.xyz = IN.positionOS.xyz - _WorldSpaceCameraPos;
                //float4 worldPos = mul(unity_ObjectToWorld)
                OUT.relativePos.xyz = UnityObjectToViewPos(IN.positionOS.xyz);
                //OUT.relativePos = mul(UNITY_MATRIX_MV, float4(IN.positionOS.xyz,1));

                // Returning the output.
                return OUT;
            }

            // The fragment shader definition.            
            half4 frag(Varyings i) : SV_Target
            {
                // Defining the color variable and returning it.
                float d = -i.relativePos.z / _dMax;
                float h = (i.relativePos.y + _hMin) / _hMax;
                float pi = 3.1415926;
                float a = (atan2(-i.relativePos.x, -i.relativePos.z) + pi * _FOV / 2 / 180)/(pi * _FOV /180);
                //return a;
                return float4(d, h, a, 1);
            }
            ENDHLSL
        }

    }
}