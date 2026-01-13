#ifndef DYNAMIC_RADIAL_MASKS_HEIGHTFIELD_4_ADVANCED_NORMALIZED_ID1_GLOBAL
#define DYNAMIC_RADIAL_MASKS_HEIGHTFIELD_4_ADVANCED_NORMALIZED_ID1_GLOBAL


uniform float4 DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Position[4];	
uniform float  DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Radius[4];
uniform float  DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Intensity[4];
uniform float  DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_NoiseStrength[4];
uniform float  DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_EdgeSize[4];
uniform float  DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Smooth[4];


#include "../../Core/Core.cginc"



////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                                Main Method                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
float DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global(float3 positionWS, float noise)
{
    float retValue = 1; 

	int i = 0;
[unroll]	for(i = 0; i < 4; i++)
	{
		float mask = ShaderExtensions_DynamicRadialMasks_HeightField_Advanced(positionWS,
																		noise,
																		DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Position[i].xyz, 
																		DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Radius[i],
																		DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Intensity[i],
																		DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_NoiseStrength[i],	
																		DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_EdgeSize[i],	
																		DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_Smooth[i]);

		retValue *= 1 - saturate(mask);	
	}		

    return 1 - retValue;
}

////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                               Helper Methods                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
void DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_float(float3 positionWS, float noise, out float retValue)
{
    retValue = DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global(positionWS, noise); 		
}

void DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global_half(half3 positionWS, half noise, out half retValue)
{
    retValue = DynamicRadialMasks_HeightField_4_Advanced_Normalized_ID1_Global(positionWS, noise); 		
}

#endif
