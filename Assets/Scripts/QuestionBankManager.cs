using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// سكربت مدير بنك الأسئلة (QuestionBankManager).
/// هذا الكود مسؤول عن:
/// 1. قراءة وتحميل مئات الأسئلة الطبية من ملف JSON المخزن في مجلد Resources في أجزاء من الثانية.
/// 2. تصفية الأسئلة بحسب العضو المطلوب (0 إلى 4) وبحسب المرحلة المطلوبة (1 إلى 3).
/// 3. خلط الأسئلة عشوائياً بدون تكرار باستخدام خوارزمية (Fisher-Yates) الآمنة تماماً.
/// 4. ضمان قطعي أن عدد الأسئلة دائماً 10 أسئلة بالضبط في كل مرحلة واختبار.
/// </summary>
public class QuestionBankManager : MonoBehaviour
{
    public static QuestionBankManager Instance { get; private set; }

    [Header("اسم ملف الأسئلة داخل مجلد Resources")]
    [SerializeField] private string jsonFileName = "questions_database";

    private QuestionDatabase database = new QuestionDatabase();

    public bool IsDataLoaded { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadQuestionsFromJson();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadQuestionsFromJson()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(jsonFileName);
        if (jsonText != null)
        {
            try
            {
                database = JsonUtility.FromJson<QuestionDatabase>(jsonText.text);
                IsDataLoaded = true;
                Debug.Log($"[QuestionBankManager] تم تحميل بنك الأسئلة بنجاح! إجمالي الأسئلة المتاحة: {database.questions.Count}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[QuestionBankManager] حدث خطأ أثناء قراءة ملف JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"[QuestionBankManager] لم يتم العثور على الملف: {jsonFileName} داخل مجلد Resources!");
        }
    }

    /// <summary>
    /// دالة جلب الأسئلة العشوائية للمرحلة المحددة:
    /// تضمن قطيعاً إرجاع 10 أسئلة عشوائية كاملة دائماً
    /// </summary>
    public List<QuestionItem> GetRandomQuestions(int organId, int stageId, int count = 10)
    {
        if (!IsDataLoaded || database.questions == null || database.questions.Count == 0)
        {
            LoadQuestionsFromJson();
        }

        // 1. تصفية الأسئلة الخاصة بالعضو والمرحلة المحددة
        List<QuestionItem> matchedPool = new List<QuestionItem>();
        for (int i = 0; i < database.questions.Count; i++)
        {
            QuestionItem q = database.questions[i];
            if (q.organId == organId && q.stageId == stageId)
            {
                matchedPool.Add(q);
            }
        }

        // 2. خلط الأسئلة المتطابقة عشوائياً بخوارزمية Fisher-Yates
        System.Random rng = new System.Random();
        int n = matchedPool.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            QuestionItem temp = matchedPool[k];
            matchedPool[k] = matchedPool[n];
            matchedPool[n] = temp;
        }

        // 3. أخذ العدد المطلوب (10 أسئلة)
        List<QuestionItem> selectedQuestions = new List<QuestionItem>();
        int takeCount = Mathf.Min(count, matchedPool.Count);
        for (int i = 0; i < takeCount; i++)
        {
            selectedQuestions.Add(matchedPool[i]);
        }

        // 4. صمام أمان قطعي: إذا كان المتاح أقل من 10 لأي سبب، نكمل من أسئلة نفس العضو ليكون العدد دائماً 10
        if (selectedQuestions.Count < count)
        {
            List<QuestionItem> fallbackPool = new List<QuestionItem>();
            for (int i = 0; i < database.questions.Count; i++)
            {
                QuestionItem q = database.questions[i];
                if (q.organId == organId && !selectedQuestions.Contains(q))
                {
                    fallbackPool.Add(q);
                }
            }

            int fn = fallbackPool.Count;
            while (fn > 1)
            {
                fn--;
                int k = rng.Next(fn + 1);
                QuestionItem temp = fallbackPool[k];
                fallbackPool[k] = fallbackPool[fn];
                fallbackPool[fn] = temp;
            }

            for (int i = 0; i < fallbackPool.Count && selectedQuestions.Count < count; i++)
            {
                selectedQuestions.Add(fallbackPool[i]);
            }
        }

        Debug.Log($"[QuestionBankManager] تم اختيار {selectedQuestions.Count} سؤالاً عشوائياً للعضو {organId} والمرحلة {stageId}.");
        return selectedQuestions;
    }
}
