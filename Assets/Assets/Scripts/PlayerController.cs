using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Transform _cameraTransform; // ������ �� ������

    [SerializeField] private float _moveSpeed = 5f;

    private void Start()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform; // ���� ������ �� �������, ����� �������� ������
        }
    }

    private void FixedUpdate()
    {
        // �������� ���� �� ���������
        float horizontalInput = _joystick.Horizontal;
        float verticalInput = _joystick.Vertical;

        // ���� ��� �����, �� ������� ���������
        if (horizontalInput == 0 && verticalInput == 0)
            return;

        // �������� �����������, ���� ������� ������
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        // ���������� ������������ ������������ ����������� ������
        forward.y = 0f;
        right.y = 0f;

        // ����������� �����������, ����� �������� ��������� ��� �������� �� ���������
        forward.Normalize();
        right.Normalize();

        // ��������� ����������� �������� � ������ ����������� ������
        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // ��������� �������� ���������
        _rigidbody.MovePosition(_rigidbody.position + moveDirection * _moveSpeed * Time.fixedDeltaTime);
    }
}
