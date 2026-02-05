using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteInEditMode]
public class BuildMainMenuUI : MonoBehaviour
{
    private bool hasBuilt = false;
    
    void OnEnable()
    {
        if (!hasBuilt)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += BuildUIDelayed;
            #endif
        }
    }
    
    void BuildUIDelayed()
    {
        if (!hasBuilt)
        {
            hasBuilt = true;
            BuildUI();
            
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(gameObject);
            }
            #endif
        }
    }

    void BuildUI()
    {
        // 1. Temizlik
        GameObject existingCanvas = GameObject.Find("MainMenuCanvas");
        if (existingCanvas) DestroyImmediate(existingCanvas);

        // 2. Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Manager Ekle
        MainMenuManager manager = canvasObj.AddComponent<MainMenuManager>();
        canvasObj.AddComponent<MainMenuBinder>(); // Auto-bind buttons

        // 4. Arka Plan (Panel)
        GameObject bgObj = CreateImage("Background", canvasObj.transform, new Color(0.05f, 0.1f, 0.2f, 1f));
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 5. Başlık
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ROV SIMULATOR";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 80;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.cyan;
        titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 200);

        // 6. Butonlar (Ocean, Pool, Quit)
        CreateButton("Btn_Ocean", "🌊 OCEAN DIVE", 50, canvasObj.transform, manager.LoadOceanScene);
        CreateButton("Btn_Pool", "🏊 OLYMPIC POOL", -50, canvasObj.transform, manager.LoadPoolScene);
        CreateButton("Btn_Quit", "❌ QUIT", -150, canvasObj.transform, manager.QuitGame);

        Debug.Log("✅ Main Menu UI başarıyla oluşturuldu.");
    }

    GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    void CreateButton(string name, string label, float yPos, Transform parent, UnityAction action)
    {
        // Buton Arka Planı
        GameObject btnObj = CreateImage(name, parent, new Color(1f, 1f, 1f, 0.1f));
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 80);
        rect.anchoredPosition = new Vector2(0, yPos);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cols = btn.colors;
        cols.normalColor = new Color(1f, 1f, 1f, 0.1f);
        cols.highlightedColor = new Color(0f, 0.8f, 1f, 0.5f);
        cols.pressedColor = new Color(0f, 0.5f, 0.8f, 0.8f);
        cols.selectedColor = cols.highlightedColor;
        btn.colors = cols;

        // Kodla Event Bağlama (Persistent değil runtime event, ama manager aynı objede olduğu için sorun olmaz)
        // Editörde kalıcı bağlantı için UnityEvent.AddPersistentListener gerekir ama o Editor namespace'inde.
        // Bu yüzden runtime event ekliyoruz. Play modda çalışır.
        // Kalıcı olması için basit bir hile:
        // Standart Unity UI eventlerini script ile edit-time'da bağlamak zordur.
        // Kullanıcıya manuel bağla mı desek? Hayır, kullanıcı "kendin yap" dedi.
        // O zaman runtime'da çalışan bir "ButtonBinder" scripti ekleyelim.
        
        // Şimdilik sadece görsel oluşturuyorum. Bağlantı için "AutoBinder" scripti ekleyeceğim.
        
        // Text
        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text t = txtObj.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 32;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        t.GetComponent<RectTransform>().anchorMax = Vector2.one;
        t.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        t.GetComponent<RectTransform>().offsetMax = Vector2.zero;
    }
}
