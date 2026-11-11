using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// مدير اللغات المتعددة (LocalizationManager).
/// يدعم 3 لغات عالمية: العربية (الافتراضية)، الإنجليزية، والفرنسية.
/// عند تغيير اللغة، يقوم بتحديث كافة عناصر اللعبة، الواجهات، والأسئلة فوراً.
/// </summary>
public enum AppLanguage
{
    Arabic = 0,
    English = 1,
    French = 2
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public static AppLanguage CurrentLanguage { get; private set; } = AppLanguage.Arabic;

    public static event Action OnLanguageChanged;

    private static readonly Dictionary<string, string[]> Dict = new Dictionary<string, string[]>()
    {
        // [Key] = { Arabic, English, French }
        { "app_title", new[] { "🌟 مغامرة استكشاف الجهاز الهضمي 🌟", "🌟 Digestive System Quest 🌟", "🌟 Aventure du Système Digestif 🌟" } },
        { "app_subtitle", new[] { "اختر عضواً من المجسم ثلاثي الأبعاد أو من الأزرار بالأسفل لبدء التحدي!", "Choose an organ from the 3D model or bottom buttons to start the quiz!", "Choisissez un organe sur le modèle 3D ou en bas pour commencer le quiz !" } },

        // الأعضاء
        { "organ_0", new[] { "👄 1. الفم والمريء", "👄 1. Mouth & Esophagus", "👄 1. Bouche et Œsophage" } },
        { "organ_1", new[] { "🫁 2. المعدة", "🫁 2. Stomach", "🫁 2. Estomac" } },
        { "organ_2", new[] { "🫀 3. الكبد والبنكرياس والمرارة", "🫀 3. Liver, Pancreas & Gallbladder", "🫀 3. Foie, Pancréas et Vésicule" } },
        { "organ_3", new[] { "🌀 4. الأمعاء الدقيقة", "🌀 4. Small Intestine", "🌀 4. Intestin Grêle" } },
        { "organ_4", new[] { "⭕ 5. الأمعاء الغليظة والمستقيم", "⭕ 5. Large Intestine & Rectum", "⭕ 5. Gros Intestin et Rectum" } },

        // نافذة المراحل
        { "modal_title_prefix", new[] { "رحلة استكشاف: ", "Exploring: ", "Exploration de : " } },
        { "stage_1_name", new[] { "المستوى 1: تشريح واستكشاف (سهل)", "Level 1: Anatomy & Exploration (Easy)", "Niveau 1 : Anatomie et Découverte (Facile)" } },
        { "stage_2_name", new[] { "المستوى 2: وظائف وكيمياء الهضم (متوسط)", "Level 2: Function & Digestion (Medium)", "Niveau 2 : Fonction et Digestion (Moyen)" } },
        { "stage_3_name", new[] { "المستوى 3: صحة وألغاز ذكاء (متقدم)", "Level 3: Health & Smart Puzzles (Advanced)", "Niveau 3 : Santé et Énigmes (Avancé)" } },
        { "btn_close_modal", new[] { "✕ رجوع للجسم", "✕ Back to Body", "✕ Retour au corps" } },
        { "status_unlocked", new[] { "التقييم: ", "Rating: ", "Note : " } },
        { "status_locked", new[] { "مغلقة 🔒", "Locked 🔒", "Verrouillé 🔒" } },
        { "status_new", new[] { "جديد ⭐", "New ⭐", "Nouveau ⭐" } },

        // نافذة الامتحان
        { "btn_exit_quiz", new[] { "🏠 خروج", "🏠 Exit", "🏠 Quitter" } },
        { "question_prefix", new[] { "السؤال ", "Question ", "Question " } },
        { "of_prefix", new[] { " من ", " of ", " sur " } },
        { "feedback_correct", new[] { "🌟 إجابة ممتازة وصحيحة!", "🌟 Excellent! Correct Answer!", "🌟 Bravo ! Excellente réponse !" } },
        { "feedback_wrong", new[] { "💡 حاول التركيز!", "💡 Try to focus!", "💡 Essaie de te concentrer !" } },
        { "correct_was", new[] { "الإجابة الصحيحة هي: ", "The correct answer is: ", "La bonne réponse est : " } },

        // شاشة الفوز
        { "win_title", new[] { "🎉 أحسنت يا بطل! أنهيت المستوى بنجاح 🎉", "🎉 Awesome Champ! Level Completed! 🎉", "🎉 Félicitations champion ! Niveau réussi ! 🎉" } },
        { "win_score_prefix", new[] { "حصلت على ", "You got ", "Tu as obtenu " } },
        { "win_score_suffix", new[] { " إجابات صحيحة!", " correct answers!", " bonnes réponses !" } },
        { "btn_retry", new[] { "🔄 أسئلة عشوائية جديدة", "🔄 New Random Questions", "🔄 Nouvelles questions aléatoires" } },
        { "btn_back_map", new[] { "🗺️ العودة لخريطة الأعضاء", "🗺️ Back to Organ Map", "🗺️ Retour à la carte des organes" } },
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SetLanguage(AppLanguage lang)
    {
        CurrentLanguage = lang;
        Debug.Log($"[LocalizationManager] Language changed to: {lang}");
        OnLanguageChanged?.Invoke();
    }

    public static string Get(string key)
    {
        if (Dict.ContainsKey(key))
        {
            int langIdx = (int)CurrentLanguage;
            string raw = Dict[key][langIdx];

            // إذا كانت اللغة عربية نقوم بتشكيلها وتوصيل حروفها فوراً
            if (CurrentLanguage == AppLanguage.Arabic)
            {
                return ArabicTextFixer.Fix(raw);
            }
            return raw;
        }
        return key;
    }

    /// <summary>
    /// دالة لمعالجة النصوص الديناميكية المكتوبة بالعربية أو باللغات الأخرى
    /// </summary>
    public static string FormatText(string text, bool isArabicContent = false)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (CurrentLanguage == AppLanguage.Arabic || isArabicContent)
        {
            return ArabicTextFixer.Fix(text);
        }
        return text;
    }
}
