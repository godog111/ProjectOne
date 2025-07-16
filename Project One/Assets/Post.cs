using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Post : MonoBehaviour
{
    public Material EffectMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (EffectMaterial != null)
        {
            Graphics.Blit(source, destination, EffectMaterial);
        }
    }
}
