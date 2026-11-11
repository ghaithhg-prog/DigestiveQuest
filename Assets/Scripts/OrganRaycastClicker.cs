using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// سكربت النقر والتحويم المباشر على الأعضاء ثلاثية الأبعاد (OrganRaycastClicker).
/// 1. إظهار رسالة تعريفية صغيرة ومتحركة بجانب مؤشر الماوس عند الاقتراب من أي عضو (Hover Tooltip).
/// 2. إضاءة العضو ببريق خفيف عند التحويم فوقه.
/// 3. استشعار النقر المباشر لاختيار العضو وإضاءته باللون الذهبي المشع وفتح المراحل.
/// </summary>
public class OrganRaycastClicker : MonoBehaviour
{
    [Header("عناصر الواجهة لرسالة التحويم (Tooltip)")]
    public GameObject tooltipPanel;
    public Text tooltipText;
    public RectTransform canvasRect;

    private Vector2 pointerDownPos;
    private float pointerDownTime;
    private Organ3DSelector currentHovered = null;

    private void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void Update()
    {
        HandleHoverTooltip();
        HandleClick();
    }

    private void HandleHoverTooltip()
    {
        // إذا كان الماوس فوق أي عنصر من عناصر واجهة المستخدم (UI)، نخفي الرسالة فوراً
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            return;
        }

        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Organ3DSelector detectedSelector = null;
        foreach (var hit in hits)
        {
            Organ3DSelector selector = hit.collider.GetComponentInParent<Organ3DSelector>();
            if (selector != null)
            {
                detectedSelector = selector;
                break;
            }
        }

        if (detectedSelector != null)
        {
            // تحديث العضو المحوم فوقه
            if (currentHovered != detectedSelector)
            {
                if (currentHovered != null) currentHovered.SetHoverGlow(false);
                currentHovered = detectedSelector;
                currentHovered.SetHoverGlow(true);
            }

            // إظهار وتحديث نص التول تيب
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(true);

                if (tooltipText != null)
                {
                    string organName = LocalizationManager.Get($"organ_{detectedSelector.organId}");
                    AppLanguage lang = LocalizationManager.CurrentLanguage;
                    if (lang == AppLanguage.Arabic)
                        tooltipText.text = ArabicTextFixer.Fix($"العضو: {organName}");
                    else if (lang == AppLanguage.French)
                        tooltipText.text = $"Organe : {organName}";
                    else
                        tooltipText.text = $"Organ: {organName}";
                }

                // موضع الرسالة بجانب الماوس بسلاسة داخل الكانفاس
                RectTransform ttRt = tooltipPanel.GetComponent<RectTransform>();
                if (ttRt != null && canvasRect != null)
                {
                    Vector2 localPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localPos);
                    ttRt.anchoredPosition = localPos + new Vector2(30, 30);
                }
            }
        }
        else
        {
            ClearHover();
        }
    }

    private void ClearHover()
    {
        if (currentHovered != null)
        {
            currentHovered.SetHoverGlow(false);
            currentHovered = null;
        }
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerDownPos = Input.mousePosition;
            pointerDownTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float dist = Vector2.Distance(pointerDownPos, (Vector2)Input.mousePosition);
            float duration = Time.time - pointerDownTime;

            // نقرة صريحة وليست سحباً لتدوير المجسم
            if (dist < 15f && duration < 0.5f)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    if (currentHovered != null)
                    {
                        currentHovered.OnOrganSelected();
                    }
                }
            }
        }
    }
}
