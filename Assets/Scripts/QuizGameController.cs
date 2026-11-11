using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// سكربت متحكم واجهة اللعبة والأسئلة (QuizGameController).
/// - إصلاح صياغة عداد الأسئلة ليظهر بدقة: "السؤال 7 من 10".
/// - إضافة زر "السؤال التالي" لتخطي مهلة الـ 3 ثوانٍ فورياً.
/// - إضافة زر إرسال تقرير النتيجة بالبريد الإلكتروني للمعلم أو ولي الأمر.
/// </summary>
public class QuizGameController : MonoBehaviour
{
    [Header("لوحة الامتحان الكاملة")]
    public GameObject quizPanel;

    [Header("عناصر واجهة المستخدم - بطاقة السؤال")]
    public Text questionTextUI;
    public Text progressCounterTextUI;
    public Text stageTitleTextUI;
    public Slider progressBarUI;

    [Header("أزرار الإجابات")]
    public Button[] optionButtons;
    public Text[] optionTexts;

    [Header("نافذة التغذية الراجعة (التشجيع والشرح)")]
    public GameObject feedbackPanel;
    public Text feedbackTitleText;
    public Text feedbackTipText;
    public Button nextQuestionButton;
    public Text nextQuestionButtonText;

    [Header("نافذة إنهاء المرحلة والاحتفال")]
    public GameObject stageCompletePanel;
    public Text stageCompleteTitleText;
    public Text finalScoreText;
    public GameObject[] starIcons;
    public Button sendReportButton;
    public Text sendReportButtonText;
    public Text reportStatusText;

    [Header("أزرار التنقل")]
    public Button closeQuizButton;
    public Text closeQuizButtonText;
    public Button retryButton;
    public Text retryButtonText;
    public Button backToMapButton;
    public Text backToMapButtonText;

    [Header("بيانات الجلسة الحالية")]
    public int currentOrganId = 0;
    public int currentStageId = 1;

    private List<QuestionItem> activeQuestions = new List<QuestionItem>();
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private Coroutine transitionCoroutine = null;

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
        if (nextQuestionButton != null)
        {
            nextQuestionButton.onClick.RemoveAllListeners();
            nextQuestionButton.onClick.AddListener(OnNextQuestionClicked);
        }
        if (sendReportButton != null)
        {
            sendReportButton.onClick.RemoveAllListeners();
            sendReportButton.onClick.AddListener(OnSendReportClicked);
        }

        LocalizationManager.OnLanguageChanged += RefreshCurrentView;
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshCurrentView;
    }

    public void StartNewQuizSession(int organId, int stageId)
    {
        currentOrganId = organId;
        currentStageId = stageId;
        currentQuestionIndex = 0;
        correctAnswersCount = 0;

        if (quizPanel != null) quizPanel.SetActive(true);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (stageCompletePanel != null) stageCompletePanel.SetActive(false);

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

    private void RefreshCurrentView()
    {
        if (quizPanel != null && quizPanel.activeSelf)
        {
            DisplayCurrentQuestion();
        }
    }

    private void DisplayCurrentQuestion()
    {
        AppLanguage lang = LocalizationManager.CurrentLanguage;

        if (stageTitleTextUI != null)
        {
            string organName = LocalizationManager.Get($"organ_{currentOrganId}");
            string stageName = LocalizationManager.Get($"stage_{currentStageId}_name");
            stageTitleTextUI.text = $"{organName} | {stageName}";
        }

        if (closeQuizButtonText != null)
            closeQuizButtonText.text = LocalizationManager.Get("btn_exit_quiz");

        if (nextQuestionButtonText != null)
            nextQuestionButtonText.text = LocalizationManager.Get("btn_next_question");

        if (currentQuestionIndex < activeQuestions.Count)
        {
            QuestionItem q = activeQuestions[currentQuestionIndex];

            if (questionTextUI != null)
                questionTextUI.text = q.GetQuestionText(lang);

            // صياغة عداد الأسئلة بشكل صحيح تماماً: "السؤال 7 من 10"
            if (progressCounterTextUI != null)
            {
                if (lang == AppLanguage.Arabic)
                {
                    string arabicCounter = $"السؤال {currentQuestionIndex + 1} من {activeQuestions.Count}";
                    progressCounterTextUI.text = ArabicTextFixer.Fix(arabicCounter);
                }
                else if (lang == AppLanguage.French)
                {
                    progressCounterTextUI.text = $"Question {currentQuestionIndex + 1} sur {activeQuestions.Count}";
                }
                else
                {
                    progressCounterTextUI.text = $"Question {currentQuestionIndex + 1} of {activeQuestions.Count}";
                }
            }

            if (progressBarUI != null)
                progressBarUI.value = currentQuestionIndex;

            string[] opts = q.GetOptions(lang);
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < opts.Length && !string.IsNullOrEmpty(opts[i]))
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].interactable = true;

                    Image btnImg = optionButtons[i].GetComponent<Image>();
                    if (btnImg != null) btnImg.color = new Color(0.18f, 0.40f, 0.70f);

                    if (i < optionTexts.Length && optionTexts[i] != null)
                    {
                        optionTexts[i].text = opts[i];
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

    public void OnOptionButtonClicked(int selectedOptionIndex)
    {
        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
        }

        AppLanguage lang = LocalizationManager.CurrentLanguage;
        QuestionItem currentQ = activeQuestions[currentQuestionIndex];
        bool isCorrect = (selectedOptionIndex == currentQ.correctOptionIndex);

        if (selectedOptionIndex < optionButtons.Length)
        {
            Image selectedImg = optionButtons[selectedOptionIndex].GetComponent<Image>();
            if (selectedImg != null)
            {
                selectedImg.color = isCorrect ? new Color(0.18f, 0.72f, 0.32f) : new Color(0.88f, 0.25f, 0.2f);
            }
        }

        if (!isCorrect && currentQ.correctOptionIndex < optionButtons.Length)
        {
            Image correctImg = optionButtons[currentQ.correctOptionIndex].GetComponent<Image>();
            if (correctImg != null)
            {
                correctImg.color = new Color(0.18f, 0.72f, 0.32f);
            }
        }

        string[] opts = currentQ.GetOptions(lang);
        string tip = currentQ.GetTip(lang);

        if (isCorrect)
        {
            correctAnswersCount++;
            ShowFeedback(true, LocalizationManager.Get("feedback_correct"), tip);
        }
        else
        {
            string correctAnswerText = (currentQ.correctOptionIndex < opts.Length) ? opts[currentQ.correctOptionIndex] : "";
            string feedbackStr = LocalizationManager.Get("correct_was") + $"({correctAnswerText})\n\n" + tip;
            ShowFeedback(false, LocalizationManager.Get("feedback_wrong"), feedbackStr);
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
                feedbackTitleText.color = isCorrect ? new Color(0.2f, 0.85f, 0.35f) : new Color(1.0f, 0.6f, 0.2f);
            }
            if (feedbackTipText != null) feedbackTipText.text = explanation;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToNextQuestionRoutine());
    }

    /// <summary>
    /// الانتقال الفوري للسؤال التالي عند الضغط على زر "التالي" دون انتظار
    /// </summary>
    public void OnNextQuestionClicked()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    private IEnumerator TransitionToNextQuestionRoutine()
    {
        yield return new WaitForSeconds(3.0f);

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

            if (stageCompleteTitleText != null)
                stageCompleteTitleText.text = LocalizationManager.Get("win_title");

            AppLanguage lang = LocalizationManager.CurrentLanguage;
            if (finalScoreText != null)
            {
                if (lang == AppLanguage.Arabic)
                {
                    string fullArabicSentence = $"حصلت على {correctAnswersCount} من {activeQuestions.Count} إجابات صحيحة";
                    finalScoreText.text = ArabicTextFixer.Fix(fullArabicSentence);
                }
                else if (lang == AppLanguage.French)
                {
                    finalScoreText.text = $"Tu as obtenu {correctAnswersCount} sur {activeQuestions.Count} bonnes réponses !";
                }
                else
                {
                    finalScoreText.text = $"You got {correctAnswersCount} out of {activeQuestions.Count} correct answers!";
                }
            }

            if (retryButtonText != null)
                retryButtonText.text = LocalizationManager.Get("btn_retry");

            if (backToMapButtonText != null)
                backToMapButtonText.text = LocalizationManager.Get("btn_back_map");

            if (sendReportButtonText != null)
                sendReportButtonText.text = LocalizationManager.Get("btn_send_report");

            if (reportStatusText != null)
                reportStatusText.text = "";

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

    public void OnSendReportClicked()
    {
        string organName = LocalizationManager.Get($"organ_{currentOrganId}");
        string stageName = LocalizationManager.Get($"stage_{currentStageId}_name");

        int earnedStars = 1;
        if (correctAnswersCount >= 8) earnedStars = 3;
        else if (correctAnswersCount >= 5) earnedStars = 2;

        if (StudentProfileManager.Instance != null)
        {
            StudentProfileManager.Instance.SendQuizReport(organName, stageName, correctAnswersCount, activeQuestions.Count, earnedStars);
            if (reportStatusText != null)
                reportStatusText.text = LocalizationManager.Get("report_sent_status");
        }
    }

    public void RetryWithNewRandomQuestions()
    {
        StartNewQuizSession(currentOrganId, currentStageId);
    }

    public void CloseQuiz()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (quizPanel != null)
            quizPanel.SetActive(false);
    }
}
