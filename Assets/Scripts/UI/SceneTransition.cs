using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    private static SceneTransition instance;
    
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create fade canvas if not assigned
        if (fadeCanvasGroup == null)
        {
            CreateFadeCanvas();
        }
    }
    
    void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Always on top
        
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        UnityEngine.UI.Image image = panelObj.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        
        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
    
    public static void LoadScene(string sceneName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.TransitionToScene(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    IEnumerator TransitionToScene(string sceneName)
    {
        // Fade out
        yield return StartCoroutine(Fade(1f));
        
        // Load scene
        SceneManager.LoadScene(sceneName);
        
        // Fade in
        yield return StartCoroutine(Fade(0f));
    }
    
    IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;
        
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;
        
        fadeCanvasGroup.blocksRaycasts = true;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0f;
    }
}
