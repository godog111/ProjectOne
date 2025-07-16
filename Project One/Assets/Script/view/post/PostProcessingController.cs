using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

[System.Serializable]
public class PostProcessingPreset
{
    [Range(0, 10)] public float bloomIntensity = 1f;
    [Range(0, 5)] public float bloomThreshold = 1f;
    [Range(0, 1)] public float bloomSoftKnee = 0.5f;
    [Range(-100, 100)] public float saturation = 0f;
    [Range(-100, 100)] public float temperature = 0f;
    [Range(-100, 100)] public float contrast = 0f;
    [Range(0, 1)] public float vignetteIntensity = 0.3f;
    [Range(0, 1)] public float vignetteSmoothness = 0.5f;
    [Range(0, 1)] public float vignetteRoundness = 1f;
    [Range(0, 1)] public float chromaticAberration = 0f;
}

public enum PresetType
{
    Normal,
    Horror,
    Dream
}
public class PostProcessingController : MonoBehaviour
{
    public static PostProcessingController Instance { get; private set; }

    [Header("组件引用")]
    public PostProcessVolume postProcessVolume;

    [Header("当前设置")]
    public PostProcessingPreset currentSettings;

    [Header("预设值")]
    public PostProcessingPreset normalPreset;
    public PostProcessingPreset horrorPreset;
    public PostProcessingPreset dreamPreset;

    private Bloom _bloom;
    private ColorGrading _colorGrading;
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private Bloom bloom;
    public PostProcessVolume volume;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); // 如果需要跨场景保持

        InitializeComponents();
        ApplyPreset(normalPreset);
    }

    private void InitializeComponents()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = GetComponent<PostProcessVolume>();
            if (postProcessVolume == null)
            {
                Debug.LogError("未找到PostProcessVolume组件!");
                return;
            }
        }
        
        if (postProcessVolume.profile == null)
        {
            Debug.LogError("PostProcessVolume没有分配Profile！");
            return;
        }

        postProcessVolume.profile.TryGetSettings(out _bloom);
        postProcessVolume.profile.TryGetSettings(out _colorGrading);
        postProcessVolume.profile.TryGetSettings(out _vignette);
        postProcessVolume.profile.TryGetSettings(out _chromaticAberration);

        if (_bloom == null) _bloom = postProcessVolume.profile.AddSettings<Bloom>();
        if (_colorGrading == null) _colorGrading = postProcessVolume.profile.AddSettings<ColorGrading>();
        if (_vignette == null) _vignette = postProcessVolume.profile.AddSettings<Vignette>();
        if (_chromaticAberration == null) _chromaticAberration = postProcessVolume.profile.AddSettings<ChromaticAberration>();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying || postProcessVolume == null) return;
        ApplyPreset(currentSettings);
    }

#region 静态访问方法

// 新增方法：通过枚举类型应用预设
public static void ApplyPresetByName_Static(PresetType presetType)
{
    if (Instance == null) return;

    switch (presetType)
    {
        case PresetType.Normal:
            Instance.ApplyPreset(Instance.normalPreset);
            break;
        case PresetType.Horror:
            Instance.ApplyPreset(Instance.horrorPreset);
            break;
        case PresetType.Dream:
            Instance.ApplyPreset(Instance.dreamPreset);
            break;
    }
}

// 新增方法：通过字符串名称应用预设
public static void ApplyPresetByName_Static(string presetName)
{
    if (Instance == null) return;

    switch (presetName.ToLower())
    {
        case "normal":
            Instance.ApplyPreset(Instance.normalPreset);
            break;
        case "horror":
            Instance.ApplyPreset(Instance.horrorPreset);
            break;
        case "dream":
            Instance.ApplyPreset(Instance.dreamPreset);
            break;
        default:
            Debug.LogWarning($"未知的预设名称: {presetName}");
            break;
    }
}

    public static void ApplyPreset_Static(PostProcessingPreset preset)
    {
        if (Instance != null) Instance.ApplyPreset(preset);
    }

    public static void LerpToPreset_Static(PostProcessingPreset targetPreset, float duration)
    {
        if (Instance != null) Instance.LerpToPreset(targetPreset, duration);
    }

    public static void PlayDamageEffect_Static(float intensity = 50f, float duration = 1f)
    {
        if (Instance != null) Instance.PlayDamageEffect(intensity, duration);
    }

    public static void SetBloom_Static(float intensity, float threshold = 1f, float softKnee = 0.5f)
    {
        if (Instance != null)
        {
            Instance.currentSettings.bloomIntensity = intensity;
            Instance.currentSettings.bloomThreshold = threshold;
            Instance.currentSettings.bloomSoftKnee = softKnee;
            Instance.ApplyPreset(Instance.currentSettings);
        }
    }

    // 其他效果的静态方法...
    #endregion

    #region 实例方法
    public void ApplyPreset(PostProcessingPreset preset)
    {
        if (preset == null) return;

        currentSettings = preset;

        if (_bloom != null)
        {
            _bloom.intensity.value = preset.bloomIntensity;
            _bloom.threshold.value = preset.bloomThreshold;
            _bloom.softKnee.value = preset.bloomSoftKnee;
        }

        if (_colorGrading != null)
        {
            _colorGrading.saturation.value = preset.saturation;
            _colorGrading.temperature.value = preset.temperature;
            _colorGrading.contrast.value = preset.contrast;
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = preset.vignetteIntensity;
            _vignette.smoothness.value = preset.vignetteSmoothness;
            _vignette.roundness.value = preset.vignetteRoundness;
        }

        if (_chromaticAberration != null)
        {
            _chromaticAberration.intensity.value = preset.chromaticAberration;
        }
    }

    public void LerpToPreset(PostProcessingPreset targetPreset, float duration)
    {
        StartCoroutine(LerpPresetCoroutine(currentSettings, targetPreset, duration));
    }

    private IEnumerator LerpPresetCoroutine(PostProcessingPreset fromPreset, PostProcessingPreset toPreset, float duration)
    {
        if (toPreset == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            PostProcessingPreset lerpedPreset = new PostProcessingPreset
            {
                bloomIntensity = Mathf.Lerp(fromPreset.bloomIntensity, toPreset.bloomIntensity, t),
                bloomThreshold = Mathf.Lerp(fromPreset.bloomThreshold, toPreset.bloomThreshold, t),
                bloomSoftKnee = Mathf.Lerp(fromPreset.bloomSoftKnee, toPreset.bloomSoftKnee, t),
                saturation = Mathf.Lerp(fromPreset.saturation, toPreset.saturation, t),
                temperature = Mathf.Lerp(fromPreset.temperature, toPreset.temperature, t),
                contrast = Mathf.Lerp(fromPreset.contrast, toPreset.contrast, t),
                vignetteIntensity = Mathf.Lerp(fromPreset.vignetteIntensity, toPreset.vignetteIntensity, t),
                vignetteSmoothness = Mathf.Lerp(fromPreset.vignetteSmoothness, toPreset.vignetteSmoothness, t),
                vignetteRoundness = Mathf.Lerp(fromPreset.vignetteRoundness, toPreset.vignetteRoundness, t),
                chromaticAberration = Mathf.Lerp(fromPreset.chromaticAberration, toPreset.chromaticAberration, t)
            };

            ApplyPreset(lerpedPreset);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyPreset(toPreset);
    }

    public void PlayDamageEffect(float intensity = 50f, float duration = 1f)
    {
        StartCoroutine(DamageEffectCoroutine(intensity, duration));
    }

    private IEnumerator DamageEffectCoroutine(float intensity, float duration)
    {
        if (_colorGrading == null) yield break;

        PostProcessingPreset original = new PostProcessingPreset
        {
            temperature = currentSettings.temperature,
            saturation = currentSettings.saturation,
            contrast = currentSettings.contrast
        };

        PostProcessingPreset redPreset = new PostProcessingPreset
        {
            temperature = intensity,
            saturation = intensity / 2f,
            contrast = original.contrast
        };
        ApplyPreset(redPreset);

        yield return new WaitForSeconds(duration * 0.3f);

        float elapsed = 0f;
        while (elapsed < duration * 0.7f)
        {
            float t = elapsed / (duration * 0.7f);

            PostProcessingPreset lerpedPreset = new PostProcessingPreset
            {
                temperature = Mathf.Lerp(redPreset.temperature, original.temperature, t),
                saturation = Mathf.Lerp(redPreset.saturation, original.saturation, t),
                contrast = original.contrast
            };

            ApplyPreset(lerpedPreset);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyPreset(original);
    }
    #endregion
}