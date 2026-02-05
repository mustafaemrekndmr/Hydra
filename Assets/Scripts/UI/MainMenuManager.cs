using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadOceanScene()
    {
        Debug.Log("Okyanus Sahnesi Yükleniyor...");
        SceneManager.LoadScene("OceanScene");
    }

    public void LoadPoolScene()
    {
        Debug.Log("Havuz Sahnesi Yükleniyor...");
        SceneManager.LoadScene("PoolScene");
    }

    public void QuitGame()
    {
        Debug.Log("Çıkış Yapılıyor...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
