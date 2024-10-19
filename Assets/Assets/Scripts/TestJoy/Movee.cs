using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControllerPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool pressed = false;
    public float sensitivity = 0.1f;
    private int fingerId;
    public Transform cameraTransform;  // Ссылка на объект камеры
    private Vector2 lastTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            pressed = true;
            fingerId = eventData.pointerId;
            lastTouchPosition = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    private void Update()
    {
        if (pressed)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.fingerId == fingerId && touch.phase == TouchPhase.Moved)
                {
                    // Вычисление смещения пальца
                    Vector2 deltaPosition = touch.deltaPosition * sensitivity;

                    // Поворот камеры на основе перемещения пальца
                    cameraTransform.Rotate(Vector3.up, deltaPosition.x, Space.World); // Горизонтальный поворот
                    cameraTransform.Rotate(Vector3.right, -deltaPosition.y);          // Вертикальный поворот
                }
            }
        }
    }
}
