using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// سكربت التفاعل ولمس وإضاءة الأعضاء ثلاثية الأبعاد (Organ3DSelector).
/// - استشعار النقر بالماوس أو اللمس بالأصابع.
/// - إضاءة العضو باللون الذهبي المشع عند اختياره (Golden Emission Glow).
/// - إضاءة ناعمة عند تحويم الماوس فوق العضو.
/// - التنسيق مع واجهة المستخدم لفتح المراحل وتمييز البطاقات.
/// </summary>
public class Organ3DSelector : MonoBehaviour
{
    public static List<Organ3DSelector> AllSelectors = new List<Organ3DSelector>();
    public static Organ3DSelector CurrentSelected = null;

    [Header("بيانات العضو")]
    public string organArabicName = "المعدة";
    public int organId = 1;

    private MeshRenderer meshRenderer;
    private Color[] originalMainColors;
    private Color[] originalEmissionColors;
    private bool isSelected = false;
    private bool isHovered = false;
    private Coroutine pulseCoroutine = null;

    private void Awake()
    {
        if (!AllSelectors.Contains(this))
        {
            AllSelectors.Add(this);
        }
    }

    private void OnDestroy()
    {
        AllSelectors.Remove(this);
    }

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material[] mats = meshRenderer.materials;
            originalMainColors = new Color[mats.Length];
            originalEmissionColors = new Color[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_Color"))
                    originalMainColors[i] = mats[i].GetColor("_Color");
                else
                    originalMainColors[i] = Color.white;

                if (mats[i].HasProperty("_EmissionColor"))
                    originalEmissionColors[i] = mats[i].GetColor("_EmissionColor");
                else
                    originalEmissionColors[i] = Color.black;
            }
        }

        if (GetComponent<Collider>() == null)
        {
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.convex = false;
        }
    }

    /// <summary>
    /// إضاءة العضو باللون الذهبي المشع عند اختياره
    /// </summary>
    public void SetSelectedGlow(bool selected)
    {
        isSelected = selected;

        if (meshRenderer == null || originalMainColors == null) return;
        Material[] mats = meshRenderer.materials;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (selected)
        {
            CurrentSelected = this;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].EnableKeyword("_EMISSION");
                mats[i].SetColor("_Color", new Color(1.0f, 0.88f, 0.40f)); // لون ذهبي مشرق
                mats[i].SetColor("_EmissionColor", new Color(1.0f, 0.82f, 0.20f) * 1.5f);
            }
            pulseCoroutine = StartCoroutine(PulseGlowRoutine());
        }
        else
        {
            if (CurrentSelected == this) CurrentSelected = null;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor("_Color", originalMainColors[i]);
                mats[i].SetColor("_EmissionColor", originalEmissionColors[i]);
                if (originalEmissionColors[i] == Color.black)
                {
                    mats[i].DisableKeyword("_EMISSION");
                }
            }
        }
    }

    /// <summary>
    /// إضاءة ناعمة عند تحويم الماوس
    /// </summary>
    public void SetHoverGlow(bool hover)
    {
        if (isSelected) return; // العضو المختار مضاء بذهبي كامل مسبقاً
        if (isHovered == hover) return;
        isHovered = hover;

        if (meshRenderer == null || originalMainColors == null) return;
        Material[] mats = meshRenderer.materials;

        for (int i = 0; i < mats.Length; i++)
        {
            if (hover)
            {
                mats[i].EnableKeyword("_EMISSION");
                Color hoverGold = Color.Lerp(originalMainColors[i], new Color(1.0f, 0.9f, 0.5f), 0.45f);
                mats[i].SetColor("_Color", hoverGold);
                mats[i].SetColor("_EmissionColor", new Color(0.85f, 0.70f, 0.15f) * 0.8f);
            }
            else
            {
                mats[i].SetColor("_Color", originalMainColors[i]);
                mats[i].SetColor("_EmissionColor", originalEmissionColors[i]);
                if (originalEmissionColors[i] == Color.black)
                {
                    mats[i].DisableKeyword("_EMISSION");
                }
            }
        }
    }

    private IEnumerator PulseGlowRoutine()
    {
        while (isSelected)
        {
            if (meshRenderer != null)
            {
                float wave = (Mathf.Sin(Time.time * 3.5f) + 1.0f) * 0.5f; // 0 to 1
                Color goldGlow = Color.Lerp(new Color(1.0f, 0.75f, 0.15f), new Color(1.0f, 0.95f, 0.45f), wave);
                float intensity = 1.2f + 0.9f * wave;

                Material[] mats = meshRenderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].SetColor("_EmissionColor", goldGlow * intensity);
                }
            }
            yield return null;
        }
    }

    public void OnOrganSelected()
    {
        BodyExplorerUI explorerUI = FindFirstObjectByType<BodyExplorerUI>();
        if (explorerUI != null)
        {
            explorerUI.SelectOrgan(organId);
        }
    }

    /// <summary>
    /// دالة استدعاء عامة لتمييز عضو بالمعرف ID
    /// </summary>
    public static void HighlightOrganById(int targetOrganId)
    {
        for (int i = 0; i < AllSelectors.Count; i++)
        {
            if (AllSelectors[i] != null)
            {
                bool isTarget = (AllSelectors[i].organId == targetOrganId);
                AllSelectors[i].SetSelectedGlow(isTarget);
            }
        }
    }

    public static void ClearAllHighlights()
    {
        for (int i = 0; i < AllSelectors.Count; i++)
        {
            if (AllSelectors[i] != null)
            {
                AllSelectors[i].SetSelectedGlow(false);
                AllSelectors[i].SetHoverGlow(false);
            }
        }
    }
}
