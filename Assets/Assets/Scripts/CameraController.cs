using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 2.0f; // ���������������� ����
    public float sensitivityJoystick = 1.0f; // ���������������� ���������
    public float maxYAngle = 80.0f; // ������������ ���� �������� �� ���������
    public Joystick joystick;
    private float rotationX = 0.0f;
    public bool joystickActive = true;

    void Update()
    {
        float mouseX = 0;
        float mouseY = 0;

        // ���������� �������� ��� ���������� �������
        if (joystickActive)
        {
            float joyX = joystick.Horizontal;
            float joyY = joystick.Vertical;

            // ������� ����� ����������������, ����� �������� ���� �������
            mouseX = joyX * sensitivityJoystick;
            mouseY = joyY * sensitivityJoystick;
        }
        else
        {
            // ���������� �����
            mouseX = Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        }

        // ������� ��������� (��� "����") � �������������� ���������
        transform.parent.Rotate(Vector3.up * mouseX);

        // ������� ������ (������) � ������������ ���������
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);
        transform.localRotation = Quaternion.Euler(rotationX, 0.0f, 0.0f);
    }
}
