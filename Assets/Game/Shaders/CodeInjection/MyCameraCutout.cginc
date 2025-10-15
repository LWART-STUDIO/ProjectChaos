/// === CAMERA CUTOUT EFFECT FOR TOONY COLORS ===
/// Этот файл добавит свойства камеры и вырежет фрагменты вокруг позиции камеры игрока.

//# BLOCK: Define Custom Properties
//# Inject @ Properties/Start
// эти поля нужны, чтобы ToonyColors видел их в инспекторе
_CameraPos("Camera Position", Vector) = (0,0,0,0)
_CutoutRadius("Camera Cutout Radius", Float) = 1.0
[TCP2Separator]

//# BLOCK: Declare Shader Variables
//# Inject @ Variables/Outside CBuffer
// ⚡️ ВАЖНО: именно здесь, а не Inside CBuffer
float4 _CameraPos;
float _CutoutRadius;


//# BLOCK: Fragment Cutout
//# Inject @ Main Pass/Fragment Start
{
    // positionWS уже есть в фрагменте как local variable (positionWS)
    // используем _CameraPos и _CutoutRadius, которые у тебя уже есть в CBUFFER
    float3 positionWS = input.worldPosAndFog.xyz;
    float3 camPos = _CameraPos.xyz;
    float dist = distance(positionWS, camPos);
    if (dist < _CutoutRadius)
    {
        float edge = smoothstep(_CutoutRadius - 0.2, _CutoutRadius, dist);
        clip(edge - 0.5);
    }
}

