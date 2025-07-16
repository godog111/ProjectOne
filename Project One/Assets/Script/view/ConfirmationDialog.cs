using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    private static ConfirmationDialog instance;
    
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }
    
    public static void Show(string title, string message, Action onConfirm, Action onCancel = null)
    {
        instance.titleText.text = title;
        instance.messageText.text = message;
        
        instance.confirmButton.onClick.RemoveAllListeners();
        instance.confirmButton.onClick.AddListener(() => {
            onConfirm?.Invoke();
            instance.gameObject.SetActive(false);
        });
        
        instance.cancelButton.onClick.RemoveAllListeners();
        instance.cancelButton.onClick.AddListener(() => {
            onCancel?.Invoke();
            instance.gameObject.SetActive(false);
        });
        
        instance.gameObject.SetActive(true);
    }
}