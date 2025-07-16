using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Glitch Effect")]
public class GlitchEffect : MonoBehaviour
{
    #region 数字故障(Digital Glitch)属性
    
    [SerializeField, Range(0, 1), Tooltip("数字故障强度")]
    float _digitalIntensity = 0;
    
    [SerializeField, Tooltip("数字故障着色器")]
    Shader _digitalShader;
    
    [SerializeField, Tooltip("启用数字故障")]
    bool _enableDigitalGlitch = true;
    
    #endregion
    
    #region 模拟故障(Analog Glitch)属性
    
    [SerializeField, Range(0, 1), Tooltip("扫描线抖动强度")]
    float _scanLineJitter = 0;
    
    [SerializeField, Range(0, 1), Tooltip("垂直跳动强度")]
    float _verticalJump = 0;
    
    [SerializeField, Range(0, 1), Tooltip("水平抖动强度")]
    float _horizontalShake = 0;
    
    [SerializeField, Range(0, 1), Tooltip("颜色漂移强度")]
    float _colorDrift = 0;
    
    [SerializeField, Tooltip("模拟故障着色器")]
    Shader _analogShader;
    
    [SerializeField, Tooltip("启用模拟故障")]
    bool _enableAnalogGlitch = false;
    
    #endregion
    
    #region 私有属性
    
    Material _digitalMaterial;
    Material _analogMaterial;
    Texture2D _noiseTexture;
    RenderTexture _trashFrame1;
    RenderTexture _trashFrame2;
    float _verticalJumpTime;
    
    #endregion
    
    #region 公共控制方法
    
    /// <summary>
    /// 启用/禁用数字故障效果
    /// </summary>
    public void SetDigitalGlitchEnabled(bool enabled)
    {
        _enableDigitalGlitch = enabled;
        if (!enabled && _digitalMaterial != null)
        {
            DestroyImmediate(_digitalMaterial);
            _digitalMaterial = null;
        }
    }
    
    /// <summary>
    /// 设置数字故障强度
    /// </summary>
    public void SetDigitalIntensity(float intensity)
    {
        _digitalIntensity = Mathf.Clamp01(intensity);
    }
    
    /// <summary>
    /// 启用/禁用模拟故障效果
    /// </summary>
    public void SetAnalogGlitchEnabled(bool enabled)
    {
        _enableAnalogGlitch = enabled;
        if (!enabled && _analogMaterial != null)
        {
            DestroyImmediate(_analogMaterial);
            _analogMaterial = null;
        }
    }
    
    /// <summary>
    /// 设置扫描线抖动强度
    /// </summary>
    public void SetScanLineJitter(float amount)
    {
        _scanLineJitter = Mathf.Clamp01(amount);
    }
    
    /// <summary>
    /// 设置垂直跳动强度
    /// </summary>
    public void SetVerticalJump(float amount)
    {
        _verticalJump = Mathf.Clamp01(amount);
    }
    
    /// <summary>
    /// 设置水平抖动强度
    /// </summary>
    public void SetHorizontalShake(float amount)
    {
        _horizontalShake = Mathf.Clamp01(amount);
    }
    
    /// <summary>
    /// 设置颜色漂移强度
    /// </summary>
    public void SetColorDrift(float amount)
    {
        _colorDrift = Mathf.Clamp01(amount);
    }
    
    /// <summary>
    /// 重置所有故障效果
    /// </summary>
    public void ResetAllEffects()
    {
        _digitalIntensity = 0;
        _scanLineJitter = 0;
        _verticalJump = 0;
        _horizontalShake = 0;
        _colorDrift = 0;
        
        if (_digitalMaterial != null)
        {
            DestroyImmediate(_digitalMaterial);
            _digitalMaterial = null;
        }
        
        if (_analogMaterial != null)
        {
            DestroyImmediate(_analogMaterial);
            _analogMaterial = null;
        }
    }
    
    /// <summary>
    /// 快速启用所有效果并设置中等强度
    /// </summary>
    public void EnableAllEffects(float intensity = 0.5f)
    {
        _enableDigitalGlitch = true;
        _enableAnalogGlitch = true;
        
        _digitalIntensity = Mathf.Clamp01(intensity);
        _scanLineJitter = Mathf.Clamp01(intensity);
        _verticalJump = Mathf.Clamp01(intensity);
        _horizontalShake = Mathf.Clamp01(intensity);
        _colorDrift = Mathf.Clamp01(intensity);
    }
    
    #endregion
    
    #region 私有方法
    
    static Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, Random.value);
    }
    
    void SetUpDigitalResources()
    {
        if (_digitalMaterial != null || _digitalShader == null || !_enableDigitalGlitch) return;
        
        _digitalMaterial = new Material(_digitalShader);
        _digitalMaterial.hideFlags = HideFlags.DontSave;
        
        _noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
        _noiseTexture.hideFlags = HideFlags.DontSave;
        _noiseTexture.wrapMode = TextureWrapMode.Clamp;
        _noiseTexture.filterMode = FilterMode.Point;
        
        _trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
        _trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
        _trashFrame1.hideFlags = HideFlags.DontSave;
        _trashFrame2.hideFlags = HideFlags.DontSave;
        
        UpdateNoiseTexture();
    }
    
    void SetUpAnalogResources()
    {
        if (_analogMaterial != null || _analogShader == null || !_enableAnalogGlitch) return;
        
        _analogMaterial = new Material(_analogShader);
        _analogMaterial.hideFlags = HideFlags.DontSave;
    }
    
    void UpdateNoiseTexture()
    {
        if (_noiseTexture == null) return;
        
        var color = RandomColor();
        
        for (var y = 0; y < _noiseTexture.height; y++)
        {
            for (var x = 0; x < _noiseTexture.width; x++)
            {
                if (Random.value > 0.89f) color = RandomColor();
                _noiseTexture.SetPixel(x, y, color);
            }
        }
        
        _noiseTexture.Apply();
    }
    
    #endregion
    
    #region MonoBehaviour方法
    
    void Update()
    {
        if (_enableDigitalGlitch && _digitalIntensity > 0 && Random.value > Mathf.Lerp(0.9f, 0.5f, _digitalIntensity))
        {
            SetUpDigitalResources();
            UpdateNoiseTexture();
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture tempTexture = null;
        
        // 数字故障处理
        if (_enableDigitalGlitch && _digitalIntensity > 0 && _digitalShader != null)
        {
            SetUpDigitalResources();
            
            var fcount = Time.frameCount;
            if (fcount % 13 == 0) Graphics.Blit(source, _trashFrame1);
            if (fcount % 73 == 0) Graphics.Blit(source, _trashFrame2);
            
            _digitalMaterial.SetFloat("_Intensity", _digitalIntensity);
            _digitalMaterial.SetTexture("_NoiseTex", _noiseTexture);
            var trashFrame = Random.value > 0.5f ? _trashFrame1 : _trashFrame2;
            _digitalMaterial.SetTexture("_TrashTex", trashFrame);
            
            tempTexture = RenderTexture.GetTemporary(source.width, source.height);
            Graphics.Blit(source, tempTexture, _digitalMaterial);
        }
        
        // 模拟故障处理
        if (_enableAnalogGlitch && (_scanLineJitter > 0 || _verticalJump > 0 || _horizontalShake > 0 || _colorDrift > 0) 
            && _analogShader != null)
        {
            SetUpAnalogResources();
            
            _verticalJumpTime += Time.deltaTime * _verticalJump * 11.3f;
            
            var sl_thresh = Mathf.Clamp01(1.0f - _scanLineJitter * 1.2f);
            var sl_disp = 0.002f + Mathf.Pow(_scanLineJitter, 3) * 0.05f;
            _analogMaterial.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));
            
            var vj = new Vector2(_verticalJump, _verticalJumpTime);
            _analogMaterial.SetVector("_VerticalJump", vj);
            
            _analogMaterial.SetFloat("_HorizontalShake", _horizontalShake * 0.2f);
            
            var cd = new Vector2(_colorDrift * 0.04f, Time.time * 606.11f);
            _analogMaterial.SetVector("_ColorDrift", cd);
            
            var inputTexture = tempTexture != null ? tempTexture : source;
            Graphics.Blit(inputTexture, destination, _analogMaterial);
        }
        else if (tempTexture != null)
        {
            Graphics.Blit(tempTexture, destination);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
        
        if (tempTexture != null)
        {
            RenderTexture.ReleaseTemporary(tempTexture);
        }
    }
    
    void OnDisable()
    {
        if (_digitalMaterial != null) DestroyImmediate(_digitalMaterial);
        if (_analogMaterial != null) DestroyImmediate(_analogMaterial);
        if (_noiseTexture != null) DestroyImmediate(_noiseTexture);
        if (_trashFrame1 != null) DestroyImmediate(_trashFrame1);
        if (_trashFrame2 != null) DestroyImmediate(_trashFrame2);
    }
    
    #endregion
}