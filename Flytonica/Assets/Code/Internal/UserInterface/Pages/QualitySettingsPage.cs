using Code.Internal.UserInterface.Pages;
using UnityEngine;
using UnityEngine.UI;

public class QualitySettingsPage : Page
{
    [SerializeField] private Button lqSettingButton, mqSettingsButton, hqSettingsButton;

    protected override void OnOpen()
    {
        base.OnOpen();
        
        int currentSettings = QualitySettings.GetQualityLevel();

        if (currentSettings == 0)
        {
            lqSettingButton.Select();
        }
        else if (currentSettings == 1)
        {
            mqSettingsButton.Select();
        }
        else if (currentSettings == 2)
        {
            hqSettingsButton.Select();
        }
        
        lqSettingButton.onClick.AddListener(() =>
        {
            QualityButtonPressed(0);
        });
        mqSettingsButton.onClick.AddListener(() =>
        {
            QualityButtonPressed(1);
        });
        hqSettingsButton.onClick.AddListener(() =>
        {
            QualityButtonPressed(2);
        });

    }

    private void QualityButtonPressed(int currentSettings)
    {
        PlayerPrefs.SetInt("QualitySettingsLevel", currentSettings);
        QualitySettings.SetQualityLevel(currentSettings, false);
        
        if (currentSettings == 0)
        {
            lqSettingButton.Select();
        }
        else if (currentSettings == 1)
        {
            mqSettingsButton.Select();
        }
        else if (currentSettings == 2)
        {
            hqSettingsButton.Select();
        }
    }
    
    protected override void OnClose()
    {
        base.OnClose();
        
        lqSettingButton.onClick.RemoveAllListeners();
        mqSettingsButton.onClick.RemoveAllListeners();
        hqSettingsButton.onClick.RemoveAllListeners();
    }
}