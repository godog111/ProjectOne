using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TextBlockHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Required Parameters")]
    public TextMeshProUGUI textMesh;
    public Material effectMaterial;

    [Header("Split Settings")]
    public bool autoSplitOnStart = true;
    public float characterSpacing = 10f;

    [Header("Effect Parameters")]
    [Range(0.1f, 2f)] public float distortionStrength = 0.5f;
    [Range(0.1f, 1f)] public float effectRadius = 0.3f;

    [Header("Animation Parameters")]
    public float spreadDistance = 20f;
    public float animationDuration = 0.5f;

    private Material originalMaterial;
    private bool isHovering;
    private List<RectTransform> textParts = new List<RectTransform>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private RectTransform parentRectTransform;
    private bool isInitialized = false;
    private Dictionary<GameObject, LTDescr> activeTweens = new Dictionary<GameObject, LTDescr>();

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        parentRectTransform = GetComponent<RectTransform>();
        
        if (parentRectTransform == null)
        {
            Debug.LogError("Script must be attached to a UI object!", this);
            return;
        }

        if (textMesh == null) 
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>(true);
            if (textMesh == null)
            {
                Debug.LogError("No TextMeshProUGUI component found!", this);
                return;
            }
        }

        originalMaterial = textMesh.fontSharedMaterial;
        
        if (!TryGetComponent<Graphic>(out var graphic))
        {
            var image = gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            image.raycastTarget = true;
        }
        else
        {
            graphic.raycastTarget = true;
        }

        textMesh.raycastTarget = false;

        if (autoSplitOnStart)
        {
            SplitText();
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        CleanupAllTweens();
    }

    private void OnDisable()
    {
        CleanupAllTweens();
    }

    private void CleanupAllTweens()
    {
        foreach (var tween in activeTweens.Values)
        {
            if (tween != null)
            {
                LeanTween.cancel(tween.id);
            }
        }
        activeTweens.Clear();
    }

    private void SplitText()
    {
        CleanupAllTweens();
        textParts.Clear();
        originalPositions.Clear();

        foreach (Transform child in transform)
        {
            if (child != textMesh.transform && child.GetComponent<TextMeshProUGUI>() != null)
                Destroy(child.gameObject);
        }

        string text = textMesh.text;
        if (string.IsNullOrEmpty(text)) return;

        float totalWidth = 0f;
        List<float> charWidths = new List<float>();
        
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i])) continue;
            
            TextMeshProUGUI tempChar = Instantiate(textMesh, transform);
            tempChar.text = text[i].ToString();
            tempChar.ForceMeshUpdate();
            charWidths.Add(tempChar.preferredWidth);
            totalWidth += tempChar.preferredWidth + (i < text.Length - 1 ? characterSpacing : 0);
            Destroy(tempChar.gameObject);
        }

        Vector3 startPos = new Vector3(-totalWidth / 2f, 0, 0);
        float currentX = startPos.x;

        int charIndex = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i])) continue;

            GameObject charObj = new GameObject($"Char_{i}", typeof(RectTransform));
            charObj.transform.SetParent(transform);
            charObj.transform.localPosition = Vector3.zero;
            charObj.transform.localRotation = Quaternion.identity;
            charObj.transform.localScale = Vector3.one;

            TextMeshProUGUI charTMP = charObj.AddComponent<TextMeshProUGUI>();
            CopyTextProperties(textMesh, charTMP);
            charTMP.text = text[i].ToString();
            charTMP.alignment = TextAlignmentOptions.Center;
            charTMP.raycastTarget = false;

            RectTransform rt = charObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(charWidths[charIndex], textMesh.preferredHeight);
            rt.anchoredPosition = new Vector2(currentX + charWidths[charIndex]/2f, 0);
            
            textParts.Add(rt);
            originalPositions.Add(rt.localPosition);
            
            currentX += charWidths[charIndex] + characterSpacing;
            charIndex++;
        }

        textMesh.gameObject.SetActive(false);
    }

    private void CopyTextProperties(TextMeshProUGUI source, TextMeshProUGUI target)
    {
        target.font = source.font;
        target.fontStyle = source.fontStyle;
        target.fontSize = source.fontSize;
        target.fontSizeMin = source.fontSizeMin;
        target.fontSizeMax = source.fontSizeMax;
        target.enableAutoSizing = source.enableAutoSizing;
        target.color = source.color;
        target.alpha = source.alpha;
        target.enableWordWrapping = source.enableWordWrapping;
        target.overflowMode = source.overflowMode;
        target.margin = source.margin;
        target.characterSpacing = source.characterSpacing;
        target.wordSpacing = source.wordSpacing;
        target.lineSpacing = source.lineSpacing;
        target.paragraphSpacing = source.paragraphSpacing;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized || textParts.Count == 0) return;
        
        isHovering = true;
        
        foreach (var part in textParts)
        {
            if (part == null) continue;
            
            TextMeshProUGUI tmp = part.GetComponent<TextMeshProUGUI>();
            if (tmp != null) 
            {
                tmp.fontSharedMaterial = effectMaterial;
            }
        }

        StartCoroutine(UpdateEffect());

        for (int i = 0; i < textParts.Count; i++)
        {
            if (textParts[i] == null) continue;
            
            GameObject partObj = textParts[i].gameObject;
            
            if (activeTweens.ContainsKey(partObj))
            {
                LeanTween.cancel(activeTweens[partObj].id);
                activeTweens.Remove(partObj);
            }

            Vector3 randomDir = Random.insideUnitCircle.normalized * spreadDistance;
            var tween = LeanTween.moveLocal(partObj, originalPositions[i] + randomDir, animationDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() => {
                    if (partObj != null && activeTweens.ContainsKey(partObj))
                    {
                        activeTweens.Remove(partObj);
                    }
                });
            
            activeTweens[partObj] = tween;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized || textParts.Count == 0) return;
        
        isHovering = false;
        
        foreach (var part in textParts)
        {
            if (part == null) continue;
            
            TextMeshProUGUI tmp = part.GetComponent<TextMeshProUGUI>();
            if (tmp != null) 
            {
                tmp.fontSharedMaterial = originalMaterial;
            }
        }

        for (int i = 0; i < textParts.Count; i++)
        {
            if (textParts[i] == null) continue;
            
            GameObject partObj = textParts[i].gameObject;
            
            if (activeTweens.ContainsKey(partObj))
            {
                LeanTween.cancel(activeTweens[partObj].id);
                activeTweens.Remove(partObj);
            }

            var tween = LeanTween.moveLocal(partObj, originalPositions[i], animationDuration)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() => {
                    if (partObj != null && activeTweens.ContainsKey(partObj))
                    {
                        activeTweens.Remove(partObj);
                    }
                });
            
            activeTweens[partObj] = tween;
        }
    }

    private IEnumerator UpdateEffect()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera eventCamera = canvas != null ? (canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera) : null;

        while (isHovering && isInitialized)
        {
            if (parentRectTransform == null) yield break;
            
            Vector2 localMousePos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform,
                Input.mousePosition,
                eventCamera,
                out localMousePos))
            {
                yield return null;
                continue;
            }

            Vector2 normalizedPos = new Vector2(
                (localMousePos.x + parentRectTransform.rect.width * 0.5f) / parentRectTransform.rect.width,
                (localMousePos.y + parentRectTransform.rect.height * 0.5f) / parentRectTransform.rect.height
            );

            if (effectMaterial != null)
            {
                effectMaterial.SetVector("_MousePos", new Vector4(normalizedPos.x, normalizedPos.y, 0, 0));
                effectMaterial.SetFloat("_Distortion", distortionStrength);
                effectMaterial.SetFloat("_Radius", effectRadius);
            }

            yield return null;
        }
    }
}