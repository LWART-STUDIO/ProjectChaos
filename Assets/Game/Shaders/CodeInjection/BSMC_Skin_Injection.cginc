//# BLOCK: BSMC Skin Properties
//# Inject @ Properties/Start
/// Свойства из BSMC_Skin, адаптированные для TC2
[NoScaleOffset] _Diffuse ("BSMC Diffuse (Map)", 2D) = "white" {}
[NoScaleOffset] _Normal ("BSMC Normal (Bump)", 2D) = "bump" {}
[TCP2Separator]
/// Цвета из BSMC_Skin
_SkinTone ("BSMC Skin Tone", Color) = (1, 0, 0, 0)
_SkinUnderTone ("BSMC Skin Under Tone", Color) = (0, 0, 0, 0)
_BrowColor ("BSMC Brow Color", Color) = (0, 1, 0, 0)
_LashesColor ("BSMC Lashes Color", Color) = (0, 0, 1, 0)
/// Остальные цвета (Underwear, Opacity) можно добавить по аналогии, если нужно
_Smoothness ("BSMC Smoothness", Float) = 0.5 // Используем float, так как в TC2 обычно так
_FuzzWidth ("BSMC Fuzz Width", Float) = 3.51 // Пока не используется в этом примере


//# BLOCK: BSMC Skin Variables
//# Inject @ Variables/Inside CBuffer
/// Переменные, соответствующие свойствам
half4 _SkinTone;
half4 _SkinUnderTone;
half4 _BrowColor;
half4 _LashesColor;
half _Smoothness;
half _FuzzWidth; // Пока не используется


//# BLOCK: BSMC Skin Textures Declaration
//# Inject @ Main Pass/Fragment Shader/Start
/// Объявляем TEXTURE2D и SAMPLER для _Diffuse и _Normal *внутри* фрагментного шейдера
/// Это необходимо для правильного использования в SAMPLE_TEXTURE2D
TEXTURE2D(_Diffuse);
SAMPLER(sampler_Diffuse);
TEXTURE2D(_Normal);
SAMPLER(sampler_Normal);


//# BLOCK: BSMC Skin Functions
//# Inject @ Functions
/// Копируем необходимые функции из BSMC_Skin. Взял несколько основных.
/// (Предполагается, что они не конфликтуют с уже существующими в TC2)
float4 Unity_Add_float4(float4 A, float4 B)
{
    return A + B;
}

float4 Unity_Multiply_float4_float4(float4 A, float4 B)
{
    return A * B;
}

float4 Unity_Lerp_float4(float4 A, float4 B, float4 T)
{
    return lerp(A, B, T);
}

float Unity_FresnelEffect_float(float3 WorldNormal, float3 WorldViewDir, float3 FresnelColor, float Power, float Scale)
{
    float3 N = WorldNormal;
    float3 V = WorldViewDir;
    float fresnel = dot(N, V);
    fresnel = pow(1.0 - saturate(fresnel), Power) * Scale;
    return fresnel;
}
// Добавьте другие необходимые Unity_* функции из BSMC_Skin по мере необходимости


//# BLOCK: BSMC Skin Fragment Logic
//# Inject @ Main Pass/Fragment Shader/End
/// Основная логика интеграции в фрагментный шейдер TC2 URP
/// Правильные имена переменных, основанные на сгенерированном шейдере.

// Получаем UV из входных данных фрагментного шейдера
float2 uv = input.pack0.xy; // Используем pack0.xy, который соответствует uv0

// Читаем диффузную текстуру BSMC через URP стиль, используя объявленные TEXTURE2D и SAMPLER *внутри* Fragment
// Теперь _Diffuse и sampler_Diffuse объявлены в начале Fragment через инжекцию Main Pass/Fragment Shader/Start
float4 bsmcDiffuseSample = TCP2_TEX2D_SAMPLE(_Diffuse,_Diffuse,uv); // Используем _Diffuse и соответствующий сэмплер

// Применяем тон кожи (_SkinTone) - используем альфа _SkinTone для смешивания
half3 finalSkinColor = lerp(bsmcDiffuseSample.rgb, _SkinTone.rgb, _SkinTone.a);

// Применяем подтон кожи (_SkinUnderTone)
half3 finalSkinColorWithUnderTone = lerp(finalSkinColor, _SkinUnderTone.rgb, _SkinUnderTone.a);

// Заменяем или модифицируем базовый цвет 'albedo', рассчитанный TC2 до этого блока
// 'albedo' - это переменная, используемая в расчетах освещения в шейдере TC2 URP
albedo = finalSkinColorWithUnderTone; // Заменяем Albedo на рассчитанный из BSMC

// --- НОРМАЛЬ ---
// ВНИМАНИЕ: Этот шейдер URP не использует нормальную карту в основном Pass (только в DepthNormals).
// Для использования _Normal, вам нужно будет:
// 1. Добавить вычисление TBN матрицы в вершинный шейдер (Vertex).
// 2. Передать TBN (или касательное пространство нормали) в фрагментный шейдер.
// 3. Использовать это в Fragment.
// Это требует более глубокой модификации, чем позволяет инжекция.
// Пока оставим комментарием.
/*
// Читаем нормальную карту BSMC
float4 bsmcNormalSample = SAMPLE_TEXTURE2D(_Normal, sampler_Normal, uv); // Используем _Normal и соответствующий сэмплер
half3 normalTS_BSMC = UnpackNormal(bsmcNormalSample);
// float3 worldNormal_BSMC = ...; // Преобразование из касательного в мировое - требует TBN
// normalWS = worldNormal_BSMC; // Заменить нормаль в мировом пространстве (если возможно инжекцией)
*/

// --- ГЛАДКОСТЬ / МЕТАЛЛИЧНОСТЬ ---
// Этот шейдер URP TC2 не управляет гладкостью напрямую в фрагментном шейдере для основного освещения.
// Он использует Ramp Shading, а не PBR.
// Если вы хотите использовать _Smoothness для чего-то, нужно смотреть, как TC2 URP обрабатывает гладкость (возможно, через альфа-канал текстуры или другой параметр).
// Пока оставим комментарием.
// half smoothnessValue = _Smoothness; // Не заменяем напрямую, нет переменной 'smoothness'

// --- ЦВЕТА ДЕТАЛЕЙ (брови, ресницы) ---
// Как и раньше, требуют маски. Интеграция без масок невозможна.
// Для простоты, не реализовано в этом блоке.
