using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIGlitchController : MonoBehaviour
{
    public enum GlitchMode { None, Analog, Digital }

    [Header("Global Settings")]
    public GlitchMode glitchMode = GlitchMode.None;

    [Header("Analog Glitch Settings")]
    [Range(0, 1)] public float scanLineJitter = 0;
    [Range(0, 1)] public float verticalJump = 0;
    [Range(0, 1)] public float horizontalShake = 0;
    [Range(0, 1)] public float colorDrift = 0;

    [Header("Digital Glitch Settings")]
    [Range(0, 1)] public float digitalIntensity = 0;
    public Texture2D noiseTexture;
    public Texture2D trashTexture;

    // 关键修改：使用Shader而非Material
    private Shader analogShader;
    private Shader digitalShader;
    private Material analogMaterial;
    private Material digitalMaterial;

    private void OnEnable()
    {
        // 加载着色器
        analogShader = Shader.Find("UI/Kino/Glitch/Analog");
        digitalShader = Shader.Find("UI/Kino/Glitch/Digital");
        
        // 创建材质实例
        if(analogShader != null) analogMaterial = new Material(analogShader);
        if(digitalShader != null) digitalMaterial = new Material(digitalShader);
        
        UpdateAllUIElements();
    }

    private void Update()
    {
        if(glitchMode == GlitchMode.Analog && analogMaterial != null)
        {
            float sl_disp = 0.002f + Mathf.Pow(scanLineJitter, 3) * 0.05f;
            float sl_thresh = Mathf.Clamp01(1.0f - scanLineJitter * 1.2f);
            analogMaterial.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));
            analogMaterial.SetVector("_VerticalJump", new Vector2(verticalJump, Time.time * verticalJump * 11.3f));
            analogMaterial.SetFloat("_HorizontalShake", horizontalShake * 0.2f);
            analogMaterial.SetVector("_ColorDrift", new Vector2(colorDrift * 0.04f, Time.time * 606.11f));
        }
        else if(glitchMode == GlitchMode.Digital && digitalMaterial != null)
        {
            digitalMaterial.SetFloat("_Intensity", digitalIntensity);
            if(noiseTexture != null) digitalMaterial.SetTexture("_NoiseTex", noiseTexture);
            if(trashTexture != null) digitalMaterial.SetTexture("_TrashTex", trashTexture);
        }
    }

    // 关键修改：正确应用材质到UI元素
    public void UpdateAllUIElements()
    {
        Graphic[] uiElements = GetComponentsInChildren<Graphic>(true);
        
        foreach(Graphic element in uiElements)
        {
            // 跳过已经使用自定义材质的元素
            if(element.material == null || element.material.shader.name == "UI/Default")
            {
                switch(glitchMode)
                {
                    case GlitchMode.None:
                        element.material = null;
                        break;
                    case GlitchMode.Analog:
                        element.material = analogMaterial;
                        break;
                    case GlitchMode.Digital:
                        element.material = digitalMaterial;
                        break;
                }
            }
        }
    }

    private void OnDisable()
    {
        ResetAllUIElements();
        if(analogMaterial != null) DestroyImmediate(analogMaterial);
        if(digitalMaterial != null) DestroyImmediate(digitalMaterial);
    }

    public void ResetAllUIElements()
    {
        Graphic[] uiElements = GetComponentsInChildren<Graphic>(true);
        foreach(Graphic element in uiElements)
        {
            if(element.material != null && 
              (element.material.shader.name.Contains("Glitch/Analog") || 
               element.material.shader.name.Contains("Glitch/Digital")))
            {
                element.material = null;
            }
        }
    }
}