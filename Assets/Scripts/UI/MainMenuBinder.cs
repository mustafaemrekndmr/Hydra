using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuBinder : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(BindButtonsAfterDelay());
    }

    IEnumerator BindButtonsAfterDelay()
    {
        // Wait for UI to be built
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        MainMenuManager manager = GetComponent<MainMenuManager>();
        if (manager == null)
        {
            Debug.LogError("MainMenuManager not found!");
            yield break;
        }
        
        Button oceanBtn = transform.Find("Btn_Ocean")?.GetComponent<Button>();
        Button poolBtn = transform.Find("Btn_Pool")?.GetComponent<Button>();
        Button quitBtn = transform.Find("Btn_Quit")?.GetComponent<Button>();

        if(oceanBtn) 
        {
            oceanBtn.onClick.AddListener(manager.LoadOceanScene);
            Debug.Log("Ocean button bound");
        }
        else Debug.LogWarning("Ocean button not found");

        if(poolBtn) 
        {
            poolBtn.onClick.AddListener(manager.LoadPoolScene);
            Debug.Log("Pool button bound");
        }
        else Debug.LogWarning("Pool button not found");

        if(quitBtn) 
        {
            quitBtn.onClick.AddListener(manager.QuitGame);
            Debug.Log("Quit button bound");
        }
        else Debug.LogWarning("Quit button not found");
    }
}
