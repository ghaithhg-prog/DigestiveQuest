using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// سكربت متحكم واجهة اللعبة والأسئلة (QuizGameController).
/// هذا الكود مسؤول عن:
/// 1. ربط واجهة المستخدم (النصوص، الأزرار، شريط التقدم، النوافذ المنبثقة) بمنطق اللعبة.
/// 2. جلب الأسئلة العشوائية الـ 10 من مدير بنك الأسئلة وعرضها واحداً تلو الآخر.
/// 3. فحص إجابة الطفل وتقديم تعزيز إيجابي وتغذية راجعة تعليمية باللغة العربية.
/// 4. حساب النجوم وفتح المراحل التالية تلقائياً عند الفوز.
/// 5. توفير زر "إعادة اللعب" الذي يجلب 10 أسئلة جديدة ومختلفة تماماً في كل مرة!
/// </summary>
public class QuizGameController : MonoBehaviour
{
    [Header("عناصر واجهة المستخدم - بطاقة السؤال")]
    [Tooltip("النص الذي يظهر فيه السؤال للطفل")]
    [SerializeField] private TextMeshProUGUI questionTextUI;

    [Tooltip("النص الذي يوضح رقم السؤال الحالي (مثلاً: السؤال 3 من 10)")]
    [SerializeField] private TextMeshProUGUI progressCounterTextUI;

    [Tooltip("شريط التقدم البصري الذي يمتلئ مع كل سؤال يجيب عليه الطفل")]
    [SerializeField] private Slider progressBarUI;

    [Header("أزرار الإجابات")]
    [Tooltip("مصفوفة تحتوي على أزرار الخيارات الأربعة")]
    [SerializeField] private Button[] optionButtons;

    [Tooltip("نصوص أزرار الخيارات")]
    [SerializeField] private TextMeshProUGUI[] optionTexts;

    [Header("نافذة التغذية الراجعة (التشجيع والشرح)")]
    [Tooltip("لوحة التغذية الراجعة المنبثقة بعد كل نقرة إجابة")]
    [SerializeField] private GameObject feedbackPanel;

    [Tooltip("عنوان الرسالة (أحسنت / حاول مرة أخرى)")]
    [SerializeField] private TextMeshProUGUI feedbackTitleText;

    [Tooltip("نص الشرح العلمي المبسط للطفل")]
    [SerializeField] private TextMeshProUGUI feedbackTipText;

    [Header("نافذة إنهاء المرحلة والاحتفال")]
    [Tooltip("اللوحة الكبيرة التي تظهر عند إنهاء الـ 10 أسئلة")]
    [SerializeField] private GameObject stageCompletePanel;

    [Tooltip("نص إجمالي النقاط والنتيجة")]
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Tooltip("صور النجوم التقديرية (1 إلى 3 نجوم)")]
    [SerializeField] private GameObject[] starIcons;

    [Header("بيانات الجلسة الحالية")]
    [Tooltip("رقم العضو الحالي المختار (0 إلى 4)")]
    public int currentOrganId = 0;

    [Tooltip("رقم المرحلة الحالية (1، 2، أو 3)")]
    public int currentStageId = 1;

    // القائمة التي تحمل الـ 10 أسئلة العشوائية المخصصة لهذه الجولة
    private List<QuestionItem> activeQuestions = new List<QuestionItem>();
    
    // مؤشر رقم السؤال المعروض حالياً (من 0 إلى 9)
    private int currentQuestionIndex = 0;
    
    // عدد الإجابات الصحيحة التي أحرزها الطفل في هذه الجولة
    private int correctAnswersCount = 0;

    /// <summary>
    /// دالة البدء Start:
    /// تُنفذ عند تشغيل المشهد. تقوم ببدء جولة جديدة وتحميل الأسئلة.
    /// </summary>
    private void Start()
    {
        StartNewQuizSession(currentOrganId, currentStageId);
    }

    /// <summary>
    /// دالة بدء جولة جديدة (StartNewQuizSession):
    /// مهمتها: تصفير العدادات وطلب 10 أسئلة عشوائية جديدة من مدير البنك.
    /// إذا أعاد اللاعب اللعب، ستعطيه هذه الدالة أسئلة مختلفة تماماً عن المرة السابقة.
    /// </summary>
    /// <param name="organId">رقم العضو المطلوب</param>
    /// <param name="stageId">رقم المرحلة المطلوبة</param>
    public void StartNewQuizSession(int organId, int stageId)
    {
        currentOrganId = organId;
        currentStageId = stageId;
        currentQuestionIndex = 0;
        correctAnswersCount = 0;

        // إخفاء النوافذ المنبثقة
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (stageCompletePanel != null) stageCompletePanel.SetActive(false);

        // جلب 10 أسئلة عشوائية من البنك المركزي
        if (QuestionBankManager.Instance != null)
        {
            activeQuestions = QuestionBankManager.Instance.GetRandomQuestions(currentOrganId, currentStageId, 10);
        }
        else
        {
            Debug.LogError("[QuizGameController] كائن QuestionBankManager غير موجود في المشهد!");
            return;
        }

        // ضبط شريط التقدم على 10 أسئلة
        if (progressBarUI != null)
        {
            progressBarUI.minValue = 0;
            progressBarUI.maxValue = activeQuestions.Count;
            progressBarUI.value = 0;
        }

        // عرض السؤال الأول
        DisplayCurrentQuestion();
    }

    /// <summary>
    /// دالة عرض السؤال الحالي (DisplayCurrentQuestion):
    /// مهمتها: أخذ بيانات السؤال وتوزيعها على النصوص والأزرار في الشاشة.
    /// </summary>
    private void DisplayCurrentQuestion()
    {
        // التحقق هل ما زال هناك أسئلة باقية في الجولة
        if (currentQuestionIndex < activeQuestions.Count)
        {
            QuestionItem q = activeQuestions[currentQuestionIndex];

            // 1. تحديث نص السؤال
            if (questionTextUI != null)
                questionTextUI.text = q.questionText;

            // 2. تحديث نص العداد البصري
            if (progressCounterTextUI != null)
                progressCounterTextUI.text = $"السؤال: {currentQuestionIndex + 1} من {activeQuestions.Count}";

            // 3. تحديث شريط التقدم
            if (progressBarUI != null)
                progressBarUI.value = currentQuestionIndex;

            // 4. توزيع الخيارات على الأزرار الأربعة
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < q.options.Length && !string.IsNullOrEmpty(q.options[i]))
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].interactable = true; // إعادة تفعيل النقر
                    if (i < optionTexts.Length && optionTexts[i] != null)
                    {
                        optionTexts[i].text = q.options[i];
                    }

                    // ربط زر الخيار بدالة فحص الإجابة عند النقر
                    int optionIndex = i; // نسخ المتغير محلياً للـ Closure
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(optionIndex));
                }
                else
                {
                    // إخفاء الأزرار الزائدة إن كانت خيارات السؤال أقل من 4
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // تم إكمال جميع الـ 10 أسئلة بنجاح!
            OnStageFinished();
        }
    }

    /// <summary>
    /// دالة معالجة نقرة الطفل على أحد الخيارات (OnOptionButtonClicked):
    /// مهمتها: مقارنة الخيار الذي ضغط عليه الطفل بالإجابة الصحيحة وعرض النتيجة.
    /// </summary>
    /// <param name="selectedOptionIndex">رقم الخيار الذي تم الضغط عليه (0 إلى 3)</param>
    public void OnOptionButtonClicked(int selectedOptionIndex)
    {
        // تعطيل جميع الأزرار مؤقتاً لمنع الطفل من الضغط مرتين بسرعة
        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
        }

        QuestionItem currentQ = activeQuestions[currentQuestionIndex];
        bool isCorrect = (selectedOptionIndex == currentQ.correctOptionIndex);

        if (isCorrect)
        {
            correctAnswersCount++;
            ShowFeedback(true, "إجابة رائعة وصحيحة! 🌟", currentQ.educationalTip);
        }
        else
        {
            string correctAnswerText = currentQ.options[currentQ.correctOptionIndex];
            ShowFeedback(false, "إجابة غير صحيحة، حاول التركيز! 💡", $"الإجابة الصحيحة هي: ({correctAnswerText}).\n{currentQ.educationalTip}");
        }
    }

    /// <summary>
    /// دالة إظهار نافذة التغذية الراجعة والتشجيع (ShowFeedback):
    /// مهمتها: إظهار النافذة المنبثقة بالشرح العلمي، ثم الانتظار لثانيتين والانتقال للسؤال التالي.
    /// </summary>
    private void ShowFeedback(bool isCorrect, string title, string explanation)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
            if (feedbackTitleText != null) feedbackTitleText.text = title;
            if (feedbackTipText != null) feedbackTipText.text = explanation;
        }

        // بدء العد التنازلي للانتقال للسؤال التالي تلقائياً
        StartCoroutine(TransitionToNextQuestionRoutine());
    }

    /// <summary>
    /// كوروتين الانتقال للسؤال التالي (TransitionToNextQuestionRoutine):
    /// مهمته: الانتظار ثانيتين ونصف حتى يقرأ الطفل المعلومة، ثم إخفاء اللوحة والانتقال للسؤال التالي.
    /// تم استخدام WaitForSeconds لضمان عدم تجميد أو تعليق اللعبة.
    /// </summary>
    private IEnumerator TransitionToNextQuestionRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    /// <summary>
    /// دالة انتهاء المرحلة (OnStageFinished):
    /// مهمتها: إظهار شاشة الفوز، حساب النجوم بحسب عدد الإجابات الصحيحة، وحفظ التقدم لفتح المرحلة التالية.
    /// </summary>
    private void OnStageFinished()
    {
        if (progressBarUI != null)
            progressBarUI.value = activeQuestions.Count;

        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(true);

            // عرض نتيجة الطفل
            if (finalScoreText != null)
                finalScoreText.text = $"أحسنت! حصلت على {correctAnswersCount} من {activeQuestions.Count} إجابات صحيحة.";

            // حساب عدد النجوم:
            // 9-10 إجابات = 3 نجوم
            // 6-8 إجابات = نجمتان
            // 1-5 إجابات = نجمة واحدة
            int earnedStars = 1;
            if (correctAnswersCount >= 9) earnedStars = 3;
            else if (correctAnswersCount >= 6) earnedStars = 2;

            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].SetActive(i < earnedStars);
            }

            // حفظ النجوم وفتح المرحلة التالية عبر نظام الحفظ
            GameProgressManager.SaveStageProgress(currentOrganId, currentStageId, earnedStars);
        }
    }

    /// <summary>
    /// دالة إعادة لعب المرحلة بأسئلة عشوائية جديدة تماماً:
    /// يتم ربطها بزر "إعادة المحاولة / أسئلة جديدة" في واجهة الفوز.
    /// </summary>
    public void RetryWithNewRandomQuestions()
    {
        StartNewQuizSession(currentOrganId, currentStageId);
    }
}
