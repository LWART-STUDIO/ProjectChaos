//# BLOCK: DMC Dissolve Properties
//# Inject @ Properties/Start
[TCP2Header(Dissolve)]
[NoScaleOffset] _MaskNoiseMap ("Mask Noise Map", 2D) = "white" {}
[HDR] _MaskEdgeColor ("Mask Edge Color", Color) = (1,1,1,1)
_AlphaClipThreshold ("Alpha Clip Threshold", Range(0.0, 1.0)) = 0.5
_EdgeThreshold ("Edge Threshold", Range(0.0, 1.0)) = 0.5
[ToggleUI] _InvertDissolveEffect ("Invert Dissolve Effect", Float) = 0.0
[TCP2Separator]

//# BLOCK: DMC Dissolve Include
//# Inject @ Include Files
#include "Assets/Plugins/VFX/Amazing Assets/Dynamic Radial Masks/Shaders/CGINC/HeightField/DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global.cginc"
TCP2_TEX2D_WITH_SAMPLER(_MaskNoiseMap); // Объявляем текстуру вне CBUFFER

//# BLOCK: DMC Dissolve Variables
//# Inject @ Variables/Outside CBuffer
// Переменные из свойств объявляем здесь, т.к. TCP2 может не помещать все в CBUFFER автоматически в этом случае
fixed4 _MaskEdgeColor;
float _AlphaClipThreshold;
float _InvertDissolveEffect;
float _EdgeThreshold;

//# BLOCK: DMC Dissolve Main Pass Tags
//# Inject @ Main Pass/Tags
"RenderType" = "TransparentCutout"

//# BLOCK: DMC Dissolve Main Pass Pragma
//# Inject @ Main Pass/Pragma
#pragma shader_feature_local _ALPHATEST_ON
// Не используем ToggleOff, просто Float, поэтому не добавляем _INVERTDISSOLVE_ON

//# BLOCK: DMC Dissolve Main Pass Varyings
//# Inject @ Main Pass/Varyings
// Не нужно добавлять worldPos, т.к. используем input.worldPosAndFog.xyz

//# BLOCK: DMC Dissolve Fragment Shader Start
//# Inject @ Main Pass/Fragment Shader/Start
// Сэмплируем шумовую карту
half4 noiseSample = TCP2_TEX2D_SAMPLE(_MaskNoiseMap, _MaskNoiseMap, input.pack0.xy);
half noiseValue = noiseSample.r;

// Вычисляем маску диссольва DRM
float dissolveMask = DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global(input.worldPosAndFog.xyz, noiseValue);

// Применяем инверсию, если нужно (используем Float, а не keyword)
if (_InvertDissolveEffect > 0.5) {
    dissolveMask = 1.0 - dissolveMask;
}

//# BLOCK: DMC Dissolve Fragment Shader End
//# Inject @ Main Pass/Fragment Shader/End
// Проверяем маску диссольва против порога (как в рабочем шейдере)
clip(dissolveMask - _AlphaClipThreshold);

// Добавляем цвет края диссольва к эмиссии (как в рабочем шейдере)
// Используем _RampSmoothing как ширину края
half3 dissolveEdgeMask = saturate(1.0 - abs(dissolveMask - _AlphaClipThreshold) / max(0.001, _EdgeThreshold)); // __rampSmoothing из TCP2
//color-=emission;
emission += dissolveEdgeMask * _MaskEdgeColor.rgb;
color+=emission;

//# BLOCK: DMC Dissolve Shadow Caster Pass Pragma
//# Inject @ Shadow Caster Pass/Pragma


//# BLOCK: DMC Dissolve Shadow Caster Pass Varyings
//# Inject @ Depth + Shadow Caster Pass/Varyings
float3 worldPos : TEXCOORD2; // Передаём worldPos для DRM в ShadowCaster

//# BLOCK: DMC Dissolve Shadow Caster Pass Vertex Shader Start
//# Inject @ Depth + Shadow Caster Pass/Vertex Shader/Start
output.worldPos = TransformObjectToWorld(input.vertex.xyz);

//# BLOCK: DMC Dissolve Shadow Caster Pass Fragment Shader Start
//# Inject @ Depth + Shadow Caster Pass/Fragment Shader/Start
// Сэмплируем шумовую карту
half4 noiseSample = TCP2_TEX2D_SAMPLE(_MaskNoiseMap, _MaskNoiseMap, input.pack0.xy);
half noiseValue = noiseSample.r;

// Вычисляем маску диссольва DRM
float dissolveMask = DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global(input.worldPos, noiseValue);

// Применяем инверсию, если нужно (берём из материала)
if (_InvertDissolveEffect > 0.5) {
    dissolveMask = 1.0 - dissolveMask;
}

// Проверяем маску диссольва против порога (как в рабочем шейдере)
clip(dissolveMask - _AlphaClipThreshold);