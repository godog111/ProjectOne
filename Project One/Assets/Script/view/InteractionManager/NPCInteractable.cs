using UnityEngine;

/// <summary>
/// NPC交互具体实现
/// </summary>
public class NPCInteractable : InteractableBase
{
    [Header("NPC设置")]
    [SerializeField] private string _npcName = "村民";
   // [SerializeField] private Dialogue _dialogue;

    public override void Interact()
    {
        Debug.Log($"与 {_npcName} 对话");

    }

}