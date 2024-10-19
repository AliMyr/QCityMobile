using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Transform _cameraTransform; // ������ �� ������

    [SerializeField] private float _moveSpeed;

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

        // �������� ����������� ������
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        // ���������� ������������ ������������ �����������
        forward.y = 0f;
        right.y = 0f;

        // ����������� �����������, ����� �� ���� ��������� ��� �������� �� ���������
        forward.Normalize();
        right.Normalize();

        // ��������� ������ ��������
        Vector3 moveDirection = forward * verticalInput + right * horizontalInput;

        // ������ �������� ��������� � ������ ����������� ������
        _rigidbody.velocity = new Vector3(moveDirection.x * _moveSpeed, _rigidbody.velocity.y, moveDirection.z * _moveSpeed);
    }
}
