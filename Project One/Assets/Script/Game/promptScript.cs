using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class promptScript : MonoBehaviour
{
    public Sprite effectivePrompt;
    public Sprite invalidPrompt;

    public bool invalid =false;
    public int noteID;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = invalidPrompt;
        spriteRenderer.sortingLayerName ="Player";
    }

    private void Update()
    {
       
       if(invalid)
       {
        
        StartCoroutine(AutoSwitchToInvalid());
       }
    }

    public void InitializeNote(int id)
    {
        noteID = id;
        invalid = false;
        spriteRenderer.sprite = invalidPrompt;
        
    }

    public void SetNoteState(bool isValid)
    {
        invalid = isValid;
        // Debug.Log(invalid);
        spriteRenderer.sprite = isValid ? effectivePrompt : invalidPrompt;
    }

    public void ShowPromptAndSwitch()
    {
        gameObject.SetActive(true);
        StartCoroutine(AutoSwitchToInvalid());
    }

    private IEnumerator AutoSwitchToInvalid()
    {
        spriteRenderer.sprite = effectivePrompt;
        yield return new WaitForSeconds(1f);
        SetNoteState(false);
    }
}
