using UnityEngine;

/// <summary>
/// سكربت إدارة حفظ تقدم اللاعب (GameProgressManager).
/// هذا الكود مسؤول عن:
/// 1. حفظ المراحل التي فاز بها الطفل والنجوم التي حصل عليها بشكل دائم.
/// 2. فتح المرحلة التالية تلقائياً عند تجاوز المرحلة السابقة بنجاح.
/// 3. العمل بسلاسة على كل من نظام الويندوز (Windows Registry) وهواتف الأندرويد (Shared Preferences).
/// </summary>
public static class GameProgressManager
{
    /// <summary>
    /// دالة حفظ نتيجة مرحلة معينة:
    /// المدخلات:
    /// - organId: رقم العضو (0 إلى 4).
    /// - stageId: رقم المرحلة (1 إلى 3).
    /// - stars: عدد النجوم المكتسبة (1 إلى 3).
    /// مهمتها: تخزين النجوم، ثم فتح المرحلة التالية إن وجدت.
    /// </summary>
    public static void SaveStageProgress(int organId, int stageId, int stars)
    {
        // 1. مفتاح حفظ النجوم لهذا العضو وهذه المرحلة
        string starKey = $"Stars_Organ_{organId}_Stage_{stageId}";
        int previousStars = PlayerPrefs.GetInt(starKey, 0);

        // إذا أحرز الطفل نجوماً أكثر من محاولته السابقة، نقوم بتحديثها
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(starKey, stars);
        }

        // 2. إذا كانت هناك مرحلة تالية لنفس العضو (مثلاً تجاوز المرحلة 1 لفتح المرحلة 2)، نفتحها
        if (stageId < 3)
        {
            string nextStageKey = $"Unlocked_Organ_{organId}_Stage_{stageId + 1}";
            PlayerPrefs.SetInt(nextStageKey, 1); // 1 تعني مفتوحة
        }
        else if (stageId == 3 && organId < 4)
        {
            // إذا أنهى المرحلة الثالثة من هذا العضو، نفتح له العضو التالي تلقائياً!
            string nextOrganKey = $"Unlocked_Organ_{organId + 1}_Stage_1";
            PlayerPrefs.SetInt(nextOrganKey, 1);
        }

        // حفظ التغييرات على القرص فوراً
        PlayerPrefs.Save();
        Debug.Log($"[GameProgressManager] تم حفظ تقدم العضو {organId} والمرحلة {stageId} بنجاح. النجوم: {stars}");
    }

    /// <summary>
    /// دالة فحص هل المرحلة مفتوحة أم مقفلة:
    /// المدخلات:
    /// - organId: رقم العضو (0 إلى 4).
    /// - stageId: رقم المرحلة (1 إلى 3).
    /// المخرجات: true إذا كانت المرحلة مفتوحة للعب، و false إذا كانت مقفلة بقفل.
    /// </summary>
    public static bool IsStageUnlocked(int organId, int stageId)
    {
        // العضو الأول والمرحلة الأولى دائماً مفتوحة للاعب في البداية
        if (organId == 0 && stageId == 1)
        {
            return true;
        }

        // فحص هل سُجلت هذه المرحلة كمفتوحة في ذاكرة الجهاز
        string key = $"Unlocked_Organ_{organId}_Stage_{stageId}";
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    /// <summary>
    /// دالة جلب عدد النجوم المحرزة في مرحلة معينة:
    /// مهمتها: إرجاع عدد النجوم السابقة لعرضها على خريطة المراحل.
    /// </summary>
    public static int GetStageStars(int organId, int stageId)
    {
        string key = $"Stars_Organ_{organId}_Stage_{stageId}";
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// دالة تصفير التقدم (Reset All Progress):
    /// مفيدة للمطور وأثناء اختبار المشروع لإعادة قفل جميع المراحل والبدء من الصفر.
    /// </summary>
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[GameProgressManager] تم تصفير جميع بيانات اللعبة بنجاح.");
    }
}
