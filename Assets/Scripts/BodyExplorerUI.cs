using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// سكربت واجهة استكشاف الجسم والمراحل (BodyExplorerUI).
/// يدير نافذة اختيار المراحل الثلاث لكل عضو، وأزرار الاختيار السريع في أسفل الشاشة.
/// </summary>
public class BodyExplorerUI : MonoBehaviour
{
    [Header("لوحة اختيار المراحل")]
    public GameObject stagesModalPanel;
    public Text modalTitleText;

    [Header("أزرار المراحل الثلاث")]
    public Button[] stageButtons;
    public GameObject[] stageLockIcons;
    public Text[] stageStarTexts;
    public Button closeModalButton;

    [Header("أزرار اختيار الأعضاء السريعة في أسفل الشاشة")]
    public Button[] bottomOrganButtons;

    [Header("المتحكم بالأسئلة")]
    public QuizGameController quizController;

    private int selectedOrganId = 0;

    private readonly string[] organNames = {
        "الفم والمريء",
        "المعدة",
        "الكبد والبنكرياس والمرارة",
        "الأمعاء الدقيقة",
        "الأمعاء الغليظة والمستقيم"
    };

    private void Start()
    {
        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(false);
        }

        if (closeModalButton != null)
        {
            closeModalButton.onClick.RemoveAllListeners();
            closeModalButton.onClick.AddListener(CloseModal);
        }

        // ربط أزرار الأعضاء السريعة في أسفل الشاشة
        if (bottomOrganButtons != null)
        {
            for (int i = 0; i < bottomOrganButtons.Length; i++)
            {
                if (bottomOrganButtons[i] != null)
                {
                    int organIdx = i;
                    bottomOrganButtons[i].onClick.RemoveAllListeners();
                    bottomOrganButtons[i].onClick.AddListener(() => {
                        string name = (organIdx < organNames.Length) ? organNames[organIdx] : "عضو";
                        OpenStagesModal(organIdx, name);
                    });
                }
            }
        }
    }

    public void OpenStagesModal(int organId, string organName)
    {
        selectedOrganId = organId;

        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(true);
        }

        if (modalTitleText != null)
        {
            modalTitleText.text = $"🌟 رحلة استكشاف: {organName}";
        }

        UpdateStageButtonsUI();
    }

    private void UpdateStageButtonsUI()
    {
        for (int i = 0; i < 3; i++)
        {
            int stageNumber = i + 1;
            bool isUnlocked = GameProgressManager.IsStageUnlocked(selectedOrganId, stageNumber);
            int stars = GameProgressManager.GetStageStars(selectedOrganId, stageNumber);

            if (i < stageButtons.Length && stageButtons[i] != null)
            {
                stageButtons[i].interactable = isUnlocked;

                int stageIndex = stageNumber;
                stageButtons[i].onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    stageButtons[i].onClick.AddListener(() => OnStageSelected(stageIndex));
                }
            }

            if (i < stageLockIcons.Length && stageLockIcons[i] != null)
            {
                stageLockIcons[i].SetActive(!isUnlocked);
            }

            if (i < stageStarTexts.Length && stageStarTexts[i] != null)
            {
                string starString = "";
                for (int s = 0; s < stars; s++) starString += "⭐";
                if (stars == 0) starString = "جديد";

                stageStarTexts[i].text = isUnlocked ? $"التقييم: {starString}" : "مغلقة 🔒";
            }
        }
    }

    public void OnStageSelected(int stageId)
    {
        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(false);
        }

        if (quizController != null)
        {
            quizController.StartNewQuizSession(selectedOrganId, stageId);
        }
    }

    public void CloseModal()
    {
        if (stagesModalPanel != null)
        {
            stagesModalPanel.SetActive(false);
        }
    }
}
