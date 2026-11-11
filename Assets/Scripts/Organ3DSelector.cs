using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// سكربت التفاعل ولمس الأعضاء ثلاثية الأبعاد (Organ3DSelector).
/// هذا الكود مسؤول عن:
/// 1. التقاط نقرات الفأرة على أجهزة الحاسوب (PC Mouse Clicks).
/// 2. التقاط لمسات الأصابع على شاشات هواتف الأندرويد (Android Touch Screen).
/// 3. إضاءة أو تمييز العضو بلون توهجي لطيف عند وضع المؤشر عليه في الحاسوب.
/// 4. منع التداخل مع عناصر واجهة المستخدم (UI) عند الضغط على الأزرار.
/// </summary>
public class Organ3DSelector : MonoBehaviour
{
    [Header("بيانات العضو")]
    [Tooltip("اسم العضو باللغة العربية")]
    public string organArabicName = "المعدة";

    [Tooltip("رقم العضو من 0 إلى 4: \n0: الفم والمريء\n1: المعدة\n2: الكبد والبنكرياس والمرارة\n3: الأمعاء الدقيقة\n4: الأمعاء الغليظة")]
    public int organId = 1;

    [Header("مواد التمييز والتأثير البصري")]
    [Tooltip("المادة الأصلية للعضو")]
    private Material originalMaterial;

    [Tooltip("المادة التوهجية المضيئة عند تحويم الفأرة أو لمس العضو")]
    [SerializeField] private Material hoverHighlightMaterial;

    // مكون الريندر الخاص بالمجسم لتغيير لونه
    private MeshRenderer meshRenderer;

    /// <summary>
    /// دالة البدء Start:
    /// مهمتها: حفظ المادة ولون العضو الأصلي للاسترجاع لاحقاً.
    /// </summary>
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        // التأكد من وجود مصادم ثلاثي الأبعاد (Collider) لتمكين الضغط واللمس
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<MeshCollider>();
        }
    }

    /// <summary>
    /// دالة تحويم الفأرة على العضو (خاصة بالحاسوب PC):
    /// مهمتها: تغيير لون العضو إلى اللون المتوهج عند مرور سهم الفأرة فوقه.
    /// </summary>
    private void OnMouseEnter()
    {
        // إذا كان مؤشر الفأرة فوق قائمة من قوائم الـ UI لا نغير اللون
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (hoverHighlightMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = hoverHighlightMaterial;
        }
    }

    /// <summary>
    /// دالة مغادرة الفأرة للعضو (خاصة بالحاسوب PC):
    /// مهمتها: إعادة العضو إلى لونه الطبيعي فور ابتعاد الفأرة عنه.
    /// </summary>
    private void OnMouseExit()
    {
        if (originalMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// دالة الضغط على العضو (تعمل على الحاسوب والأندرويد معاً):
    /// مهمتها: استشعار نقرة الماوس أو لمسة الإصبع وفتح نافذة مراحل هذا العضو.
    /// </summary>
    private void OnMouseDown()
    {
        // 1. فحص هل ضغط اللاعب على زر في واجهة المستخدم (UI)؟
        // هذا السطر يمنع النقر بالخطأ على العضو إذا كان هناك نافذة مفتوحة فوقه
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        OnOrganSelected();
    }

    /// <summary>
    /// دالة اختيار العضو وتأكيد الضغط:
    /// مهمتها: فتح نافذة اختيار المراحل الثلاث الخاصة بهذا العضو للطفل.
    /// </summary>
    public void OnOrganSelected()
    {
        Debug.Log($"[Organ3DSelector] تم لمس واختيار: {organArabicName} (معرف العضو: {organId})");

        // البحث عن مدير واجهة استكشاف الجسم في المشهد واستدعاء نافذة المراحل
        BodyExplorerUI explorerUI = FindAnyObjectByType<BodyExplorerUI>();
        if (explorerUI != null)
        {
            explorerUI.OpenStagesModal(organId, organArabicName);
        }
        else
        {
            Debug.LogWarning("[Organ3DSelector] لم يتم العثور على سكربت BodyExplorerUI في المشهد!");
        }
    }
}
