using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public float sensitivity = 2.0f;
    public float sensitivityJoystick = 1.0f; // „увствительность мыши
    public float maxYAngle = 80.0f; // ћаксимальный угол вращени€ по вертикали "45"

    public Joystick joystick;

    private float rotationX = 0.0f;
    public bool joystickActive = false;

    private void Update()
    {
        float mouseX = 0;
        float mouseY = 0;

        if (joystickActive)
        {
            float joyX = joystick.Horizontal;
            float joyY = joystick.Vertical;

            if (Mathf.Abs(joyX) > 0.7f)
            {
                mouseX = joyX * sensitivityJoystick;
            }

            if (Mathf.Abs(joyY) > 0.7f)
            {
                mouseY = joyY * sensitivityJoystick;
            }
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        }

        // ¬ращаем персонажа в горизонтальной плоскости
        transform.parent.Rotate(Vector3.up * mouseX * sensitivity);

        // ¬ращаем камеру в вертикальной плоскости
        rotationX -= mouseY * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);
        transform.localRotation = Quaternion.Euler(rotationX, 0.0f, 0.0f);
    }
    
}
