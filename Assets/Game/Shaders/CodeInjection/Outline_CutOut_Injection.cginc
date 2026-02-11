//# BLOCK: Outline AlphaClip Property
//# Inject @ Properties/End
[ToggleUI] _OutlineEnabled ("Outline Enabled", Float) = 1
[ToggleUI] _OutlineAlphaClip ("Outline Alpha Clip", Float) = 1


//# BLOCK: Outline AlphaClip Varyings
//# Inject @ Outline Pass/Varyings
float2 uv : TEXCOORD3;

//# BLOCK: Outline AlphaClip Toggle Variable
//# Inject @ Variables/Outside CBuffer
float _OutlineAlphaClip;
float _OutlineEnabled;

//# BLOCK: Outline AlphaClip Appdata
//# Inject @ Outline Pass/Attributes
float2 texcoord0 : TEXCOORD0;

//# BLOCK: Outline AlphaClip Vertex
//# Inject @ Outline Pass/Vertex Shader/Start
output.uv = v.texcoord0.xy;

//# BLOCK: Outline AlphaClip Fragment
//# Inject @ Outline Pass/Fragment Shader/Start
if (_OutlineEnabled < 0.5)
{
    clip(-1);
}
if (_OutlineAlphaClip > 0.5)
{
    half alpha =
        TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.uv).a
        * _BaseColor.a;

    clip(alpha - _Cutoff);
}
