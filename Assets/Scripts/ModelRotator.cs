using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// سكربت تدوير المجسم ثلاثي الأبعاد بالفأرة واللمس (ModelRotator).
/// يجعل النموذج يدور ببطء وتلقائية، ويسمح للطفل بسحبه وتدويره في أي اتجاه.
/// </summary>
public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float idleSpeed = 12f;
    [SerializeField] private float dragSpeed = 220f;
    
    private bool isDragging = false;
    private Vector3 pointerDownPos;
    private Vector3 lastMousePos;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                pointerDownPos = Input.mousePosition;
                lastMousePos = Input.mousePosition;
                isDragging = false;
            }
        }

        if (Input.GetMouseButton(0))
        {
            float dist = Vector3.Distance(pointerDownPos, Input.mousePosition);
            if (dist > 8f) // يبدأ التدوير فقط بعد حركة فعلية بالماوس لمنع الخلط بين النقر والسحب
            {
                isDragging = true;
                Vector3 delta = Input.mousePosition - lastMousePos;
                float rotY = -delta.x * dragSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotY, Space.World);
                lastMousePos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging)
        {
            // دوران استوديو بطيء تلقائي جذاب
            transform.Rotate(Vector3.up, idleSpeed * Time.deltaTime, Space.World);
        }
    }
}
