using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// هذا السكربت يحتوي على الهياكل البيانية (Data Models) لتخزين الأسئلة وبنك الأسئلة الضخم.
/// تم تصميمه ليكون قابلاً للتسلسل (Serializable) ليتم تحويله من وإلى ملفات JSON بسرعة فائقة
/// دون أي استهلاك زائد لذاكرة الجهاز، مما يمنع تعليق يونيتي تماماً حتى لو احتوى البنك على 10,000 سؤال.
/// </summary>

[System.Serializable]
public class QuestionItem
{
    // المعرف الرقمي الفريد لكل سؤال (ID)
    public int id;

    // العضو المستهدف من الأعضاء الـ 5:
    // 0 = الفم والمريء
    // 1 = المعدة
    // 2 = الكبد والبنكرياس والمرارة
    // 3 = الأمعاء الدقيقة
    // 4 = الأمعاء الغليظة والمستقيم
    public int organId;

    // المرحلة المستهدفة من المراحل الـ 3:
    // 1 = المرحلة الأولى (تشريح وموقع - سهل)
    // 2 = المرحلة الثانية (وظيفة وإنزيمات - متوسط)
    // 3 = المرحلة الثالثة (صحة وألغاز - صعب)
    public int stageId;

    // نص السؤال باللغة العربية
    public string questionText;

    // مصفوفة تحتوي على خيارات الإجابة (عادة 4 خيارات)
    public string[] options;

    // رقم الإجابة الصحيحة (0، 1، 2، أو 3)
    public int correctOptionIndex;

    // شرح وتعليل علمي مبسط يظهر للطفل بعد الإجابة لتعزيز المعلومة
    public string educationalTip;
}

[System.Serializable]
public class QuestionDatabase
{
    // قائمة تحتوي على جميع الأسئلة المحملة من الملف (تتسع لآلاف الأسئلة دون أي مشكلة)
    public List<QuestionItem> questions = new List<QuestionItem>();
}
