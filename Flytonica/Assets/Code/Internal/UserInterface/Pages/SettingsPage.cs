using Code.Internal.UserInterface.Pages;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPage : Page
{
    [SerializeField] private Button inputSettingsButton, qualitySettingsButton, audioSettingsButton, deviceInfoButton;
    [SerializeField] private Page inputSettingsPage, qualitySettingsPage, audioSettingsPage, deviceInfoPage;
    
    private void Start()
    {
        
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        inputSettingsButton.onClick.AddListener(OnInputSettingsClicked);
        qualitySettingsButton.onClick.AddListener(OnQualitySettingsClicked);
        audioSettingsButton.onClick.AddListener(OnAudioSettingsClicked);
        deviceInfoButton.onClick.AddListener(OnDeviceInfoClicked);
    }

    private void OnQualitySettingsClicked()
    {
        if (qualitySettingsPage != null) 
            qualitySettingsPage.Open();
    }

    private void OnAudioSettingsClicked()
    {
        if (audioSettingsPage != null)
            audioSettingsPage.Open();
    }

    private void OnDeviceInfoClicked()
    {
        if (deviceInfoPage != null)
            deviceInfoPage.Open();
    }

    private void OnInputSettingsClicked()
    {
        if (inputSettingsPage != null)
            inputSettingsPage.Open();
    }

    protected override void OnClose()
    {
        base.OnClose();
        
        inputSettingsButton.onClick.RemoveListener(OnInputSettingsClicked);
        qualitySettingsButton.onClick.RemoveListener(OnQualitySettingsClicked);
        audioSettingsButton.onClick.RemoveListener(OnAudioSettingsClicked);
        deviceInfoButton.onClick.RemoveListener(OnDeviceInfoClicked);
    }
}