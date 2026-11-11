using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// سكربت النقر المباشر على الأعضاء ثلاثية الأبعاد (OrganRaycastClicker).
/// يضمن استشعار أي نقرة ماوس أو لمسة إصبع على العضو ثلاثي الأبعاد مباشرة
/// دون أي تداخل مع دوران المجسم أو عناصر الواجهة.
/// </summary>
public class OrganRaycastClicker : MonoBehaviour
{
    private Vector2 pointerDownPos;
    private float pointerDownTime;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerDownPos = Input.mousePosition;
            pointerDownTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // إذا كانت المسافة قصيرة والمدة أقل من نصف ثانية، فهي نقرة صريحة وليست سحباً
            float dist = Vector2.Distance(pointerDownPos, (Vector2)Input.mousePosition);
            float duration = Time.time - pointerDownTime;

            if (dist < 12f && duration < 0.5f)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    TryRaycastOrgan(Input.mousePosition);
                }
            }
        }
    }

    private void TryRaycastOrgan(Vector3 screenPos)
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        // الترتيب حسب الأقرب للكاميرا
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            Organ3DSelector selector = hit.collider.GetComponentInParent<Organ3DSelector>();
            if (selector != null)
            {
                Debug.Log($"[OrganRaycastClicker] تم النقر المباشر على العضو: {selector.organArabicName} (ID: {selector.organId})");
                selector.OnOrganSelected();
                return;
            }
        }
    }
}
