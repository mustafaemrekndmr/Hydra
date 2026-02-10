using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BuildPauseMenu : MonoBehaviour
{
    private bool hasBuilt = false;
    
    void OnEnable()
    {
        if (!hasBuilt)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += InstallDelayed;
            #endif
        }
    }
    
    void InstallDelayed()
    {
        if (!hasBuilt)
        {
            hasBuilt = true;
            Install();
            
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(gameObject);
            }
            #endif
        }
    }

    void Install()
    {
        // Canvas Bul veya Oluştur
        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (!canvasObj)
        {
            Canvas existingCanvas = FindAnyObjectByType<Canvas>();
            if (existingCanvas) 
            {
                // Mevcut canvas'ı kullan ama adını GameCanvas yapma, olduğu gibi kalsın
                canvasObj = existingCanvas.gameObject;
            }
            else
            {
                canvasObj = new GameObject("GameCanvas");
                canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        // Scriptleri Ekle
        PauseMenu menuScript = canvasObj.GetComponent<PauseMenu>();
        if (!menuScript) menuScript = canvasObj.AddComponent<PauseMenu>();
        
        PauseMenuBinder binder = canvasObj.GetComponent<PauseMenuBinder>();
        if (!binder) canvasObj.AddComponent<PauseMenuBinder>();

        // Pause Paneli Oluştur
        GameObject panelObj = new GameObject("PauseMenuPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image img = panelObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.85f); // Koyu arka plan
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Başlangıçta gizli yap - sadece ESC'ye basıldığında görünsün
        panelObj.SetActive(false);

        // Başlık
        GameObject title = new GameObject("Title");
        title.transform.SetParent(panelObj.transform, false);
        Text t = title.AddComponent<Text>();
        t.text = "PAUSED";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 60;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);
        t.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);

        // Butonlar
        CreateButton("ResumeButton", "RESUME", 0, panelObj.transform);
        CreateButton("MenuButton", "MAIN MENU", -80, panelObj.transform);
        CreateButton("QuitButton", "QUIT", -160, panelObj.transform);

        // Script referansını bağla
        menuScript.pauseMenuUI = panelObj;
        
        // Başlangıçta gizle
        panelObj.SetActive(false);

        Debug.Log("✅ Pause Menu başarıyla kuruldu.");
    }

    void CreateButton(string name, string label, float yPos, Transform parent)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.1f);
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 60);
        rect.anchoredPosition = new Vector2(0, yPos);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cols = btn.colors;
        cols.normalColor = new Color(1, 1, 1, 0.1f);
        cols.highlightedColor = new Color(0, 1, 1, 0.5f);
        cols.pressedColor = new Color(0, 0.5f, 0.8f, 1f);
        cols.selectedColor = cols.highlightedColor;
        btn.colors = cols;

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(btnObj.transform, false);
        Text t = txt.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 24;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        t.GetComponent<RectTransform>().anchorMax = Vector2.one;
        t.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        t.GetComponent<RectTransform>().offsetMax = Vector2.zero;
    }
}
