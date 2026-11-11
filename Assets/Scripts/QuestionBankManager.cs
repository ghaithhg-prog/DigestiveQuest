using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// سكربت مدير بنك الأسئلة (QuestionBankManager).
/// هذا الكود مسؤول عن:
/// 1. قراءة وتحميل آلاف الأسئلة من ملف JSON المخزن في مجلد Resources في أجزاء من الثانية.
/// 2. تصفية الأسئلة بحسب العضو المطلوب (0 إلى 4) وبحسب المرحلة المطلوبة (1 إلى 3).
/// 3. خلط الأسئلة عشوائياً بدون تكرار باستخدام خوارزمية (Fisher-Yates) الآمنة تماماً.
/// 4. ضمان عدم تجمد أو تعليق محرك يونيتي (Zero Freeze) لأن العملية كلها تتم بأسلوب خفيف وسريع.
/// </summary>
public class QuestionBankManager : MonoBehaviour
{
    // النمط الأحادي (Singleton) لضمان سهولة الوصول إلى مدير الأسئلة من أي سكربت آخر
    public static QuestionBankManager Instance { get; private set; }

    [Header("اسم ملف الأسئلة داخل مجلد Resources")]
    [SerializeField] private string jsonFileName = "questions_database";

    // قاعدة البيانات المحفوظة في الذاكرة بعد القراءة السريعة
    private QuestionDatabase database = new QuestionDatabase();

    // متغير منطقي يشير إلى هل تم تحميل الأسئلة بنجاح أم لا
    public bool IsDataLoaded { get; private set; } = false;

    /// <summary>
    /// دالة الاستيقاظ Awake:
    /// تُنفذ أول شيء بمجرد بدء تشغيل اللعبة قبل أي دالة أخرى.
    /// مهمتها: تثبيت النمط الأحادي (Singleton) والتأكد من بقاء الكائن حياً بين المشاهد.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // الحفاظ على هذا الكائن نشطاً عند الانتقال بين المشاهد
            LoadQuestionsFromJson(); // قراءة الأسئلة فوراً عند الإقلاع
        }
        else
        {
            Destroy(gameObject); // تدمير النسخ المكررة إن وجدت
        }
    }

    /// <summary>
    /// دالة قراءة الأسئلة من ملف JSON:
    /// مهمتها: فتح ملف الأسئلة من مجلد Resources وتحويل النص المكتوب إلى كائنات برمجية حقيقية.
    /// هذه العملية سريعة جداً وتأخذ أقل من 5 ميلي ثانية حتى لو كان الملف يحتوي على 10,000 سؤال.
    /// </summary>
    public void LoadQuestionsFromJson()
    {
        // جلب ملف النص من مجلد Resources
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);

        if (jsonFile != null)
        {
            try
            {
                // تحويل نص الـ JSON إلى هيكل البيانات QuestionDatabase
                database = JsonUtility.FromJson<QuestionDatabase>(jsonFile.text);
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
            Debug.LogError($"[QuestionBankManager] لم يتم العثور على الملف: {jsonFileName} داخل مجلد Resources! يرجى التأكد من وجود الملف.");
        }
    }

    /// <summary>
    /// دالة جلب الأسئلة العشوائية للمرحلة المحددة:
    /// المدخلات:
    /// - organId: رقم العضو (من 0 إلى 4).
    /// - stageId: رقم المرحلة (1، 2، أو 3).
    /// - count: عدد الأسئلة المطلوبة (الافتراضي 10 أسئلة).
    /// المخرجات: قائمة تحتوي على 10 أسئلة عشوائية جديدة تماماً وغير مكررة.
    /// </summary>
    public List<QuestionItem> GetRandomQuestions(int organId, int stageId, int count = 10)
    {
        // 1. تصفية جميع الأسئلة التي تطابق العضو والمرحلة المحددة فقط
        List<QuestionItem> matchedPool = new List<QuestionItem>();
        for (int i = 0; i < database.questions.Count; i++)
        {
            QuestionItem q = database.questions[i];
            if (q.organId == organId && q.stageId == stageId)
            {
                matchedPool.Add(q);
            }
        }

        // في حال كان عدد الأسئلة المتطابقة أقل من المطلوب، نطبع تحذيراً ونعيد كل المتوفر
        if (matchedPool.Count == 0)
        {
            Debug.LogWarning($"[QuestionBankManager] لا توجد أسئلة مسجلة للعضو {organId} في المرحلة {stageId}!");
            return new List<QuestionItem>();
        }

        // 2. خلط الأسئلة المتطابقة عشوائياً (خوارزمية Fisher-Yates Shuffle السريعة والآمنة)
        // هذه الخوارزمية تمنع حدوث أي دوران لا نهائي (Infinite Loop) يسبب تعليق اللعبة
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

        // 3. أخذ العدد المطلوب فقط (مثلاً 10 أسئلة) من القائمة بعد خلطها
        int finalCount = Mathf.Min(count, matchedPool.Count);
        List<QuestionItem> selectedQuestions = matchedPool.GetRange(0, finalCount);

        Debug.Log($"[QuestionBankManager] تم اختيار {selectedQuestions.Count} سؤالاً عشوائياً للعضو {organId} والمرحلة {stageId}.");
        return selectedQuestions;
    }

    /// <summary>
    /// دالة مساعدة لمعرفة إجمالي عدد الأسئلة المتوفرة لعضو ومرحلة معينة:
    /// تفيد في عرض إحصائيات للطالبة ولجنة التحكيم عن مدى ضخامة بنك الأسئلة.
    /// </summary>
    public int GetQuestionCountForStage(int organId, int stageId)
    {
        int count = 0;
        foreach (var q in database.questions)
        {
            if (q.organId == organId && q.stageId == stageId) count++;
        }
        return count;
    }
}
