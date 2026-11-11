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
        Debug.Log("[ProjectAutomator] بدء بناء واجهة اللعبة الاحترافية المضبوطة...");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. الكاميرا مع مستشعر النقر المباشر على الأعضاء
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.08f, 0.13f);
        camGo.transform.position = new Vector3(0, 0.05f, -2.60f);
        camGo.transform.rotation = Quaternion.Euler(2.0f, 0f, 0f);
        camGo.tag = "MainCamera";

        // إضافة سكربت النقر المباشر على الكاميرا
        camGo.AddComponent<OrganRaycastClicker>();

        // 2. إضاءة الاستوديو ثلاثية النقاط
        GameObject keyLightGo = new GameObject("Key Light");
        Light keyLight = keyLightGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = new Color(1.0f, 0.97f, 0.92f);
        keyLight.intensity = 1.35f;
        keyLightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        GameObject fillLightGo = new GameObject("Fill Light");
        Light fillLight = fillLightGo.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.65f, 0.80f, 1.0f);
        fillLight.intensity = 0.70f;
        fillLightGo.transform.rotation = Quaternion.Euler(-20f, 140f, 0f);

        GameObject rimLightGo = new GameObject("Rim Light");
        Light rimLight = rimLightGo.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.color = new Color(0.40f, 0.85f, 1.0f);
        rimLight.intensity = 0.80f;
        rimLightGo.transform.rotation = Quaternion.Euler(-50f, 180f, 0f);

        // 3. النظم المركزية
        GameObject systemsGo = new GameObject("Systems");
        systemsGo.AddComponent<QuestionBankManager>();
        systemsGo.AddComponent<LocalizationManager>();
        WindowController windowCtrl = systemsGo.AddComponent<WindowController>();

        // 4. EventSystem
        GameObject eventSysGo = new GameObject("EventSystem");
        eventSysGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSysGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 5. استيراد المجسم التشريحي الطبي الواقعي
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/DigestiveSystemFull.fbx");
        if (modelPrefab != null)
        {
            GameObject digestiveModel = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            digestiveModel.name = "DigestiveSystem_3D";
            // ضبط الحجم والموضع بدقة ليكون في منتصف الشاشة تماماً دون أي تغطية من الأشرطة
            digestiveModel.transform.position = new Vector3(0, -0.02f, 0);
            digestiveModel.transform.localScale = Vector3.one * 0.95f;
            digestiveModel.AddComponent<ModelRotator>();

            SetupOrgan(digestiveModel, "Organ_Mouth_Esophagus", 0, "الفم والمريء");
            SetupOrgan(digestiveModel, "Organ_Stomach", 1, "المعدة");
            SetupOrgan(digestiveModel, "Organ_Liver_Pancreas", 2, "الكبد والبنكرياس والمرارة");
            SetupOrgan(digestiveModel, "Organ_Small_Intestine", 3, "الأمعاء الدقيقة");
            SetupOrgan(digestiveModel, "Organ_Large_Intestine", 4, "الأمعاء الغليظة والمستقيم");
        }

        // 6. واجهة المستخدم (Canvas 1920x1080)
        GameObject canvasGo = new GameObject("UI_Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        Font defaultFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/TahomaBold.ttf");
        if (defaultFont == null) defaultFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Tahoma.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- أ. الشريط العلوي النظيف والأنيق (بدون أزرار النافذة الزائدة، ومع خط Tahoma النظيف) ---
        GameObject headerPanel = CreateUIBox("HeaderPanel", canvasGo.transform, new Vector2(1860, 90), new Vector2(0, 485), new Color(0.09f, 0.13f, 0.22f, 0.95f));

        Text titleText = CreateText("TitleText", headerPanel.transform, new Vector2(1100, 44), new Vector2(-180, 16), ArabicTextFixer.Fix("مغامرة استكشاف الجهاز الهضمي"), 34, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);
        Text subText = CreateText("SubText", headerPanel.transform, new Vector2(1100, 30), new Vector2(-180, -22), ArabicTextFixer.Fix("انقر على أي عضو في المجسم أو اختر من البطاقات الجانبية لبدء التحدي!"), 21, new Color(0.85f, 0.92f, 1.0f), defaultFont, TextAnchor.MiddleCenter);

        // أزرار اللغات في أقصى اليمين
        GameObject langContainer = CreateUIBox("LanguageBar", headerPanel.transform, new Vector2(360, 54), new Vector2(680, 0), new Color(0.14f, 0.20f, 0.32f, 0.85f));
        Button btnAr = CreateButton("Btn_Arabic", langContainer.transform, new Vector2(110, 44), new Vector2(-115, 0), new Color(0.18f, 0.55f, 0.32f), ArabicTextFixer.Fix("العربية"), Color.white, 20, defaultFont);
        Button btnEn = CreateButton("Btn_English", langContainer.transform, new Vector2(110, 44), new Vector2(0, 0), new Color(0.20f, 0.40f, 0.65f), "English", Color.white, 20, defaultFont);
        Button btnFr = CreateButton("Btn_French", langContainer.transform, new Vector2(110, 44), new Vector2(115, 0), new Color(0.65f, 0.35f, 0.20f), "Français", Color.white, 20, defaultFont);

        // --- ب. البطاقات الجانبية الأنيقة للأعضاء (تستغل المساحات الفارغة على اليمين واليسار) ---
        Button[] bottomButtons = new Button[5];
        Text[] bottomTexts = new Text[5];
        string[] rawLabels = {
            "1. الفم والمريء",
            "2. المعدة",
            "3. الكبد والبنكرياس والمرارة",
            "4. الأمعاء الدقيقة",
            "5. الأمعاء الغليظة والمستقيم"
        };
        Color[] accentColors = {
            new Color(0.88f, 0.35f, 0.42f),
            new Color(0.82f, 0.25f, 0.25f),
            new Color(0.55f, 0.18f, 0.15f),
            new Color(0.88f, 0.55f, 0.25f),
            new Color(0.70f, 0.35f, 0.35f)
        };

        // الجانب الأيسر: الأعضاء 0 و 1 و 2
        for (int i = 0; i < 3; i++)
        {
            float posY = 130 - (i * 140);
            GameObject cardGo = CreateUIBox($"OrganCard_{i}", canvasGo.transform, new Vector2(340, 120), new Vector2(-750, posY), new Color(0.10f, 0.15f, 0.24f, 0.94f));
            CreateUIBox("Accent", cardGo.transform, new Vector2(10, 120), new Vector2(-165, 0), accentColors[i]);

            bottomButtons[i] = CreateButton($"OrganBtn_{i}", cardGo.transform, new Vector2(310, 110), new Vector2(10, 0), new Color(0, 0, 0, 0.01f), "", Color.white, 20, defaultFont);
            bottomTexts[i] = CreateText($"Label_{i}", cardGo.transform, new Vector2(290, 40), new Vector2(15, 20), ArabicTextFixer.Fix(rawLabels[i]), 22, Color.white, defaultFont, TextAnchor.MiddleRight);
            CreateText($"Sub_{i}", cardGo.transform, new Vector2(290, 30), new Vector2(15, -20), ArabicTextFixer.Fix("3 مستويات استكشافية | 30 سؤالاً"), 16, new Color(0.7f, 0.82f, 0.95f), defaultFont, TextAnchor.MiddleRight);
        }

        // الجانب الأيمن: الأعضاء 3 و 4 + بطاقة استكشاف 3D
        for (int i = 3; i < 5; i++)
        {
            int rightIdx = i - 3;
            float posY = 130 - (rightIdx * 140);
            GameObject cardGo = CreateUIBox($"OrganCard_{i}", canvasGo.transform, new Vector2(340, 120), new Vector2(750, posY), new Color(0.10f, 0.15f, 0.24f, 0.94f));
            CreateUIBox("Accent", cardGo.transform, new Vector2(10, 120), new Vector2(165, 0), accentColors[i]);

            bottomButtons[i] = CreateButton($"OrganBtn_{i}", cardGo.transform, new Vector2(310, 110), new Vector2(-10, 0), new Color(0, 0, 0, 0.01f), "", Color.white, 20, defaultFont);
            bottomTexts[i] = CreateText($"Label_{i}", cardGo.transform, new Vector2(290, 40), new Vector2(-15, 20), ArabicTextFixer.Fix(rawLabels[i]), 22, Color.white, defaultFont, TextAnchor.MiddleLeft);
            CreateText($"Sub_{i}", cardGo.transform, new Vector2(290, 30), new Vector2(-15, -20), ArabicTextFixer.Fix("3 مستويات استكشافية | 30 سؤالاً"), 16, new Color(0.7f, 0.82f, 0.95f), defaultFont, TextAnchor.MiddleLeft);
        }

        // بطاقة إرشادات التفاعل 3D في أسفل اليمين
        GameObject tipCard = CreateUIBox("TipCard_3D", canvasGo.transform, new Vector2(340, 120), new Vector2(750, -150), new Color(0.08f, 0.18f, 0.28f, 0.92f));
        CreateUIBox("AccentTip", tipCard.transform, new Vector2(10, 120), new Vector2(165, 0), new Color(0.2f, 0.8f, 0.7f));
        CreateText("TipTitle", tipCard.transform, new Vector2(290, 35), new Vector2(-15, 22), ArabicTextFixer.Fix("استكشاف تفاعلي 360°"), 20, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);
        CreateText("TipDesc", tipCard.transform, new Vector2(290, 48), new Vector2(-15, -18), ArabicTextFixer.Fix("اسحب بالفأرة لتدوير الجسم\nوانقر مباشرة على أي عضو لفتحه!"), 16, new Color(0.85f, 0.92f, 1.0f), defaultFont, TextAnchor.MiddleCenter);

        // --- ج. نافذة اختيار المراحل (Stages Modal) ---
        GameObject stagesModal = CreateUIBox("StagesModalPanel", canvasGo.transform, new Vector2(1920, 1080), Vector2.zero, new Color(0, 0, 0, 0.82f));
        GameObject modalCard = CreateUIBox("ModalCard", stagesModal.transform, new Vector2(900, 600), Vector2.zero, new Color(0.11f, 0.16f, 0.26f, 0.99f));

        Text modalTitle = CreateText("ModalTitle", modalCard.transform, new Vector2(850, 55), new Vector2(0, 245), ArabicTextFixer.Fix("رحلة استكشاف: المعدة"), 32, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);

        Button[] stageBtns = new Button[3];
        Text[] stageBtnTexts = new Text[3];
        Text[] stageStarTexts = new Text[3];
        GameObject[] lockIcons = new GameObject[3];

        string[] stageNames = {
            "المستوى 1: تشريح واستكشاف (سهل)",
            "المستوى 2: وظائف وكيمياء الهضم (متوسط)",
            "المستوى 3: صحة وألغاز ذكاء (متقدم)"
        };

        for (int i = 0; i < 3; i++)
        {
            float posY = 120 - (i * 135);
            stageBtns[i] = CreateButton($"StageBtn_{i+1}", modalCard.transform, new Vector2(800, 105), new Vector2(0, posY), new Color(0.18f, 0.5f, 0.8f), "", Color.white, 24, defaultFont);
            stageBtnTexts[i] = CreateText($"NameText_{i}", stageBtns[i].transform, new Vector2(550, 80), new Vector2(-100, 0), ArabicTextFixer.Fix(stageNames[i]), 24, Color.white, defaultFont, TextAnchor.MiddleLeft);
            stageStarTexts[i] = CreateText($"StarText_{i}", stageBtns[i].transform, new Vector2(220, 40), new Vector2(270, 0), "", 22, Color.yellow, defaultFont, TextAnchor.MiddleRight);

            GameObject lockObj = CreateUIBox($"Lock_{i}", stageBtns[i].transform, new Vector2(40, 40), new Vector2(-360, 0), Color.white);
            lockObj.SetActive(false);
            lockIcons[i] = lockObj;
        }

        Button closeModalBtn = CreateButton("CloseModalBtn", modalCard.transform, new Vector2(280, 60), new Vector2(0, -250), new Color(0.45f, 0.45f, 0.52f), ArabicTextFixer.Fix("رجوع للجسم"), Color.white, 22, defaultFont);
        stagesModal.SetActive(false);

        // --- د. نافذة الأسئلة الكاملة (Quiz Screen) ---
        GameObject quizPanel = CreateUIBox("QuizGamePanel", canvasGo.transform, new Vector2(1920, 1080), Vector2.zero, new Color(0.06f, 0.09f, 0.15f, 0.99f));

        GameObject quizTopBar = CreateUIBox("QuizTopBar", quizPanel.transform, new Vector2(1700, 80), new Vector2(0, 485), new Color(0.11f, 0.16f, 0.26f));
        Text stageTitleText = CreateText("StageTitleText", quizTopBar.transform, new Vector2(600, 45), new Vector2(-500, 0), ArabicTextFixer.Fix("المعدة - المستوى 1"), 26, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleLeft);
        Text progressCounterText = CreateText("ProgressCounterText", quizTopBar.transform, new Vector2(300, 45), new Vector2(180, 0), ArabicTextFixer.Fix("السؤال 1 من 10"), 22, Color.white, defaultFont, TextAnchor.MiddleCenter);

        Slider progressBar = CreateSimpleSlider("ProgressBar", quizTopBar.transform, new Vector2(450, 22), new Vector2(-120, 0));
        Button closeQuizBtn = CreateButton("CloseQuizBtn", quizTopBar.transform, new Vector2(180, 55), new Vector2(720, 0), new Color(0.85f, 0.22f, 0.22f), ArabicTextFixer.Fix("خروج"), Color.white, 22, defaultFont);

        // بطاقة السؤال
        GameObject questionCard = CreateUIBox("QuestionCard", quizPanel.transform, new Vector2(1400, 230), new Vector2(0, 240), new Color(0.13f, 0.19f, 0.30f));
        Text questionText = CreateText("QuestionText", questionCard.transform, new Vector2(1300, 190), Vector2.zero, ArabicTextFixer.Fix("أين تبدأ أولى خطوات رحلة هضم الطعام في جسم الإنسان؟"), 32, Color.white, defaultFont, TextAnchor.MiddleCenter);

        // خيارات الإجابة
        Button[] optionBtns = new Button[4];
        Text[] optionTexts = new Text[4];
        Vector2[] optPositions = {
            new Vector2(-360, 60),
            new Vector2(360, 60),
            new Vector2(-360, -60),
            new Vector2(360, -60)
        };

        for (int i = 0; i < 4; i++)
        {
            optionBtns[i] = CreateButton($"OptionBtn_{i}", quizPanel.transform, new Vector2(690, 95), optPositions[i], new Color(0.18f, 0.40f, 0.70f), $"Option {i+1}", Color.white, 24, defaultFont);
            optionTexts[i] = optionBtns[i].GetComponentInChildren<Text>();
        }

        // نافذة التغذية الراجعة
        GameObject feedbackBox = CreateUIBox("FeedbackBox", quizPanel.transform, new Vector2(1000, 320), new Vector2(0, -220), new Color(0.10f, 0.14f, 0.22f, 0.99f));
        Text feedbackTitle = CreateText("FeedbackTitle", feedbackBox.transform, new Vector2(950, 50), new Vector2(0, 115), ArabicTextFixer.Fix("إجابة ممتازة وصحيحة!"), 30, Color.green, defaultFont, TextAnchor.MiddleCenter);
        Text feedbackTip = CreateText("FeedbackTip", feedbackBox.transform, new Vector2(950, 190), new Vector2(0, -20), "", 24, Color.white, defaultFont, TextAnchor.MiddleCenter);
        feedbackBox.SetActive(false);

        // نافذة انتهاء المرحلة والاحتفال
        GameObject completeBox = CreateUIBox("CompleteBox", quizPanel.transform, new Vector2(950, 580), Vector2.zero, new Color(0.10f, 0.15f, 0.25f, 0.99f));
        Text completeTitle = CreateText("CompleteTitle", completeBox.transform, new Vector2(850, 60), new Vector2(0, 230), ArabicTextFixer.Fix("أحسنت يا بطل! أنهيت المستوى بنجاح"), 32, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);

        GameObject[] stars = new GameObject[3];
        Sprite goldStarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/star_gold.png");
        for (int i = 0; i < 3; i++)
        {
            float posX = -130 + (i * 130);
            GameObject sGo = CreateUIBox($"Star_{i}", completeBox.transform, new Vector2(85, 85), new Vector2(posX, 125), Color.white);
            if (goldStarSprite != null) sGo.GetComponent<Image>().sprite = goldStarSprite;
            stars[i] = sGo;
        }

        // نص النتيجة المضبوط والمصاغ بدقة
        Text finalScore = CreateText("FinalScore", completeBox.transform, new Vector2(850, 50), new Vector2(0, 20), ArabicTextFixer.Fix("حصلت على 4 من 10 إجابات صحيحة"), 28, Color.white, defaultFont, TextAnchor.MiddleCenter);
        Button retryBtn = CreateButton("RetryBtn", completeBox.transform, new Vector2(440, 70), new Vector2(0, -75), new Color(0.18f, 0.65f, 0.32f), ArabicTextFixer.Fix("أسئلة عشوائية جديدة"), Color.white, 22, defaultFont);
        Button backMapBtn = CreateButton("BackMapBtn", completeBox.transform, new Vector2(440, 70), new Vector2(0, -165), new Color(0.20f, 0.45f, 0.75f), ArabicTextFixer.Fix("العودة لخريطة الأعضاء"), Color.white, 22, defaultFont);
        completeBox.SetActive(false);
        quizPanel.SetActive(false);

        // --- ربط المكونات بالسكربتات ---
        BodyExplorerUI explorerUI = canvasGo.AddComponent<BodyExplorerUI>();
        explorerUI.headerTitleText = titleText;
        explorerUI.headerSubText = subText;
        explorerUI.btnLangArabic = btnAr;
        explorerUI.btnLangEnglish = btnEn;
        explorerUI.btnLangFrench = btnFr;
        explorerUI.stagesModalPanel = stagesModal;
        explorerUI.modalTitleText = modalTitle;
        explorerUI.stageButtons = stageBtns;
        explorerUI.stageButtonTexts = stageBtnTexts;
        explorerUI.stageStarTexts = stageStarTexts;
        explorerUI.stageLockIcons = lockIcons;
        explorerUI.closeModalButton = closeModalBtn;
        explorerUI.bottomOrganButtons = bottomButtons;
        explorerUI.bottomOrganTexts = bottomTexts;

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
        quizCtrl.stageCompleteTitleText = completeTitle;
        quizCtrl.finalScoreText = finalScore;
        quizCtrl.starIcons = stars;
        quizCtrl.closeQuizButton = closeQuizBtn;
        quizCtrl.closeQuizButtonText = closeQuizBtn.GetComponentInChildren<Text>();
        quizCtrl.retryButton = retryBtn;
        quizCtrl.retryButtonText = retryBtn.GetComponentInChildren<Text>();
        quizCtrl.backToMapButton = backMapBtn;
        quizCtrl.backToMapButtonText = backMapBtn.GetComponentInChildren<Text>();

        explorerUI.quizController = quizCtrl;

        // حفظ المشهد
        string scenesDir = "Assets/Scenes";
        if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        Debug.Log("[ProjectAutomator] تم بناء واجهة اللعبة الاحترافية والمشهد بنجاح تام!");
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

        GameObject bg = CreateUIBox("Background", sliderGo.transform, size, Vector2.zero, new Color(0.18f, 0.22f, 0.32f));
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = Vector2.zero;

        GameObject fill = CreateUIBox("Fill", fillArea.transform, Vector2.zero, Vector2.zero, new Color(0.2f, 0.85f, 0.4f));
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

            var col = child.gameObject.GetComponent<MeshCollider>();
            if (col == null) col = child.gameObject.AddComponent<MeshCollider>();
            col.convex = false; // التقاط شعاعي فيزيائي دقيق 100% بكامل تفاصيل النموذج الطبي
        }
    }

    [MenuItem("DigestiveQuest/2. بناء نسخة الحاسوب (Build PC .exe)")]
    public static void BuildWindows()
    {
        SetupGameScene();

        string buildDir = "Builds/Windows";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);
        string exePath = Path.Combine(buildDir, "DigestiveQuest.exe");

        // ضبط إعدادات المشغل لدعم وضع النافذة القياسية القابلة للتكبير والتصغير والإغلاق
        PlayerSettings.defaultIsFullScreen = false;
        PlayerSettings.defaultScreenWidth = 1366;
        PlayerSettings.defaultScreenHeight = 768;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.visibleInBackground = true;
        PlayerSettings.allowFullscreenSwitch = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

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
