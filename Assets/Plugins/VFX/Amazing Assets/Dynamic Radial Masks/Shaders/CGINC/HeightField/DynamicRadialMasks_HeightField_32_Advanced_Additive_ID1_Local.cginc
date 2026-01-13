#ifndef DYNAMIC_RADIAL_MASKS_HEIGHTFIELD_32_ADVANCED_ADDITIVE_ID1_LOCAL
#define DYNAMIC_RADIAL_MASKS_HEIGHTFIELD_32_ADVANCED_ADDITIVE_ID1_LOCAL


float4 DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Position[32];	
float  DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Radius[32];
float  DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Intensity[32];
float  DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_NoiseStrength[32];
float  DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_EdgeSize[32];
float  DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Smooth[32];


#include "../../Core/Core.cginc"



////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                                Main Method                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
float DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local(float3 positionWS, float noise)
{
    float retValue = 0; 

	int i = 0;
[unroll]	for(i = 0; i < 32; i++)
	{
		retValue += ShaderExtensions_DynamicRadialMasks_HeightField_Advanced(positionWS,
																	noise,
																	DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Position[i].xyz,
																	DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Radius[i],
																	DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Intensity[i],
																	DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_NoiseStrength[i],
																	DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_EdgeSize[i],
																	DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_Smooth[i]);
	}		

    return retValue;
}

////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                               Helper Methods                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
void DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_float(float3 positionWS, float noise, out float retValue)
{
    retValue = DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local(positionWS, noise); 		
}

void DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local_half(half3 positionWS, half noise, out half retValue)
{
    retValue = DynamicRadialMasks_HeightField_32_Advanced_Additive_ID1_Local(positionWS, noise); 		
}

#endif
