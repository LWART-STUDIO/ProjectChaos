#ifndef DYNAMIC_RADIAL_MASKS_DOT_8_ADVANCED_ADDITIVE_ID3_LOCAL
#define DYNAMIC_RADIAL_MASKS_DOT_8_ADVANCED_ADDITIVE_ID3_LOCAL


float4 DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_Position[8];	
float  DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_Radius[8];
float  DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_Intensity[8];
float  DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_NoiseStrength[8];


#include "../../Core/Core.cginc"



////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                                Main Method                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
float DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local(float3 positionWS, float noise)
{
    float retValue = 0; 

	int i = 0;
[unroll]	for(i = 0; i < 8; i++)
	{
		retValue += ShaderExtensions_DynamicRadialMasks_Dot_Advanced(positionWS,
																	noise,
																	DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_Position[i].xyz,
																	DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_Radius[i],
																	DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_Intensity[i],
																	DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_NoiseStrength[i]);
	}		

    return retValue;
}

////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                               Helper Methods                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
void DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_float(float3 positionWS, float noise, out float retValue)
{
    retValue = DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local(positionWS, noise); 		
}

void DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local_half(half3 positionWS, half noise, out half retValue)
{
    retValue = DynamicRadialMasks_Dot_8_Advanced_Additive_ID3_Local(positionWS, noise); 		
}

#endif
