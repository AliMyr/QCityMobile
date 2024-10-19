using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 2.0f; // Чувствительность мыши
    public float sensitivityJoystick = 1.0f; // Чувствительность джойстика
    public float maxYAngle = 80.0f; // Максимальный угол вращения по вертикали
    public Joystick joystick;
    private float rotationX = 0.0f;
    public bool joystickActive = true;

    void Update()
    {
        float mouseX = 0;
        float mouseY = 0;

        // Используем джойстик для управления головой
        if (joystickActive)
        {
            float joyX = joystick.Horizontal;
            float joyY = joystick.Vertical;

            // Снижаем порог чувствительности, чтобы вращение было плавнее
            mouseX = joyX * sensitivityJoystick;
            mouseY = joyY * sensitivityJoystick;
        }
        else
        {
            // Управление мышью
            mouseX = Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        }

        // Вращаем персонажа (его "тело") в горизонтальной плоскости
        transform.parent.Rotate(Vector3.up * mouseX);

        // Вращаем камеру (голову) в вертикальной плоскости
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);
        transform.localRotation = Quaternion.Euler(rotationX, 0.0f, 0.0f);
    }
}
