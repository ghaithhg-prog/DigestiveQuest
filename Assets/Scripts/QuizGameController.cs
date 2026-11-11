using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// سكربت متحكم واجهة اللعبة والأسئلة (QuizGameController).
/// تم تصميمه باستخدام نظام Unity UI القياسي (UnityEngine.UI.Text و Button) لضمان العمل بنسبة 100%
/// على جميع نسخ الويندوز وهواتف الأندرويد دون الحاجة لأي مكاتب خارجية.
/// </summary>
public class QuizGameController : MonoBehaviour
{
    [Header("لوحة الامتحان الكاملة")]
    public GameObject quizPanel;

    [Header("عناصر واجهة المستخدم - بطاقة السؤال")]
    [Tooltip("النص الذي يظهر فيه السؤال للطفل")]
    public Text questionTextUI;

    [Tooltip("النص الذي يوضح رقم السؤال الحالي (مثلاً: السؤال 3 من 10)")]
    public Text progressCounterTextUI;

    [Tooltip("نص يعرض اسم العضو والمرحلة في أعلى شاشة الأسئلة")]
    public Text stageTitleTextUI;

    [Tooltip("شريط التقدم البصري الذي يمتلئ مع كل سؤال يجيب عليه الطفل")]
    public Slider progressBarUI;

    [Header("أزرار الإجابات")]
    [Tooltip("مصفوفة تحتوي على أزرار الخيارات الأربعة")]
    public Button[] optionButtons;

    [Tooltip("نصوص أزرار الخيارات")]
    public Text[] optionTexts;

    [Header("نافذة التغذية الراجعة (التشجيع والشرح)")]
    [Tooltip("لوحة التغذية الراجعة المنبثقة بعد كل نقرة إجابة")]
    public GameObject feedbackPanel;

    [Tooltip("عنوان الرسالة (أحسنت / حاول مرة أخرى)")]
    public Text feedbackTitleText;

    [Tooltip("نص الشرح العلمي المبسط للطفل")]
    public Text feedbackTipText;

    [Header("نافذة إنهاء المرحلة والاحتفال")]
    [Tooltip("اللوحة الكبيرة التي تظهر عند إنهاء الـ 10 أسئلة")]
    public GameObject stageCompletePanel;

    [Tooltip("نص إجمالي النقاط والنتيجة")]
    public Text finalScoreText;

    [Tooltip("صور النجوم التقديرية (1 إلى 3 نجوم)")]
    public GameObject[] starIcons;

    [Header("أزرار التنقل")]
    public Button closeQuizButton;
    public Button retryButton;
    public Button backToMapButton;

    [Header("بيانات الجلسة الحالية")]
    public int currentOrganId = 0;
    public int currentStageId = 1;

    private List<QuestionItem> activeQuestions = new List<QuestionItem>();
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;

    private readonly string[] organNames = {
        "الفم والمريء",
        "المعدة",
        "الكبد والبنكرياس والمرارة",
        "الأمعاء الدقيقة",
        "الأمعاء الغليظة والمستقيم"
    };

    private void Start()
    {
        if (closeQuizButton != null)
        {
            closeQuizButton.onClick.RemoveAllListeners();
            closeQuizButton.onClick.AddListener(CloseQuiz);
        }
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryWithNewRandomQuestions);
        }
        if (backToMapButton != null)
        {
            backToMapButton.onClick.RemoveAllListeners();
            backToMapButton.onClick.AddListener(CloseQuiz);
        }
    }

    /// <summary>
    /// دالة بدء جولة جديدة (StartNewQuizSession):
    /// تجلب 10 أسئلة عشوائية مختلفة تماماً من البنك المركزي وتعرض الواجهة.
    /// </summary>
    public void StartNewQuizSession(int organId, int stageId)
    {
        currentOrganId = organId;
        currentStageId = stageId;
        currentQuestionIndex = 0;
        correctAnswersCount = 0;

        if (quizPanel != null) quizPanel.SetActive(true);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (stageCompletePanel != null) stageCompletePanel.SetActive(false);

        string organName = (currentOrganId >= 0 && currentOrganId < organNames.Length) ? organNames[currentOrganId] : "الجهاز الهضمي";
        if (stageTitleTextUI != null)
        {
            stageTitleTextUI.text = $"{organName} - المستوى {currentStageId}";
        }

        if (QuestionBankManager.Instance != null)
        {
            activeQuestions = QuestionBankManager.Instance.GetRandomQuestions(currentOrganId, currentStageId, 10);
        }

        if (progressBarUI != null)
        {
            progressBarUI.minValue = 0;
            progressBarUI.maxValue = activeQuestions.Count;
            progressBarUI.value = 0;
        }

        DisplayCurrentQuestion();
    }

    /// <summary>
    /// دالة عرض السؤال الحالي وتوزيع الخيارات على الأزرار
    /// </summary>
    private void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex < activeQuestions.Count)
        {
            QuestionItem q = activeQuestions[currentQuestionIndex];

            if (questionTextUI != null)
                questionTextUI.text = q.questionText;

            if (progressCounterTextUI != null)
                progressCounterTextUI.text = $"السؤال: {currentQuestionIndex + 1} من {activeQuestions.Count}";

            if (progressBarUI != null)
                progressBarUI.value = currentQuestionIndex;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < q.options.Length && !string.IsNullOrEmpty(q.options[i]))
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].interactable = true;
                    
                    // إعادة لون الزر الطبيعي
                    Image btnImg = optionButtons[i].GetComponent<Image>();
                    if (btnImg != null) btnImg.color = new Color(0.2f, 0.45f, 0.75f);

                    if (i < optionTexts.Length && optionTexts[i] != null)
                    {
                        optionTexts[i].text = q.options[i];
                    }

                    int optionIndex = i;
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(optionIndex));
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            OnStageFinished();
        }
    }

    /// <summary>
    /// فحص خيار الطفل وتلوين الأزرار بالأخضر أو الأحمر
    /// </summary>
    public void OnOptionButtonClicked(int selectedOptionIndex)
    {
        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
        }

        QuestionItem currentQ = activeQuestions[currentQuestionIndex];
        bool isCorrect = (selectedOptionIndex == currentQ.correctOptionIndex);

        // تلوين الزر المختار
        if (selectedOptionIndex < optionButtons.Length)
        {
            Image selectedImg = optionButtons[selectedOptionIndex].GetComponent<Image>();
            if (selectedImg != null)
            {
                selectedImg.color = isCorrect ? new Color(0.18f, 0.7f, 0.3f) : new Color(0.85f, 0.25f, 0.2f);
            }
        }

        // تلوين الزر الصحيح بالأخضر إذا أخطأ الطفل
        if (!isCorrect && currentQ.correctOptionIndex < optionButtons.Length)
        {
            Image correctImg = optionButtons[currentQ.correctOptionIndex].GetComponent<Image>();
            if (correctImg != null)
            {
                correctImg.color = new Color(0.18f, 0.7f, 0.3f);
            }
        }

        if (isCorrect)
        {
            correctAnswersCount++;
            ShowFeedback(true, "إجابة رائعة وصحيحة! 🌟", currentQ.educationalTip);
        }
        else
        {
            string correctAnswerText = currentQ.options[currentQ.correctOptionIndex];
            ShowFeedback(false, "حاول ثانية! 💡", $"الإجابة الصحيحة هي: ({correctAnswerText})\n\n{currentQ.educationalTip}");
        }
    }

    private void ShowFeedback(bool isCorrect, string title, string explanation)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
            if (feedbackTitleText != null)
            {
                feedbackTitleText.text = title;
                feedbackTitleText.color = isCorrect ? new Color(0.1f, 0.6f, 0.2f) : new Color(0.8f, 0.3f, 0.1f);
            }
            if (feedbackTipText != null) feedbackTipText.text = explanation;
        }

        StartCoroutine(TransitionToNextQuestionRoutine());
    }

    private IEnumerator TransitionToNextQuestionRoutine()
    {
        yield return new WaitForSeconds(2.8f);

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    private void OnStageFinished()
    {
        if (progressBarUI != null)
            progressBarUI.value = activeQuestions.Count;

        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = $"أحسنت يا بطل! حصلت على {correctAnswersCount} من {activeQuestions.Count} إجابات صحيحة.";

            int earnedStars = 1;
            if (correctAnswersCount >= 8) earnedStars = 3;
            else if (correctAnswersCount >= 5) earnedStars = 2;

            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].SetActive(i < earnedStars);
            }

            GameProgressManager.SaveStageProgress(currentOrganId, currentStageId, earnedStars);
        }
    }

    public void RetryWithNewRandomQuestions()
    {
        StartNewQuizSession(currentOrganId, currentStageId);
    }

    public void CloseQuiz()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);
    }
}
