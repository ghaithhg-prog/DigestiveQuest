using UnityEngine;

/// <summary>
/// سكربت إدارة حفظ تقدم اللاعب (GameProgressManager).
/// يسمح بفتح المرحلة 1 لجميع الأعضاء الخمسة تلقائياً لتمكين الطفل ولجنة المناقشة من تجربة أي عضو مباشرة.
/// </summary>
public static class GameProgressManager
{
    public static void SaveStageProgress(int organId, int stageId, int stars)
    {
        string starKey = $"Stars_Organ_{organId}_Stage_{stageId}";
        int previousStars = PlayerPrefs.GetInt(starKey, 0);

        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(starKey, stars);
        }

        // فتح المرحلة التالية
        if (stageId < 3)
        {
            string nextStageKey = $"Unlocked_Organ_{organId}_Stage_{stageId + 1}";
            PlayerPrefs.SetInt(nextStageKey, 1);
        }
        else if (stageId == 3 && organId < 4)
        {
            string nextOrganKey = $"Unlocked_Organ_{organId + 1}_Stage_1";
            PlayerPrefs.SetInt(nextOrganKey, 1);
        }

        PlayerPrefs.Save();
        Debug.Log($"[GameProgressManager] تم حفظ تقدم العضو {organId} والمرحلة {stageId} بنجاح. النجوم: {stars}");
    }

    /// <summary>
    /// دالة فحص هل المرحلة مفتوحة أم مقفلة:
    /// المرحلة الأولى مفتوحة دائماً في جميع الأعضاء الـ 5 حتى يتمكن الطفل من تجربة أي عضو يريده مباشرة!
    /// </summary>
    public static bool IsStageUnlocked(int organId, int stageId)
    {
        // المرحلة الأولى من كل عضو مفتوحة دائماً
        if (stageId == 1)
        {
            return true;
        }

        string key = $"Unlocked_Organ_{organId}_Stage_{stageId}";
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static int GetStageStars(int organId, int stageId)
    {
        string key = $"Stars_Organ_{organId}_Stage_{stageId}";
        return PlayerPrefs.GetInt(key, 0);
    }

    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[GameProgressManager] تم تصفير جميع بيانات اللعبة بنجاح.");
    }
}
