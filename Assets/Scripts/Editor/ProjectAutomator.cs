#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;

public static class ProjectAutomator
{
    private static readonly string ScenePath = "Assets/Scenes/MainGameScene.unity";

    [MenuItem("DigestiveQuest/1. تجهيز المشهد آلياً (Setup Game Scene)")]
    public static void SetupGameScene()
    {
        Debug.Log("[ProjectAutomator] بدء بناء واجهة اللعبة الكاملة والمشهد التفاعلي...");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. الكاميرا
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.09f, 0.12f, 0.18f); // أزرق داكن ليلي أنيق
        camGo.transform.position = new Vector3(0, 1.25f, -3.6f);
        camGo.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
        camGo.tag = "MainCamera";

        // 2. الإضاءة
        GameObject lightGo = new GameObject("Main Directional Light");
        Light dirLight = lightGo.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1.0f, 0.96f, 0.9f);
        dirLight.intensity = 1.4f;
        lightGo.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

        GameObject fillLightGo = new GameObject("Fill Light");
        Light fillLight = fillLightGo.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.6f, 0.75f, 1.0f);
        fillLight.intensity = 0.6f;
        fillLightGo.transform.rotation = Quaternion.Euler(-30f, 150f, 0f);

        // 3. النظم المركزية
        GameObject systemsGo = new GameObject("Systems");
        systemsGo.AddComponent<QuestionBankManager>();

        // 4. EventSystem
        GameObject eventSysGo = new GameObject("EventSystem");
        eventSysGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSysGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 5. استيراد المجسم ثلاثي الأبعاد للأعضاء
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/DigestiveSystemFull.fbx");
        if (modelPrefab != null)
        {
            GameObject digestiveModel = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            digestiveModel.name = "DigestiveSystem_3D";
            digestiveModel.transform.position = new Vector3(0, -0.25f, 0);
            digestiveModel.transform.localScale = Vector3.one * 1.25f;
            digestiveModel.AddComponent<ModelRotator>();

            SetupOrgan(digestiveModel, "Organ_Mouth_Esophagus", 0, "الفم والمريء");
            SetupOrgan(digestiveModel, "Organ_Stomach", 1, "المعدة");
            SetupOrgan(digestiveModel, "Organ_Liver_Pancreas", 2, "الكبد والبنكرياس والمرارة");
            SetupOrgan(digestiveModel, "Organ_Small_Intestine", 3, "الأمعاء الدقيقة");
            SetupOrgan(digestiveModel, "Organ_Large_Intestine", 4, "الأمعاء الغليظة والمستقيم");
        }

        // 6. بناء الـ Canvas الرئيسي
        GameObject canvasGo = new GameObject("UI_Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- أ. الشريط العلوي (Header Banner) ---
        GameObject headerPanel = CreateUIBox("HeaderPanel", canvasGo.transform, new Vector2(1600, 95), new Vector2(0, 475), new Color(0.12f, 0.18f, 0.28f, 0.92f));
        CreateText("TitleText", headerPanel.transform, new Vector2(1500, 45), new Vector2(0, 18), "🌟 رحلة استكشاف الجهاز الهضمي (Digestive Quest) 🌟", 34, new Color(1.0f, 0.85f, 0.2f), defaultFont);
        CreateText("SubText", headerPanel.transform, new Vector2(1500, 30), new Vector2(0, -22), "انقر على أي عضو في المجسم ثلاثي الأبعاد أو اختر من الأزرار بالأسفل لبدء التحدي!", 20, new Color(0.85f, 0.92f, 1.0f), defaultFont);

        // --- ب. أزرار الأعضاء السريعة في أسفل الشاشة (Bottom Organ Bar) ---
        GameObject bottomBar = CreateUIBox("BottomOrganBar", canvasGo.transform, new Vector2(1750, 95), new Vector2(0, -475), new Color(0.12f, 0.18f, 0.28f, 0.92f));
        
        Button[] bottomButtons = new Button[5];
        string[] btnLabels = { "👄 الفم والمريء", "🫁 المعدة", "🫀 الكبد والبنكرياس", "🌀 الأمعاء الدقيقة", "⭕ الأمعاء الغليظة" };
        Color[] btnColors = {
            new Color(0.85f, 0.35f, 0.45f),
            new Color(0.85f, 0.25f, 0.25f),
            new Color(0.65f, 0.25f, 0.15f),
            new Color(0.85f, 0.55f, 0.25f),
            new Color(0.70f, 0.35f, 0.35f)
        };

        for (int i = 0; i < 5; i++)
        {
            float posX = -660 + (i * 330);
            bottomButtons[i] = CreateButton($"OrganBtn_{i}", bottomBar.transform, new Vector2(310, 65), new Vector2(posX, 0), btnColors[i], btnLabels[i], Color.white, 22, defaultFont);
        }

        // --- ج. نافذة اختيار المراحل (Stages Modal) ---
        GameObject stagesModal = CreateUIBox("StagesModalPanel", canvasGo.transform, new Vector2(1920, 1080), Vector2.zero, new Color(0, 0, 0, 0.75f));
        GameObject modalCard = CreateUIBox("ModalCard", stagesModal.transform, new Vector2(850, 560), Vector2.zero, new Color(0.14f, 0.20f, 0.30f, 0.98f));
        
        Text modalTitle = CreateText("ModalTitle", modalCard.transform, new Vector2(800, 50), new Vector2(0, 230), "🌟 رحلة استكشاف: المعدة", 32, new Color(1.0f, 0.85f, 0.2f), defaultFont);

        Button[] stageBtns = new Button[3];
        Text[] stageStarTexts = new Text[3];
        GameObject[] lockIcons = new GameObject[3];

        string[] stageNames = {
            "المرحلة 1: استكشاف وتشريح (سهل)",
            "المرحلة 2: وظائف وكيمياء الهضم (متوسط)",
            "المرحلة 3: صحة وألغاز ذكاء (متقدم)"
        };
        Color[] stageColors = {
            new Color(0.2f, 0.65f, 0.35f),
            new Color(0.2f, 0.50f, 0.80f),
            new Color(0.85f, 0.45f, 0.15f)
        };

        for (int i = 0; i < 3; i++)
        {
            float posY = 110 - (i * 125);
            stageBtns[i] = CreateButton($"StageBtn_{i+1}", modalCard.transform, new Vector2(750, 95), new Vector2(0, posY), stageColors[i], stageNames[i], Color.white, 24, defaultFont);
            stageStarTexts[i] = CreateText($"StarText_{i}", stageBtns[i].transform, new Vector2(200, 30), new Vector2(250, 0), "⭐⭐⭐", 22, Color.yellow, defaultFont);
            
            // قفل للمراحل المقفلة
            GameObject lockObj = CreateUIBox($"Lock_{i}", stageBtns[i].transform, new Vector2(40, 40), new Vector2(-320, 0), Color.white);
            lockObj.SetActive(false);
            lockIcons[i] = lockObj;
        }

        Button closeModalBtn = CreateButton("CloseModalBtn", modalCard.transform, new Vector2(240, 55), new Vector2(0, -230), new Color(0.5f, 0.5f, 0.55f), "✕ رجوع للجسم", Color.white, 20, defaultFont);
        stagesModal.SetActive(false);

        // --- د. نافذة الأسئلة الكاملة (Quiz Screen) ---
        GameObject quizPanel = CreateUIBox("QuizGamePanel", canvasGo.transform, new Vector2(1920, 1080), Vector2.zero, new Color(0.08f, 0.11f, 0.17f, 0.98f));
        
        // شريط معلومات السؤال العلوي
        GameObject quizTopBar = CreateUIBox("QuizTopBar", quizPanel.transform, new Vector2(1600, 75), new Vector2(0, 480), new Color(0.14f, 0.20f, 0.30f));
        Text stageTitleText = CreateText("StageTitleText", quizTopBar.transform, new Vector2(500, 40), new Vector2(-480, 0), "المعدة - المستوى 1", 26, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleLeft);
        Text progressCounterText = CreateText("ProgressCounterText", quizTopBar.transform, new Vector2(300, 40), new Vector2(150, 0), "السؤال 1 من 10", 22, Color.white, defaultFont);
        
        // شريط التقدم الأخضر
        Slider progressBar = CreateSimpleSlider("ProgressBar", quizTopBar.transform, new Vector2(400, 20), new Vector2(-120, 0));
        Button closeQuizBtn = CreateButton("CloseQuizBtn", quizTopBar.transform, new Vector2(180, 50), new Vector2(680, 0), new Color(0.8f, 0.25f, 0.25f), "🏠 خروج", Color.white, 20, defaultFont);

        // بطاقة السؤال الكبرى
        GameObject questionCard = CreateUIBox("QuestionCard", quizPanel.transform, new Vector2(1300, 210), new Vector2(0, 240), new Color(0.16f, 0.23f, 0.35f));
        Text questionText = CreateText("QuestionText", questionCard.transform, new Vector2(1200, 170), Vector2.zero, "أين تبدأ أولى خطوات رحلة هضم الطعام في جسم الإنسان؟", 30, Color.white, defaultFont);

        // خيارات الإجابة الأربعة
        Button[] optionBtns = new Button[4];
        Text[] optionTexts = new Text[4];
        Vector2[] optPositions = {
            new Vector2(-340, 60),
            new Vector2(340, 60),
            new Vector2(-340, -50),
            new Vector2(340, -50)
        };

        for (int i = 0; i < 4; i++)
        {
            optionBtns[i] = CreateButton($"OptionBtn_{i}", quizPanel.transform, new Vector2(640, 85), optPositions[i], new Color(0.2f, 0.45f, 0.75f), $"الخيار {i+1}", Color.white, 24, defaultFont);
            optionTexts[i] = optionBtns[i].GetComponentInChildren<Text>();
        }

        // نافذة التغذية الراجعة (Feedback Popup)
        GameObject feedbackBox = CreateUIBox("FeedbackBox", quizPanel.transform, new Vector2(900, 300), new Vector2(0, -220), new Color(0.12f, 0.16f, 0.24f, 0.98f));
        Text feedbackTitle = CreateText("FeedbackTitle", feedbackBox.transform, new Vector2(850, 45), new Vector2(0, 110), "🌟 إجابة ممتازة وصحيحة!", 28, Color.green, defaultFont);
        Text feedbackTip = CreateText("FeedbackTip", feedbackBox.transform, new Vector2(850, 180), new Vector2(0, -15), "الشرح العلمي يظهر هنا لمساعدة الطفل.", 22, Color.white, defaultFont);
        feedbackBox.SetActive(false);

        // نافذة انتهاء المرحلة والاحتفال (Stage Complete)
        GameObject completeBox = CreateUIBox("CompleteBox", quizPanel.transform, new Vector2(900, 560), Vector2.zero, new Color(0.12f, 0.18f, 0.28f, 0.99f));
        CreateText("CompleteTitle", completeBox.transform, new Vector2(800, 55), new Vector2(0, 220), "🎉 أحسنت يا بطل! أنهيت المستوى بنجاح 🎉", 32, new Color(1.0f, 0.85f, 0.2f), defaultFont);
        
        GameObject[] stars = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            float posX = -120 + (i * 120);
            stars[i] = CreateText($"Star_{i}", completeBox.transform, new Vector2(80, 80), new Vector2(posX, 120), "⭐", 60, Color.yellow, defaultFont).gameObject;
        }

        Text finalScore = CreateText("FinalScore", completeBox.transform, new Vector2(800, 50), new Vector2(0, 20), "حصلت على 9 من 10 إجابات صحيحة!", 26, Color.white, defaultFont);
        Button retryBtn = CreateButton("RetryBtn", completeBox.transform, new Vector2(400, 65), new Vector2(0, -70), new Color(0.2f, 0.65f, 0.35f), "🔄 أسئلة عشوائية جديدة", Color.white, 22, defaultFont);
        Button backMapBtn = CreateButton("BackMapBtn", completeBox.transform, new Vector2(400, 65), new Vector2(0, -155), new Color(0.25f, 0.45f, 0.75f), "🗺️ العودة لخريطة الأعضاء", Color.white, 22, defaultFont);
        completeBox.SetActive(false);
        quizPanel.SetActive(false);

        // --- ربط المكونات بالسكربتات ---
        BodyExplorerUI explorerUI = canvasGo.AddComponent<BodyExplorerUI>();
        explorerUI.stagesModalPanel = stagesModal;
        explorerUI.modalTitleText = modalTitle;
        explorerUI.stageButtons = stageBtns;
        explorerUI.stageStarTexts = stageStarTexts;
        explorerUI.stageLockIcons = lockIcons;
        explorerUI.closeModalButton = closeModalBtn;
        explorerUI.bottomOrganButtons = bottomButtons;

        QuizGameController quizCtrl = canvasGo.AddComponent<QuizGameController>();
        quizCtrl.quizPanel = quizPanel;
        quizCtrl.stageTitleTextUI = stageTitleText;
        quizCtrl.progressCounterTextUI = progressCounterText;
        quizCtrl.progressBarUI = progressBar;
        quizCtrl.questionTextUI = questionText;
        quizCtrl.optionButtons = optionBtns;
        quizCtrl.optionTexts = optionTexts;
        quizCtrl.feedbackPanel = feedbackBox;
        quizCtrl.feedbackTitleText = feedbackTitle;
        quizCtrl.feedbackTipText = feedbackTip;
        quizCtrl.stageCompletePanel = completeBox;
        quizCtrl.finalScoreText = finalScore;
        quizCtrl.starIcons = stars;
        quizCtrl.closeQuizButton = closeQuizBtn;
        quizCtrl.retryButton = retryBtn;
        quizCtrl.backToMapButton = backMapBtn;

        explorerUI.quizController = quizCtrl;

        // حفظ المشهد
        string scenesDir = "Assets/Scenes";
        if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        Debug.Log("[ProjectAutomator] تم بناء وحفظ الواجهة والمشهد بنجاح تام!");
    }

    private static GameObject CreateUIBox(string name, Transform parent, Vector2 size, Vector2 anchoredPos, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static Button CreateButton(string name, Transform parent, Vector2 size, Vector2 anchoredPos, Color bgColor, string textStr, Color textColor, int fontSize, Font font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        Button btn = go.AddComponent<Button>();

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        Text t = textGo.AddComponent<Text>();
        t.text = textStr;
        t.font = font;
        t.fontSize = fontSize;
        t.color = textColor;
        t.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    private static Text CreateText(string name, Transform parent, Vector2 size, Vector2 anchoredPos, string textStr, int fontSize, Color color, Font font, TextAnchor align = TextAnchor.MiddleCenter)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        Text t = go.AddComponent<Text>();
        t.text = textStr;
        t.font = font;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        return t;
    }

    private static Slider CreateSimpleSlider(string name, Transform parent, Vector2 size, Vector2 anchoredPos)
    {
        GameObject sliderGo = new GameObject(name);
        sliderGo.transform.SetParent(parent, false);
        RectTransform rt = sliderGo.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        Slider slider = sliderGo.AddComponent<Slider>();

        GameObject bg = CreateUIBox("Background", sliderGo.transform, size, Vector2.zero, new Color(0.2f, 0.2f, 0.25f));
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = Vector2.zero;

        GameObject fill = CreateUIBox("Fill", fillArea.transform, Vector2.zero, Vector2.zero, new Color(0.2f, 0.8f, 0.35f));
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;

        slider.fillRect = fillRt;
        slider.minValue = 0;
        slider.maxValue = 10;
        slider.value = 0;

        return slider;
    }

    private static void SetupOrgan(GameObject parent, string childName, int organId, string arabicName)
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

        string sdkPath = @"C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK";
        string jdkPath = @"C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK";
        string ndkPath = @"C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK";
        
        if (Directory.Exists(sdkPath)) EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        if (Directory.Exists(jdkPath)) EditorPrefs.SetString("JdkPath", jdkPath);
        if (Directory.Exists(ndkPath)) EditorPrefs.SetString("AndroidNdkRoot", ndkPath);

        EditorUserBuildSettings.buildAppBundle = false;

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
