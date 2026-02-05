using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class ProjectSetupHelper : EditorWindow
{
    [MenuItem("Tools/ROV Project/Setup Project")]
    public static void SetupProject()
    {
        Debug.Log("Setting up ROV Project...");

        // 1. Add scenes to build settings
        AddScenesToBuildSettings();

        // 2. Fix any missing references
        Debug.Log("Project setup complete!");
        EditorUtility.DisplayDialog("Setup Complete", 
            "Project has been configured!\n\n" +
            "Scenes added to Build Settings:\n" +
            "0. MainMenu\n" +
            "1. OceanScene\n" +
            "2. PoolScene", 
            "OK");
    }

    [MenuItem("Tools/ROV Project/Build ADVANCED Indoor Pool")]
    public static void BuildAdvancedPool()
    {
        // Load PoolScene
        EditorSceneManager.OpenScene("Assets/Scenes/PoolScene.unity");
        
        // Clear the scene first
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.transform.parent == null)
            {
                DestroyImmediate(obj);
            }
        }

        // Create the advanced builder
        GameObject builder = new GameObject("AdvancedPoolBuilder");
        builder.AddComponent<BuildAdvancedIndoorPool>();

        Debug.Log("Gelismis Havuz Sahnesi olusturuluyor...");
        EditorUtility.DisplayDialog("Gelismis Havuz Sahnesi", 
            "GELISMIS Kapali Havuz Olusturuluyor!\n\n" +
            "Ozellikler:\n" +
            "- Gerstner Dalga Simulasyonu\n" +
            "- Gelismis Yuzdurme Fizigi (F = rho*g*V)\n" +
            "- Volumetrik Aydinlatma & Tanri Isinlari\n" +
            "- Islaklik Efektli Prosedurel Fayanslar\n" +
            "- Caustics & Kirilma\n" +
            "- Partikul Efektleri (Baloncuk, Toz, Sicrama)\n\n" +
            "Sahne otomatik olarak olusturulacak!\n" +
            "Birkac saniye bekleyin...", 
            "Tamam");
    }

    [MenuItem("Tools/ROV Project/Rebuild Main Menu")]
    public static void RebuildMainMenu()
    {
        // Load MainMenu scene
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        // Find and destroy old canvas
        GameObject oldCanvas = GameObject.Find("MainMenuCanvas");
        if (oldCanvas) DestroyImmediate(oldCanvas);

        // Create UI installer
        GameObject installer = new GameObject("UI_Installer");
        installer.AddComponent<BuildMainMenuUI>();

        Debug.Log("Ana Menu UI olusturucu eklendi. Play moduna girin.");
        EditorUtility.DisplayDialog("Ana Menu Kurulumu", 
            "Ana Menu Olusturucu Eklendi!\n\n" +
            "Ana menuyu olusturmak için PLAY tusuna basin.", 
            "Tamam");
    }

    static void AddScenesToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        // Add scenes in order
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/OceanScene.unity",
            "Assets/Scenes/PoolScene.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            if (System.IO.File.Exists(scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"Build'e eklendi: {scenePath}");
            }
            else
            {
                Debug.LogWarning($"Sahne bulunamadi: {scenePath}");
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"Build ayarlari {scenes.Count} sahne ile guncellendi");
    }

    [MenuItem("Tools/ROV Project/Show Help")]
    public static void ShowHelp()
    {
        EditorUtility.DisplayDialog("ROV Projesi Yardim", 
            "ROV SUALTI SIMULASYONU PROJESI\n\n" +
            "KURULUM ADIMLARI:\n" +
            "1. Tools > ROV Project > Setup Project\n" +
            "   (Sahneleri build ayarlarina ekler)\n\n" +
            "2. Tools > ROV Project > Rebuild Main Menu\n" +
            "   (Ana menu UI'ini olusturur)\n\n" +
            "3. Tools > ROV Project > Build ADVANCED Indoor Pool\n" +
            "   (Gelismis kapali havuz ortami)\n\n" +
            "KONTROLLER:\n" +
            "W/S - Ileri/Geri\n" +
            "A/D - Sola/Saga Kayma\n" +
            "Q/E - Yukari/Asagi\n" +
            "C/V - Sola/Saga Donus\n" +
            "UP/DOWN - Kamera Egimi\n" +
            "ESC - Duraklat Menusu\n" +
            "SPACE - Derinlik Sabitleme (sadece ROV)", 
            "Anladim!");
    }
}
