#ifndef DYNAMIC_RADIAL_MASKS_SHOCKWAVE_24_ADVANCED_ADDITIVE_ID1_LOCAL
#define DYNAMIC_RADIAL_MASKS_SHOCKWAVE_24_ADVANCED_ADDITIVE_ID1_LOCAL


float4 DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Position[24];	
float  DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Radius[24];
float  DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Intensity[24];
float  DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_NoiseStrength[24];
float  DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_EdgeSize[24];
float  DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Smooth[24];


#include "../../Core/Core.cginc"



////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                                Main Method                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
float DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local(float3 positionWS, float noise)
{
    float retValue = 0; 

	int i = 0;
[unroll]	for(i = 0; i < 24; i++)
	{
		retValue += ShaderExtensions_DynamicRadialMasks_Shockwave_Advanced(positionWS,
																	noise,
																	DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Position[i].xyz, 
																	DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Radius[i],         
																	DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Intensity[i],
																	DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_NoiseStrength[i],  
																	DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_EdgeSize[i],		
																	DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_Smooth[i]);
	}		

    return retValue;
}

////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                               Helper Methods                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
void DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_float(float3 positionWS, float noise, out float retValue)
{
    retValue = DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local(positionWS, noise); 		
}

void DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local_half(half3 positionWS, half noise, out half retValue)
{
    retValue = DynamicRadialMasks_Shockwave_24_Advanced_Additive_ID1_Local(positionWS, noise); 		
}

#endif
