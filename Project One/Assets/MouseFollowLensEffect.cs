using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class UIMouseLensEffect : MonoBehaviour
{
    [Header("Lens Settings")]
    [Range(0.01f, 0.5f)] public float lensSize = 0.2f;
    [Range(0, 1f)] public float maxDistortion = 0.5f;
    [Range(1, 5f)] public float magnification = 2f;
    [Range(0.01f, 1f)] public float smoothness = 0.3f;

    [Header("UI Capture")]
    public Canvas targetCanvas;
    public RawImage uiDisplayImage;

    private Material material;
    private RenderTexture uiRenderTexture;

    void Start()
    {
        // 创建材质
        var shader = Shader.Find("Custom/SmoothMouseLens");
        material = new Material(shader);

        // 创建Render Texture捕获UI
        if (targetCanvas != null && uiDisplayImage != null)
        {
            uiRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            targetCanvas.worldCamera.targetTexture = uiRenderTexture;
            uiDisplayImage.texture = uiRenderTexture;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // 设置鼠标位置（0-1范围）
        Vector2 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        // 设置Shader参数
        material.SetVector("_MousePosition", mousePos);
        material.SetFloat("_LensSize", lensSize);
        material.SetFloat("_MaxDistortion", maxDistortion);
        material.SetFloat("_Magnification", magnification);
        material.SetFloat("_Smoothness", smoothness);

        // 合并UI和场景渲染
        if (uiRenderTexture != null)
        {
            // 临时纹理
            RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height);
            
            // 先处理场景
            Graphics.Blit(source, temp, material);
            
            // 再处理UI（使用相同的扭曲效果）
            material.SetTexture("_MainTex", uiRenderTexture);
            Graphics.Blit(uiRenderTexture, destination, material);
            
            // 混合结果
            Graphics.Blit(temp, destination);
            
            RenderTexture.ReleaseTemporary(temp);
        }
        else
        {
            Graphics.Blit(source, destination, material);
        }
    }

    void OnDisable()
    {
        if (uiRenderTexture != null) 
            uiRenderTexture.Release();
        if (material != null)
            DestroyImmediate(material);
    }
}