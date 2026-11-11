#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using TMPro;

/// <summary>
/// سكربت أتمتة وبناء المشروع بالكامل (ProjectAutomator).
/// هذا الكود محرك آلي يعمل داخل محرر Unity أو من سطر الأوامر (CLI):
/// 1. يبني المشهد الرئيسي (MainGameScene) برمجياً: يضيف الكاميرا، الإضاءة، المجسمات ثلاثية الأبعاد، والواجهات.
/// 2. يربط عناصر اللعبة (الأعضاء الخمسة، الأزرار، النصوص، نظام الأسئلة العشوائية).
/// 3. يبني نسخة الحاسوب بصيغة Windows Executable (.exe).
/// 4. يبني نسخة الهاتف بصيغة Android Package (.apk).
/// </summary>
public static class ProjectAutomator
{
    private static readonly string ScenePath = "Assets/Scenes/MainGameScene.unity";

    [MenuItem("DigestiveQuest/1. تجهيز المشهد آلياً (Setup Game Scene)")]
    public static void SetupGameScene()
    {
        Debug.Log("[ProjectAutomator] بدء بناء وتجهيز مشهد اللعبة التفاعلي...");

        // إنشاء مشهد جديد فارغ
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. إنشاء الكاميرا الرئيسية (Main Camera)
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.15f, 0.22f); // خلفية زرقاء داكنة هادئة
        camGo.transform.position = new Vector3(0, 1.15f, -3.8f);
        camGo.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
        camGo.tag = "MainCamera";

        // 2. إنشاء الإضاءة (Directional Light)
        GameObject lightGo = new GameObject("Directional Light");
        Light dirLight = lightGo.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = Color.white;
        dirLight.intensity = 1.3f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 3. إنشاء كائن النظم المركزية (Systems)
        GameObject systemsGo = new GameObject("Systems");
        systemsGo.AddComponent<QuestionBankManager>();

        // 4. إنشاء EventSystem للمس والفأرة
        GameObject eventSysGo = new GameObject("EventSystem");
        eventSysGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSysGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 5. استيراد مجسم الجهاز الهضمي ثلاثي الأبعاد
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/DigestiveSystemFull.fbx");
        GameObject digestiveModelInstance = null;
        if (modelPrefab != null)
        {
            digestiveModelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            digestiveModelInstance.name = "DigestiveSystem_3D";
            digestiveModelInstance.transform.position = new Vector3(0, 0, 0);

            // ربط السكربتات التفاعلية للأعضاء
            SetupOrganChild(digestiveModelInstance, "Organ_Mouth_Esophagus", 0, "الفم والمريء");
            SetupOrganChild(digestiveModelInstance, "Organ_Stomach", 1, "المعدة");
            SetupOrganChild(digestiveModelInstance, "Organ_Liver_Pancreas", 2, "الكبد والبنكرياس والمرارة");
            SetupOrganChild(digestiveModelInstance, "Organ_Small_Intestine", 3, "الأمعاء الدقيقة");
            SetupOrganChild(digestiveModelInstance, "Organ_Large_Intestine", 4, "الأمعاء الغليظة والمستقيم");
        }
        else
        {
            Debug.LogWarning("[ProjectAutomator] لم يتم العثور على ملف DigestiveSystemFull.fbx!");
        }

        // 6. إنشاء واجهة المستخدم الرئيسية (UI Canvas)
        GameObject canvasGo = new GameObject("UI_Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // واجهة اختيار المراحل BodyExplorerUI
        GameObject explorerUIGo = new GameObject("BodyExplorerUI");
        explorerUIGo.transform.SetParent(canvasGo.transform, false);
        BodyExplorerUI explorerUI = explorerUIGo.AddComponent<BodyExplorerUI>();

        // واجهة الأسئلة QuizGameController
        GameObject quizUIGo = new GameObject("QuizGameController");
        quizUIGo.transform.SetParent(canvasGo.transform, false);
        QuizGameController quizController = quizUIGo.AddComponent<QuizGameController>();

        // حفظ المشهد
        string scenesDir = "Assets/Scenes";
        if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);
        EditorSceneManager.SaveScene(scene, ScenePath);

        // إضافة المشهد لقائمة مشاهد البناء
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        Debug.Log("[ProjectAutomator] تم بناء وحفظ المشهد بنجاح في: " + ScenePath);
    }

    private static void SetupOrganChild(GameObject parent, string childName, int organId, string arabicName)
    {
        Transform child = parent.transform.Find(childName);
        if (child != null)
        {
            var selector = child.gameObject.GetComponent<Organ3DSelector>();
            if (selector == null) selector = child.gameObject.AddComponent<Organ3DSelector>();
            selector.organId = organId;
            selector.organArabicName = arabicName;

            if (child.gameObject.GetComponent<Collider>() == null)
            {
                child.gameObject.AddComponent<MeshCollider>();
            }
        }
    }

    [MenuItem("DigestiveQuest/2. بناء نسخة الحاسوب (Build PC .exe)")]
    public static void BuildWindows()
    {
        SetupGameScene();

        string buildDir = "Builds/Windows";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);
        string exePath = Path.Combine(buildDir, "DigestiveQuest.exe");

        Debug.Log("[ProjectAutomator] جاري بناء نسخة الحاسوب...");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[ProjectAutomator] نجح بناء نسخة الحاسوب بنجاح! الملف: {exePath}");
        }
        else
        {
            Debug.LogError($"[ProjectAutomator] فشل بناء نسخة الحاسوب: {report.summary.totalErrors} أخطاء.");
        }
    }

    [MenuItem("DigestiveQuest/3. بناء نسخة الأندرويد (Build Android .apk)")]
    public static void BuildAndroid()
    {
        SetupGameScene();

        string buildDir = "Builds/Android";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);
        string apkPath = Path.Combine(buildDir, "DigestiveQuest.apk");

        // ضبط مسارات SDK و JDK الخاصة بـ Unity تلقائياً
        string sdkPath = @"C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK";
        string jdkPath = @"C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK";
        string ndkPath = @"C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK";
        
        if (Directory.Exists(sdkPath)) EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        if (Directory.Exists(jdkPath)) EditorPrefs.SetString("JdkPath", jdkPath);
        if (Directory.Exists(ndkPath)) EditorPrefs.SetString("AndroidNdkRoot", ndkPath);

        EditorUserBuildSettings.buildAppBundle = false; // تصدير ملف APK مباشر

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.ghaith.digestivequest");
        PlayerSettings.productName = "رحلة في الجهاز الهضمي";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        Debug.Log("[ProjectAutomator] جاري بناء نسخة الأندرويد (APK)...");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[ProjectAutomator] نجح بناء نسخة الأندرويد بنجاح! الملف: {apkPath}");
        }
        else
        {
            Debug.LogError($"[ProjectAutomator] فشل بناء نسخة الأندرويد: {report.summary.totalErrors} أخطاء.");
        }
    }
}
#endif
