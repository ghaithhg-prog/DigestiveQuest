using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// نموذج بيانات السؤال متعدد اللغات (عربي، إنجليزي، فرنسي)
/// </summary>
[System.Serializable]
public class QuestionItem
{
    public int id;
    public int organId;
    public int stageId;

    [Header("اللغة العربية")]
    public string questionText;
    public string[] options;
    public int correctOptionIndex;
    public string educationalTip;

    [Header("English")]
    public string questionText_en;
    public string[] options_en;
    public string educationalTip_en;

    [Header("Français")]
    public string questionText_fr;
    public string[] options_fr;
    public string educationalTip_fr;

    public string GetQuestionText(AppLanguage lang)
    {
        if (lang == AppLanguage.English && !string.IsNullOrEmpty(questionText_en)) return questionText_en;
        if (lang == AppLanguage.French && !string.IsNullOrEmpty(questionText_fr)) return questionText_fr;
        return ArabicTextFixer.Fix(questionText);
    }

    public string[] GetOptions(AppLanguage lang)
    {
        if (lang == AppLanguage.English && options_en != null && options_en.Length > 0) return options_en;
        if (lang == AppLanguage.French && options_fr != null && options_fr.Length > 0) return options_fr;

        if (options == null) return new string[0];
        string[] fixedOpts = new string[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            fixedOpts[i] = ArabicTextFixer.Fix(options[i]);
        }
        return fixedOpts;
    }

    public string GetTip(AppLanguage lang)
    {
        if (lang == AppLanguage.English && !string.IsNullOrEmpty(educationalTip_en)) return educationalTip_en;
        if (lang == AppLanguage.French && !string.IsNullOrEmpty(educationalTip_fr)) return educationalTip_fr;
        return ArabicTextFixer.Fix(educationalTip);
    }
}

[System.Serializable]
public class QuestionDatabase
{
    public List<QuestionItem> questions = new List<QuestionItem>();
}
