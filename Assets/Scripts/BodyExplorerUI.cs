using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// سكربت واجهة استكشاف الجسم والمراحل (BodyExplorerUI) مع دعم اللغات الثلاث.
/// </summary>
public class BodyExplorerUI : MonoBehaviour
{
    [Header("لوحة العنوان العلوية")]
    public Text headerTitleText;
    public Text headerSubText;

    [Header("أزرار اللغات في الزاوية العلوية")]
    public Button btnLangArabic;
    public Button btnLangEnglish;
    public Button btnLangFrench;

    [Header("لوحة اختيار المراحل")]
    public GameObject stagesModalPanel;
    public Text modalTitleText;

    [Header("أزرار المراحل الثلاث")]
    public Button[] stageButtons;
    public Text[] stageButtonTexts;
    public GameObject[] stageLockIcons;
    public Text[] stageStarTexts;
    public Button closeModalButton;

    [Header("أزرار اختيار الأعضاء السريعة في أسفل الشاشة")]
    public Button[] bottomOrganButtons;
    public Text[] bottomOrganTexts;

    [Header("المتحكم بالأسئلة")]
    public QuizGameController quizController;

    private int selectedOrganId = 0;

    private void Start()
    {
        if (stagesModalPanel != null) stagesModalPanel.SetActive(false);

        if (closeModalButton != null)
        {
            closeModalButton.onClick.RemoveAllListeners();
            closeModalButton.onClick.AddListener(CloseModal);
        }

        // أزرار تبديل اللغة (عربي، إنجليزي، فرنسي)
        if (btnLangArabic != null)
        {
            btnLangArabic.onClick.RemoveAllListeners();
            btnLangArabic.onClick.AddListener(() => LocalizationManager.SetLanguage(AppLanguage.Arabic));
        }
        if (btnLangEnglish != null)
        {
            btnLangEnglish.onClick.RemoveAllListeners();
            btnLangEnglish.onClick.AddListener(() => LocalizationManager.SetLanguage(AppLanguage.English));
        }
        if (btnLangFrench != null)
        {
            btnLangFrench.onClick.RemoveAllListeners();
            btnLangFrench.onClick.AddListener(() => LocalizationManager.SetLanguage(AppLanguage.French));
        }

        // ربط أزرار الأعضاء السريعة
        if (bottomOrganButtons != null)
        {
            for (int i = 0; i < bottomOrganButtons.Length; i++)
            {
                if (bottomOrganButtons[i] != null)
                {
                    int organIdx = i;
                    bottomOrganButtons[i].onClick.RemoveAllListeners();
                    bottomOrganButtons[i].onClick.AddListener(() => OpenStagesModal(organIdx));
                }
            }
        }

        LocalizationManager.OnLanguageChanged += UpdateAllTexts;
        UpdateAllTexts();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateAllTexts;
    }

    public void UpdateAllTexts()
    {
        if (headerTitleText != null) headerTitleText.text = LocalizationManager.Get("app_title");
        if (headerSubText != null) headerSubText.text = LocalizationManager.Get("app_subtitle");

        if (bottomOrganTexts != null)
        {
            for (int i = 0; i < bottomOrganTexts.Length; i++)
            {
                if (bottomOrganTexts[i] != null)
                    bottomOrganTexts[i].text = LocalizationManager.Get($"organ_{i}");
            }
        }

        if (closeModalButton != null)
        {
            Text closeTxt = closeModalButton.GetComponentInChildren<Text>();
            if (closeTxt != null) closeTxt.text = LocalizationManager.Get("btn_close_modal");
        }

        if (stagesModalPanel != null && stagesModalPanel.activeSelf)
        {
            UpdateModalContent();
        }
    }

    public void OpenStagesModal(int organId, string organName = "")
    {
        selectedOrganId = organId;
        if (stagesModalPanel != null) stagesModalPanel.SetActive(true);
        UpdateModalContent();
    }

    private void UpdateModalContent()
    {
        string organName = LocalizationManager.Get($"organ_{selectedOrganId}");
        if (modalTitleText != null)
        {
            modalTitleText.text = LocalizationManager.Get("modal_title_prefix") + organName;
        }

        for (int i = 0; i < 3; i++)
        {
            int stageNumber = i + 1;
            bool isUnlocked = GameProgressManager.IsStageUnlocked(selectedOrganId, stageNumber);
            int stars = GameProgressManager.GetStageStars(selectedOrganId, stageNumber);

            if (i < stageButtons.Length && stageButtons[i] != null)
            {
                stageButtons[i].interactable = isUnlocked;

                // تحديث لون الزر حسب القفل
                Image btnImg = stageButtons[i].GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = isUnlocked ? GetStageColor(i) : new Color(0.3f, 0.35f, 0.4f, 0.8f);
                }

                int stageIdx = stageNumber;
                stageButtons[i].onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    stageButtons[i].onClick.AddListener(() => OnStageSelected(stageIdx));
                }
            }

            if (i < stageButtonTexts.Length && stageButtonTexts[i] != null)
            {
                stageButtonTexts[i].text = LocalizationManager.Get($"stage_{stageNumber}_name");
            }

            if (i < stageLockIcons.Length && stageLockIcons[i] != null)
            {
                stageLockIcons[i].SetActive(!isUnlocked);
            }

            if (i < stageStarTexts.Length && stageStarTexts[i] != null)
            {
                if (!isUnlocked)
                {
                    stageStarTexts[i].text = LocalizationManager.Get("status_locked");
                }
                else if (stars == 0)
                {
                    stageStarTexts[i].text = LocalizationManager.Get("status_new");
                }
                else
                {
                    stageStarTexts[i].text = LocalizationManager.Get("status_unlocked") + $"{stars}/3";
                }
            }
        }
    }

    private Color GetStageColor(int stageIndex)
    {
        switch (stageIndex)
        {
            case 0: return new Color(0.18f, 0.65f, 0.32f); // أخضر أنيق
            case 1: return new Color(0.18f, 0.50f, 0.85f); // أزرق مريح
            case 2: return new Color(0.88f, 0.45f, 0.15f); // برتقالي ذهبي
            default: return Color.gray;
        }
    }

    public void OnStageSelected(int stageId)
    {
        if (stagesModalPanel != null) stagesModalPanel.SetActive(false);
        if (quizController != null)
        {
            quizController.StartNewQuizSession(selectedOrganId, stageId);
        }
    }

    public void CloseModal()
    {
        if (stagesModalPanel != null) stagesModalPanel.SetActive(false);
    }
}
