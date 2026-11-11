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
        systemsGo.AddComponent<WindowController>();

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

        // --- أ. الشريط العلوي النظيف والأنيق ---
        GameObject headerPanel = CreateUIBox("HeaderPanel", canvasGo.transform, new Vector2(1860, 90), new Vector2(0, 485), new Color(0.09f, 0.13f, 0.22f, 0.95f));

        // زر تصفير الحساب والبدء من جديد (أقصى اليسار)
        Button resetProgressBtn = CreateButton("ResetProgressBtn", headerPanel.transform, new Vector2(170, 46), new Vector2(-790, 0), new Color(0.75f, 0.22f, 0.22f), ArabicTextFixer.Fix("تصفير الحساب"), Color.white, 18, defaultFont);

        // شارة اسم الطالب (بجانب زر التصفير)
        Text studentBadgeText = CreateText("StudentBadge", headerPanel.transform, new Vector2(300, 40), new Vector2(-540, 0), "", 20, new Color(1.0f, 0.88f, 0.4f), defaultFont, TextAnchor.MiddleRight);

        // العنوان والعنوان الفرعي (في المنتصف)
        Text titleText = CreateText("TitleText", headerPanel.transform, new Vector2(760, 44), new Vector2(30, 16), ArabicTextFixer.Fix("مغامرة استكشاف الجهاز الهضمي"), 30, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);
        Text subText = CreateText("SubText", headerPanel.transform, new Vector2(760, 30), new Vector2(30, -22), ArabicTextFixer.Fix("انقر على أي عضو في المجسم أو اختر من البطاقات الجانبية لبدء التحدي!"), 19, new Color(0.85f, 0.92f, 1.0f), defaultFont, TextAnchor.MiddleCenter);

        // أزرار اللغات في أقصى اليمين
        GameObject langContainer = CreateUIBox("LanguageBar", headerPanel.transform, new Vector2(360, 54), new Vector2(680, 0), new Color(0.14f, 0.20f, 0.32f, 0.85f));
        Button btnAr = CreateButton("Btn_Arabic", langContainer.transform, new Vector2(110, 44), new Vector2(-115, 0), new Color(0.18f, 0.55f, 0.32f), ArabicTextFixer.Fix("العربية"), Color.white, 20, defaultFont);
        Button btnEn = CreateButton("Btn_English", langContainer.transform, new Vector2(110, 44), new Vector2(0, 0), new Color(0.20f, 0.40f, 0.65f), "English", Color.white, 20, defaultFont);
        Button btnFr = CreateButton("Btn_French", langContainer.transform, new Vector2(110, 44), new Vector2(115, 0), new Color(0.65f, 0.35f, 0.20f), "Français", Color.white, 20, defaultFont);

        // --- ب. البطاقات الجانبية للأعضاء ---
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
            float posY = 130 - ((i - 3) * 140);
            GameObject cardGo = CreateUIBox($"OrganCard_{i}", canvasGo.transform, new Vector2(340, 120), new Vector2(750, posY), new Color(0.10f, 0.15f, 0.24f, 0.94f));
            CreateUIBox("Accent", cardGo.transform, new Vector2(10, 120), new Vector2(165, 0), accentColors[i]);

            bottomButtons[i] = CreateButton($"OrganBtn_{i}", cardGo.transform, new Vector2(310, 110), new Vector2(-10, 0), new Color(0, 0, 0, 0.01f), "", Color.white, 20, defaultFont);
            bottomTexts[i] = CreateText($"Label_{i}", cardGo.transform, new Vector2(290, 40), new Vector2(-15, 20), ArabicTextFixer.Fix(rawLabels[i]), 22, Color.white, defaultFont, TextAnchor.MiddleRight);
            CreateText($"Sub_{i}", cardGo.transform, new Vector2(290, 30), new Vector2(-15, -20), ArabicTextFixer.Fix("3 مستويات استكشافية | 30 سؤالاً"), 16, new Color(0.7f, 0.82f, 0.95f), defaultFont, TextAnchor.MiddleRight);
        }

        // بطاقة الإرشاد التفاعلي ثلاثي الأبعاد
        GameObject tipCardGo = CreateUIBox("OrganCard_Tip", canvasGo.transform, new Vector2(340, 120), new Vector2(750, -150), new Color(0.08f, 0.12f, 0.20f, 0.94f));
        CreateUIBox("AccentTip", tipCardGo.transform, new Vector2(10, 120), new Vector2(165, 0), new Color(0.2f, 0.7f, 0.9f));
        CreateText("TipTitle", tipCardGo.transform, new Vector2(290, 40), new Vector2(-15, 20), ArabicTextFixer.Fix("استكشاف تفاعلي 360"), 20, new Color(0.3f, 0.85f, 1.0f), defaultFont, TextAnchor.MiddleRight);
        CreateText("TipBody", tipCardGo.transform, new Vector2(290, 50), new Vector2(-15, -20), ArabicTextFixer.Fix("اسحب بالفأرة لتدوير الجسم في أي اتجاه، وانقر مباشرة على أي عضو لفتحه!"), 15, new Color(0.8f, 0.88f, 0.98f), defaultFont, TextAnchor.MiddleRight);

        // --- ج. نافذة اختيار المراحل (Stages Modal) ---
        GameObject stagesModal = CreateUIBox("StagesModalPanel", canvasGo.transform, new Vector2(1920, 1080), Vector2.zero, new Color(0, 0, 0, 0.70f));
        GameObject modalCard = CreateUIBox("ModalCard", stagesModal.transform, new Vector2(900, 620), Vector2.zero, new Color(0.10f, 0.15f, 0.25f, 0.98f));
        Text modalTitle = CreateText("ModalTitle", modalCard.transform, new Vector2(800, 60), new Vector2(0, 240), ArabicTextFixer.Fix("رحلة استكشاف العضو"), 32, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);

        Button[] stageBtns = new Button[3];
        Text[] stageBtnTexts = new Text[3];
        GameObject[] lockIcons = new GameObject[3];
        Text[] stageStarTexts = new Text[3];

        for (int i = 0; i < 3; i++)
        {
            float posY = 110 - (i * 110);
            stageBtns[i] = CreateButton($"StageBtn_{i+1}", modalCard.transform, new Vector2(800, 85), new Vector2(0, posY), new Color(0.18f, 0.65f, 0.32f), "", Color.white, 24, defaultFont);
            stageBtnTexts[i] = stageBtns[i].GetComponentInChildren<Text>();

            stageStarTexts[i] = CreateText($"StarText_{i}", stageBtns[i].transform, new Vector2(220, 40), new Vector2(270, 0), "", 22, Color.yellow, defaultFont, TextAnchor.MiddleRight);

            GameObject lockObj = CreateUIBox($"Lock_{i}", stageBtns[i].transform, new Vector2(40, 40), new Vector2(-360, 0), Color.white);
            lockObj.SetActive(false);
            lockIcons[i] = lockObj;
        }

        Button closeModalBtn = CreateButton("CloseModalBtn", modalCard.transform, new Vector2(280, 60), new Vector2(0, -250), new Color(0.45f, 0.45f, 0.52f), ArabicTextFixer.Fix("رجوع للجسم"), Color.white, 22, defaultFont);
        stagesModal.SetActive(false);

        // --- د. نافذة الأسئلة الكاملة (Quiz Screen) ---
        GameObject quizPanel = CreateUIBox("QuizGamePanel", canvasGo.transform, new Vector2(1920, 1080), Vector2.zero, new Color(0.06f, 0.09f, 0.15f, 0.99f));

        // شريط رأس الامتحان (QuizTopBar): توزيع عناصر خالي تماماً من التداخل مع مسافات آمنة جداً
        GameObject quizTopBar = CreateUIBox("QuizTopBar", quizPanel.transform, new Vector2(1750, 80), new Vector2(0, 485), new Color(0.11f, 0.16f, 0.26f));

        // 1. عنوان المرحلة (أقصى اليسار: -810 إلى -270)
        Text stageTitleText = CreateText("StageTitleText", quizTopBar.transform, new Vector2(540, 45), new Vector2(-540, 0), ArabicTextFixer.Fix("المعدة - المستوى 1"), 24, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleLeft);

        // 2. شريط التقدم الأخضر (الوسط: -120 إلى +240) - مسافة فاصلة 150 بكسل عن العنوان لمنع أي تداخل
        Slider progressBar = CreateSimpleSlider("ProgressBar", quizTopBar.transform, new Vector2(360, 22), new Vector2(60, 0));

        // 3. عداد الأسئلة (بجانب شريط التقدم: +280 إلى +520)
        Text progressCounterText = CreateText("ProgressCounterText", quizTopBar.transform, new Vector2(240, 45), new Vector2(400, 0), ArabicTextFixer.Fix("السؤال 1 من 10"), 22, Color.white, defaultFont, TextAnchor.MiddleCenter);

        // 4. زر خروج (أقصى اليمين: +650 إلى +810)
        Button closeQuizBtn = CreateButton("CloseQuizBtn", quizTopBar.transform, new Vector2(150, 52), new Vector2(730, 0), new Color(0.85f, 0.22f, 0.22f), ArabicTextFixer.Fix("خروج"), Color.white, 22, defaultFont);

        // بطاقة السؤال
        GameObject questionCard = CreateUIBox("QuestionCard", quizPanel.transform, new Vector2(1400, 230), new Vector2(0, 240), new Color(0.13f, 0.19f, 0.30f));
        Text questionText = CreateText("QuestionText", questionCard.transform, new Vector2(1300, 190), Vector2.zero, ArabicTextFixer.Fix("أين تبدأ أولى خطوات رحلة هضم الطعام في جسم الإنسان؟"), 32, Color.white, defaultFont, TextAnchor.MiddleCenter);

        // خيارات الإجابة الأربعة
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

        // نافذة التغذية الراجعة (مع زر "السؤال التالي" لتجاوز الانتظار فوراً)
        GameObject feedbackBox = CreateUIBox("FeedbackBox", quizPanel.transform, new Vector2(1000, 340), new Vector2(0, -220), new Color(0.10f, 0.14f, 0.22f, 0.99f));
        Text feedbackTitle = CreateText("FeedbackTitle", feedbackBox.transform, new Vector2(950, 50), new Vector2(0, 125), ArabicTextFixer.Fix("إجابة ممتازة وصحيحة!"), 30, Color.green, defaultFont, TextAnchor.MiddleCenter);
        Text feedbackTip = CreateText("FeedbackTip", feedbackBox.transform, new Vector2(950, 140), new Vector2(0, 20), "", 24, Color.white, defaultFont, TextAnchor.MiddleCenter);
        Button nextQuestionBtn = CreateButton("NextQuestionBtn", feedbackBox.transform, new Vector2(260, 52), new Vector2(0, -100), new Color(0.18f, 0.65f, 0.32f), ArabicTextFixer.Fix("السؤال التالي ➔"), Color.white, 22, defaultFont);
        feedbackBox.SetActive(false);

        // نافذة انتهاء المرحلة والاحتفال (مع إمكانية إرسال التقرير بالبريد)
        GameObject completeBox = CreateUIBox("CompleteBox", quizPanel.transform, new Vector2(960, 680), Vector2.zero, new Color(0.10f, 0.15f, 0.25f, 0.99f));
        Text completeTitle = CreateText("CompleteTitle", completeBox.transform, new Vector2(850, 60), new Vector2(0, 280), ArabicTextFixer.Fix("أحسنت يا بطل! أنهيت المستوى بنجاح"), 32, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);

        GameObject[] stars = new GameObject[3];
        Sprite goldStarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/star_gold.png");
        for (int i = 0; i < 3; i++)
        {
            float posX = -130 + (i * 130);
            GameObject sGo = CreateUIBox($"Star_{i}", completeBox.transform, new Vector2(85, 85), new Vector2(posX, 195), Color.white);
            if (goldStarSprite != null) sGo.GetComponent<Image>().sprite = goldStarSprite;
            stars[i] = sGo;
        }

        // نص النتيجة النهائي
        Text finalScore = CreateText("FinalScore", completeBox.transform, new Vector2(850, 50), new Vector2(0, 110), ArabicTextFixer.Fix("حصلت على 4 من 10 إجابات صحيحة"), 28, Color.white, defaultFont, TextAnchor.MiddleCenter);

        // زر إرسال تقرير النتيجة بالبريد
        Button sendReportBtn = CreateButton("SendReportBtn", completeBox.transform, new Vector2(460, 65), new Vector2(0, 20), new Color(0.28f, 0.25f, 0.65f), ArabicTextFixer.Fix("إرسال التقرير بالبريد ✉"), Color.white, 22, defaultFont);
        Text reportStatusText = CreateText("ReportStatusText", completeBox.transform, new Vector2(600, 35), new Vector2(0, -35), "", 20, new Color(0.25f, 0.90f, 0.85f), defaultFont, TextAnchor.MiddleCenter);

        Button retryBtn = CreateButton("RetryBtn", completeBox.transform, new Vector2(460, 65), new Vector2(0, -110), new Color(0.18f, 0.65f, 0.32f), ArabicTextFixer.Fix("أسئلة عشوائية جديدة"), Color.white, 22, defaultFont);
        Button backMapBtn = CreateButton("BackMapBtn", completeBox.transform, new Vector2(460, 65), new Vector2(0, -195), new Color(0.20f, 0.45f, 0.75f), ArabicTextFixer.Fix("العودة لخريطة الأعضاء"), Color.white, 22, defaultFont);
        completeBox.SetActive(false);
        quizPanel.SetActive(false);

        // --- هـ. نافذة تسجيل وإنشاء حساب التلميذ لأول مرة (Student Account Modal) ---
        GameObject accountModal = CreateUIBox("StudentAccountModal", canvasGo.transform, new Vector2(840, 600), Vector2.zero, new Color(0.08f, 0.12f, 0.22f, 0.99f));
        CreateUIBox("AccentHeader", accountModal.transform, new Vector2(840, 8), new Vector2(0, 296), new Color(0.2f, 0.65f, 0.9f));
        Text accountModalTitle = CreateText("AccountModalTitle", accountModal.transform, new Vector2(760, 50), new Vector2(0, 235), ArabicTextFixer.Fix("إنشاء ملف التلميذ / الطالب"), 32, new Color(1.0f, 0.85f, 0.2f), defaultFont, TextAnchor.MiddleCenter);
        CreateText("AccountModalSub", accountModal.transform, new Vector2(760, 35), new Vector2(0, 190), ArabicTextFixer.Fix("سجل بياناتك لحفظ نتائجك ومستوياتك وإرسال التقارير"), 20, new Color(0.8f, 0.88f, 0.98f), defaultFont, TextAnchor.MiddleCenter);

        CreateText("Lbl_Name", accountModal.transform, new Vector2(700, 30), new Vector2(0, 140), ArabicTextFixer.Fix("اسم التلميذ / الطالب:"), 22, Color.white, defaultFont, TextAnchor.MiddleRight);
        InputField inputName = CreateInputField("Input_Name", accountModal.transform, new Vector2(700, 50), new Vector2(0, 95), ArabicTextFixer.Fix("أدخل اسم التلميذ هنا..."), defaultFont, 22);

        CreateText("Lbl_Grade", accountModal.transform, new Vector2(700, 30), new Vector2(0, 45), ArabicTextFixer.Fix("الصف أو المستوى الدراسي:"), 22, Color.white, defaultFont, TextAnchor.MiddleRight);
        InputField inputGrade = CreateInputField("Input_Grade", accountModal.transform, new Vector2(700, 50), new Vector2(0, 0), ArabicTextFixer.Fix("مثال: السنة الرابعة متوسط / ثانوية..."), defaultFont, 22);

        CreateText("Lbl_Email", accountModal.transform, new Vector2(700, 30), new Vector2(0, -50), ArabicTextFixer.Fix("البريد الإلكتروني للمعلم أو ولي الأمر (لإرسال النتائج):"), 22, Color.white, defaultFont, TextAnchor.MiddleRight);
        InputField inputEmail = CreateInputField("Input_Email", accountModal.transform, new Vector2(700, 50), new Vector2(0, -95), "teacher.parent@example.com", defaultFont, 22);

        Button saveAccountBtn = CreateButton("Btn_SaveAccount", accountModal.transform, new Vector2(400, 65), new Vector2(0, -195), new Color(0.18f, 0.65f, 0.32f), ArabicTextFixer.Fix("حفظ وبدء المغامرة ➔"), Color.white, 24, defaultFont);
        accountModal.SetActive(false);

        // --- و. نافذة التحذير الصريح (Warning Modal: احذر عند تصفير الحساب) ---
        GameObject warningModal = CreateUIBox("WarningModal", canvasGo.transform, new Vector2(780, 440), Vector2.zero, new Color(0.15f, 0.08f, 0.10f, 0.99f));
        CreateUIBox("AccentDanger", warningModal.transform, new Vector2(780, 8), new Vector2(0, 216), new Color(0.9f, 0.2f, 0.2f));
        Text warningTitle = CreateText("WarningTitle", warningModal.transform, new Vector2(700, 50), new Vector2(0, 135), ArabicTextFixer.Fix("تحذير واحتراس!"), 32, new Color(1.0f, 0.35f, 0.25f), defaultFont, TextAnchor.MiddleCenter);
        Text warningBody = CreateText("WarningBody", warningModal.transform, new Vector2(700, 140), new Vector2(0, 30), ArabicTextFixer.Fix("احذر: هل أنت متأكد من رغبتك في مسح كافة نتائج وتاريخ الطالب والبدء من جديد كحساب جديد؟ لن يمكنك استرجاع الدرجات السابقة!"), 23, Color.white, defaultFont, TextAnchor.MiddleCenter);

        Button confirmResetBtn = CreateButton("Btn_ConfirmReset", warningModal.transform, new Vector2(320, 60), new Vector2(-180, -120), new Color(0.85f, 0.22f, 0.22f), ArabicTextFixer.Fix("نعم، امسح وابدأ من جديد"), Color.white, 22, defaultFont);
        Button cancelResetBtn = CreateButton("Btn_CancelReset", warningModal.transform, new Vector2(320, 60), new Vector2(180, -120), new Color(0.25f, 0.35f, 0.50f), ArabicTextFixer.Fix("إلغاء"), Color.white, 22, defaultFont);
        warningModal.SetActive(false);

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
        quizCtrl.nextQuestionButton = nextQuestionBtn;
        quizCtrl.nextQuestionButtonText = nextQuestionBtn.GetComponentInChildren<Text>();

        quizCtrl.stageCompletePanel = completeBox;
        quizCtrl.stageCompleteTitleText = completeTitle;
        quizCtrl.finalScoreText = finalScore;
        quizCtrl.starIcons = stars;
        quizCtrl.sendReportButton = sendReportBtn;
        quizCtrl.sendReportButtonText = sendReportBtn.GetComponentInChildren<Text>();
        quizCtrl.reportStatusText = reportStatusText;

        quizCtrl.closeQuizButton = closeQuizBtn;
        quizCtrl.closeQuizButtonText = closeQuizBtn.GetComponentInChildren<Text>();
        quizCtrl.retryButton = retryBtn;
        quizCtrl.retryButtonText = retryBtn.GetComponentInChildren<Text>();
        quizCtrl.backToMapButton = backMapBtn;
        quizCtrl.backToMapButtonText = backMapBtn.GetComponentInChildren<Text>();

        explorerUI.quizController = quizCtrl;

        // ربط مدير حساب وملف الطالب
        StudentProfileManager profileMgr = canvasGo.AddComponent<StudentProfileManager>();
        profileMgr.accountModalPanel = accountModal;
        profileMgr.accountModalTitle = accountModalTitle;
        profileMgr.inputStudentName = inputName;
        profileMgr.inputStudentGrade = inputGrade;
        profileMgr.inputStudentEmail = inputEmail;
        profileMgr.saveAccountButton = saveAccountBtn;
        profileMgr.saveAccountButtonText = saveAccountBtn.GetComponentInChildren<Text>();
        profileMgr.studentNameBadgeText = studentBadgeText;
        profileMgr.resetProgressButton = resetProgressBtn;
        profileMgr.resetProgressButtonText = resetProgressBtn.GetComponentInChildren<Text>();
        profileMgr.warningModalPanel = warningModal;
        profileMgr.warningModalTitle = warningTitle;
        profileMgr.warningModalBody = warningBody;
        profileMgr.confirmResetButton = confirmResetBtn;
        profileMgr.confirmResetButtonText = confirmResetBtn.GetComponentInChildren<Text>();
        profileMgr.cancelResetButton = cancelResetBtn;
        profileMgr.cancelResetButtonText = cancelResetBtn.GetComponentInChildren<Text>();

        // حفظ المشهد
        string scenesDir = "Assets/Scenes";
        if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        Debug.Log("[ProjectAutomator] تم بناء وتجهيز وحفظ المشهد بالكامل بنجاح!");
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

    private static InputField CreateInputField(string name, Transform parent, Vector2 size, Vector2 anchoredPos, string placeholderStr, Font font, int fontSize = 22)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);

        // نص الإدخال
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-24, -10);
        textRt.anchoredPosition = Vector2.zero;
        Text t = textGo.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.supportRichText = false;

        // النص الإرشادي التوضيحي (Placeholder)
        GameObject phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(go.transform, false);
        RectTransform phRt = phGo.AddComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.sizeDelta = new Vector2(-24, -10);
        phRt.anchoredPosition = Vector2.zero;
        Text ph = phGo.AddComponent<Text>();
        ph.font = font;
        ph.fontSize = fontSize;
        ph.color = new Color(0.65f, 0.72f, 0.85f, 0.55f);
        ph.alignment = TextAnchor.MiddleCenter;
        ph.text = placeholderStr;

        InputField input = go.AddComponent<InputField>();
        input.textComponent = t;
        input.placeholder = ph;
        input.lineType = InputField.LineType.SingleLine;

        return input;
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
