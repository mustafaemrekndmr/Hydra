using UnityEngine;
using UnityEngine.UI;

public class PauseMenuBinder : MonoBehaviour
{
    void Start()
    {
        PauseMenu manager = GetComponent<PauseMenu>();
        if(!manager) return;

        // Pause Panel içindeki butonları bul (Panel ismi PauseMenuPanel olmalı)
        Transform panel = manager.pauseMenuUI.transform;

        Button resumeBtn = panel.Find("ResumeButton")?.GetComponent<Button>();
        Button menuBtn = panel.Find("MenuButton")?.GetComponent<Button>();
        Button quitBtn = panel.Find("QuitButton")?.GetComponent<Button>();

        if(resumeBtn) resumeBtn.onClick.AddListener(manager.Resume);
        if(menuBtn) menuBtn.onClick.AddListener(manager.LoadMenu);
        if(quitBtn) quitBtn.onClick.AddListener(manager.QuitGame);
    }
}
