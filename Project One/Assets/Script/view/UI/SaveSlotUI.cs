using UnityEngine;
using UnityEngine.UI;
using System;

public class SaveSlotUI : BasePanel
{
    [Header("UI Elements")]
    [SerializeField] private Text slotNumberText;
    [SerializeField] private Text saveTimeText;
    [SerializeField] private Text previewText;
   // [SerializeField] private Image thumbnailImage;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    //[SerializeField] private Button newGameButton;

    public override string Name => throw new NotImplementedException();

    public override void HandleEvent(string eventName, object data)
    {
        throw new NotImplementedException();
    }

    public void SetupForExistingSave(int slotNumber, string saveTime, string previewText, 
                                  Sprite thumbnail, Action onLoad, Action onDelete)
    {
        slotNumberText.text = $" {slotNumber}";
        saveTimeText.text = saveTime;
        this.previewText.text = previewText;
       // thumbnailImage.sprite = thumbnail;
        
        loadButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(true);
       // newGameButton.gameObject.SetActive(false);
        
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => onLoad?.Invoke());
        
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete?.Invoke());
    }
    
    public void SetupForNewSave(int slotNumber, Action onCreate)
    {
        slotNumberText.text = $" {slotNumber}";
        saveTimeText.text = "空槽位";
        previewText.text = "开始新的冒险";
      //  thumbnailImage.sprite = null;
        
        loadButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
       // newGameButton.gameObject.SetActive(true);
        
       // newGameButton.onClick.RemoveAllListeners();
       // newGameButton.onClick.AddListener(() => onCreate?.Invoke());
    }
}