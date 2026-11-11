using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// سكربت تدوير المجسم ثلاثي الأبعاد بالفأرة واللمس (ModelRotator).
/// يجعل نموذج الجهاز الهضمي يدور ببطء تلقائياً، مع إمكانية سحبه وتدويره بالكامل بالماوس أو اللمس.
/// </summary>
public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float idleSpeed = 15f;
    [SerializeField] private float dragSpeed = 250f;
    private bool isDragging = false;
    private Vector3 lastMousePos;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            float rotY = -delta.x * dragSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotY, Space.World);
            lastMousePos = Input.mousePosition;
        }
        else
        {
            // دوران بطيء تلقائي جذاب
            transform.Rotate(Vector3.up, idleSpeed * Time.deltaTime, Space.World);
        }
    }
}
