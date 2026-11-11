using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// سكربت واجهة استكشاف الجسم والمراحل (BodyExplorerUI).
/// هذا الكود مسؤول عن:
/// 1. إظهار نافذة منبثقة أنيقة عند لمس أو النقر على أي عضو في الجسم البشري.
/// 2. عرض اسم العضو والمراحل الثلاث (المرحلة 1: تشريح، المرحلة 2: وظيفة، المرحلة 3: صحة وتحدي).
/// 3. إظهار النجوم المحرزة أو أيقونة القفل إذا كانت المرحلة مقفلة بعد.
/// 4. بدء الجولة ونقل الطفل إلى أسئلة المرحلة المختارة.
/// </summary>
public class BodyExplorerUI : MonoBehaviour
{
    [Header("لوحة اختيار المراحل")]
    [Tooltip("النافذة المنبثقة لاختيار المرحلة للعضو المحدد")]
    [SerializeField] private GameObject stagesModalPanel;

    [Tooltip("عنوان النافذة (اسم العضو المختار)")]
    [SerializeField] private TextMeshProUGUI modalTitleText;

    [Header("أزرار المراحل الثلاث")]
    [SerializeField] private Button[] stageButtons; // مصفوفة للأزرار الثلاثة
    [SerializeField] private GameObject[] stageLockIcons; // صور القفل للمراحل المقفلة
    [SerializeField] private TextMeshProUGUI[] stageStarTexts; // نصوص عرض النجوم لكل مرحلة

    [Header("المتحكم باللعبة (QuizGameController)")]
    [SerializeField] private QuizGameController quizController;

    // العضو المختار حالياً
    private int selectedOrganId = 0;

    /// <summary>
    /// دالة البدء Start:
    /// مهمتها: التأكد من إخفاء النافذة المنبثقة عند تشغيل اللعبة في البداية.
    /// </summary>
    private void Start()
    {
        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(false);
        }
    }

    /// <summary>
    /// دالة فتح نافذة المراحل للعضو المختار (OpenStagesModal):
    /// يتم استدعاؤها تلقائياً عندما ينقر الطفل على أي عضو في الجسم ثلاثي الأبعاد.
    /// المدخلات:
    /// - organId: رقم العضو (0 إلى 4).
    /// - organName: اسم العضو بالعربية ليظهر في العنوان.
    /// </summary>
    public void OpenStagesModal(int organId, string organName)
    {
        selectedOrganId = organId;

        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(true);
        }

        if (modalTitleText != null)
        {
            modalTitleText.text = $"رحلة استكشاف: {organName}";
        }

        // تحديث حالة الأزرار الثلاثة (مفتوحة / مقفلة / عدد النجوم)
        UpdateStageButtonsUI();
    }

    /// <summary>
    /// دالة تحديث حالة أزرار المراحل (UpdateStageButtonsUI):
    /// مهمتها: فحص نظام الحفظ لمعرفة أي المراحل مفتوحة، ووضع القفل على المراحل غير المفتوحة بعد.
    /// </summary>
    private void UpdateStageButtonsUI()
    {
        for (int i = 0; i < 3; i++)
        {
            int stageNumber = i + 1; // المرحلة 1 أو 2 أو 3
            bool isUnlocked = GameProgressManager.IsStageUnlocked(selectedOrganId, stageNumber);
            int stars = GameProgressManager.GetStageStars(selectedOrganId, stageNumber);

            if (i < stageButtons.Length && stageButtons[i] != null)
            {
                // تفعيل أو تعطيل النقر على الزر بحسب حالة القفل
                stageButtons[i].interactable = isUnlocked;

                int stageIndex = stageNumber;
                stageButtons[i].onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    stageButtons[i].onClick.AddListener(() => OnStageSelected(stageIndex));
                }
            }

            // إظهار أو إخفاء أيقونة القفل
            if (i < stageLockIcons.Length && stageLockIcons[i] != null)
            {
                stageLockIcons[i].SetActive(!isUnlocked);
            }

            // عرض عدد النجوم المكتسبة سابقاً
            if (i < stageStarTexts.Length && stageStarTexts[i] != null)
            {
                stageStarTexts[i].text = isUnlocked ? $"النجوم: {stars} / 3 ⭐" : "مغلقة 🔒";
            }
        }
    }

    /// <summary>
    /// دالة بدء لعب المرحلة المختارة (OnStageSelected):
    /// مهمتها: إغلاق النافذة المنبثقة وتشغيل جولة الأسئلة العشوائية للعضو والمرحلة المحددة.
    /// </summary>
    /// <param name="stageId">رقم المرحلة (1 إلى 3)</param>
    public void OnStageSelected(int stageId)
    {
        Debug.Log($"[BodyExplorerUI] تم بدء المرحلة {stageId} للعضو {selectedOrganId}");

        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(false);
        }

        if (quizController != null)
        {
            quizController.gameObject.SetActive(true);
            quizController.StartNewQuizSession(selectedOrganId, stageId);
        }
    }

    /// <summary>
    /// دالة إغلاق النافذة المنبثقة (زر الرجوع):
    /// مهمتها: إعادة الطفل لشاشة استكشاف الجسم الكاملة.
    /// </summary>
    public void CloseModal()
    {
        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(false);
        }
    }
}
