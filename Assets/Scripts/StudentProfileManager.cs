using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// سكربت إدارة ملف وحساب الطالب (StudentProfileManager).
/// - ينشئ حساب الطالب في أول تشغيل للتطبيق (الاسم، الصف/المستوى، بريد المعلم/ولي الأمر).
/// - يحفظ البيانات محلياً ويتذكرها عند إعادة فتح اللعبة.
/// - يوفر زراً لإعادة التعيين والبدء من جديد مع نافذة تحذيرية صريحة ("احذر").
/// - يولد تقارير النتائج الطبية بالبريد الإلكتروني ويرسلها إلى البريد المخصص.
/// </summary>
public class StudentProfileManager : MonoBehaviour
{
    public static StudentProfileManager Instance { get; private set; }

    [Header("نافذة تسجيل وإنشاء الحساب")]
    public GameObject accountModalPanel;
    public Text accountModalTitle;
    public InputField inputStudentName;
    public InputField inputStudentGrade;
    public InputField inputStudentEmail;
    public Button saveAccountButton;
    public Text saveAccountButtonText;

    [Header("عناصر شريط الهيدر الرئيسي")]
    public Text studentNameBadgeText;
    public Button resetProgressButton;
    public Text resetProgressButtonText;

    [Header("نافذة التحذير (احذر: إعادة التعيين)")]
    public GameObject warningModalPanel;
    public Text warningModalTitle;
    public Text warningModalBody;
    public Button confirmResetButton;
    public Text confirmResetButtonText;
    public Button cancelResetButton;
    public Text cancelResetButtonText;

    [Header("البريد الافتراضي لإرسال التقارير")]
    public static string ConfiguredTargetEmail = "digestivequest.results@gmail.com";

    private const string KeyName = "Student_Name";
    private const string KeyGrade = "Student_Grade";
    private const string KeyEmail = "Student_Email";
    private const string KeyRegistered = "Student_IsRegistered";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (saveAccountButton != null)
        {
            saveAccountButton.onClick.RemoveAllListeners();
            saveAccountButton.onClick.AddListener(OnSaveAccountClicked);
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.RemoveAllListeners();
            resetProgressButton.onClick.AddListener(OnResetProgressClicked);
        }

        if (confirmResetButton != null)
        {
            confirmResetButton.onClick.RemoveAllListeners();
            confirmResetButton.onClick.AddListener(OnConfirmResetClicked);
        }

        if (cancelResetButton != null)
        {
            cancelResetButton.onClick.RemoveAllListeners();
            cancelResetButton.onClick.AddListener(OnCancelResetClicked);
        }

        LocalizationManager.OnLanguageChanged += RefreshLocalizedUI;

        CheckRegistrationStatus();
        RefreshLocalizedUI();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshLocalizedUI;
    }

    /// <summary>
    /// فحص حالة تسجيل الطالب عند بدء التطبيق
    /// </summary>
    public void CheckRegistrationStatus()
    {
        bool isRegistered = PlayerPrefs.GetInt(KeyRegistered, 0) == 1;

        if (!isRegistered)
        {
            // أول مرة: إظهار نافذة إنشاء الحساب
            if (accountModalPanel != null) accountModalPanel.SetActive(true);
            if (warningModalPanel != null) warningModalPanel.SetActive(false);
        }
        else
        {
            // مسجل مسبقاً: إخفاء النافذة وتحديث شارة الاسم
            if (accountModalPanel != null) accountModalPanel.SetActive(false);
            if (warningModalPanel != null) warningModalPanel.SetActive(false);
            UpdateStudentBadge();
        }
    }

    /// <summary>
    /// حفظ بيانات حساب الطالب
    /// </summary>
    public void OnSaveAccountClicked()
    {
        string name = (inputStudentName != null && !string.IsNullOrEmpty(inputStudentName.text.Trim()))
            ? inputStudentName.text.Trim()
            : "تلميذ مستكشف";

        string grade = (inputStudentGrade != null && !string.IsNullOrEmpty(inputStudentGrade.text.Trim()))
            ? inputStudentGrade.text.Trim()
            : "المستوى العام";

        string email = (inputStudentEmail != null && !string.IsNullOrEmpty(inputStudentEmail.text.Trim()))
            ? inputStudentEmail.text.Trim()
            : ConfiguredTargetEmail;

        PlayerPrefs.SetString(KeyName, name);
        PlayerPrefs.SetString(KeyGrade, grade);
        PlayerPrefs.SetString(KeyEmail, email);
        PlayerPrefs.SetInt(KeyRegistered, 1);
        PlayerPrefs.Save();

        Debug.Log($"[StudentProfileManager] تم حفظ حساب الطالب: {name} | {grade} | {email}");

        if (accountModalPanel != null) accountModalPanel.SetActive(false);
        UpdateStudentBadge();
    }

    /// <summary>
    /// الضغط على زر إعادة التعيين لإظهار نافذة التحذير "احذر"
    /// </summary>
    public void OnResetProgressClicked()
    {
        if (warningModalPanel != null)
        {
            warningModalPanel.SetActive(true);
            RefreshLocalizedUI();
        }
    }

    /// <summary>
    /// إلغاء إعادة التعيين والعودة للعبة
    /// </summary>
    public void OnCancelResetClicked()
    {
        if (warningModalPanel != null)
            warningModalPanel.SetActive(false);
    }

    /// <summary>
    /// تأكيد الحذف ومسح كافة النتائج والبدء من جديد
    /// </summary>
    public void OnConfirmResetClicked()
    {
        // 1. مسح كافة الدرجات والمراحل
        GameProgressManager.ResetAllProgress();

        // 2. مسح ملف الطالب
        PlayerPrefs.DeleteKey(KeyName);
        PlayerPrefs.DeleteKey(KeyGrade);
        PlayerPrefs.DeleteKey(KeyEmail);
        PlayerPrefs.SetInt(KeyRegistered, 0);
        PlayerPrefs.Save();

        Debug.Log("[StudentProfileManager] تم تأكيد تصفير درجات الطالب والبدء من جديد.");

        if (warningModalPanel != null)
            warningModalPanel.SetActive(false);

        // 3. إعادة إظهار نافذة إنشاء الحساب للبدء كحساب جديد تماماً
        if (inputStudentName != null) inputStudentName.text = "";
        if (inputStudentGrade != null) inputStudentGrade.text = "";
        if (inputStudentEmail != null) inputStudentEmail.text = "";

        if (accountModalPanel != null)
            accountModalPanel.SetActive(true);

        UpdateStudentBadge();

        // تحديث واجهة استكشاف الأعضاء
        BodyExplorerUI explorer = FindFirstObjectByType<BodyExplorerUI>();
        if (explorer != null) explorer.UpdateAllTexts();
    }

    public void UpdateStudentBadge()
    {
        if (studentNameBadgeText != null)
        {
            string studentName = PlayerPrefs.GetString(KeyName, "");
            if (string.IsNullOrEmpty(studentName))
            {
                studentNameBadgeText.text = "";
            }
            else
            {
                AppLanguage lang = LocalizationManager.CurrentLanguage;
                if (lang == AppLanguage.Arabic)
                    studentNameBadgeText.text = ArabicTextFixer.Fix($"الطالب: {studentName}");
                else if (lang == AppLanguage.French)
                    studentNameBadgeText.text = $"Élève : {studentName}";
                else
                    studentNameBadgeText.text = $"Student: {studentName}";
            }
        }
    }

    public void RefreshLocalizedUI()
    {
        UpdateStudentBadge();

        if (accountModalTitle != null) accountModalTitle.text = LocalizationManager.Get("student_profile_title");
        if (saveAccountButtonText != null) saveAccountButtonText.text = LocalizationManager.Get("btn_save_start");
        if (resetProgressButtonText != null) resetProgressButtonText.text = LocalizationManager.Get("btn_reset_account");

        if (warningModalTitle != null) warningModalTitle.text = LocalizationManager.Get("warning_reset_title");
        if (warningModalBody != null) warningModalBody.text = LocalizationManager.Get("warning_reset_body");
        if (confirmResetButtonText != null) confirmResetButtonText.text = LocalizationManager.Get("btn_confirm_reset");
        if (cancelResetButtonText != null) cancelResetButtonText.text = LocalizationManager.Get("btn_cancel");
    }

    /// <summary>
    /// إرسال تقرير نتيجة الطالب إلى البريد الإلكتروني المحدد
    /// </summary>
    public void SendQuizReport(string organName, string stageName, int score, int totalQuestions, int stars)
    {
        string name = PlayerPrefs.GetString(KeyName, "تلميذ مستكشف");
        string grade = PlayerPrefs.GetString(KeyGrade, "المستوى التعليمي");
        string email = PlayerPrefs.GetString(KeyEmail, ConfiguredTargetEmail);
        if (string.IsNullOrEmpty(email)) email = ConfiguredTargetEmail;

        int percentage = (totalQuestions > 0) ? (score * 100 / totalQuestions) : 0;
        string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        string subject = $"[DigestiveQuest] تقرير نتيجة الطالب: {name} - {organName}";

        string body =
            "====================================================\n" +
            "  تقرير نتائج الطالب في لعبة استكشاف الجهاز الهضمي\n" +
            "====================================================\n\n" +
            $"اسم التلميذ / الطالب: {name}\n" +
            $"الصف أو المستوى الدراسي: {grade}\n" +
            $"تاريخ وتوقيت الاختبار: {dateStr}\n\n" +
            "---------------- تفاصيل النتيجة ----------------\n" +
            $"العضو المستكشف: {organName}\n" +
            $"المرحلة والمستوى: {stageName}\n" +
            $"الإجابات الصحيحة: {score} من {totalQuestions}\n" +
            $"النسبة المئوية: {percentage}%\n" +
            $"التقييم بالنجوم: {stars} من 3 نجوم\n" +
            "------------------------------------------------\n\n" +
            "تم إنشاء هذا التقرير آلياً عبر تطبيق Digestive Quest التعليمي.\n" +
            "مشروع التخرج الطبي التفاعلي ثلاثي الأبعاد.";

        // فتح رابط mailto: المتوافق مع ويندوز وأندرويد وكافة برامج البريد
        string mailtoUrl = "mailto:" + Uri.EscapeDataString(email) +
                           "?subject=" + Uri.EscapeDataString(subject) +
                           "&body=" + Uri.EscapeDataString(body);

        Debug.Log($"[StudentProfileManager] فتح تطبيق البريد لإرسال النتيجة إلى: {email}");
        Application.OpenURL(mailtoUrl);
    }
}
