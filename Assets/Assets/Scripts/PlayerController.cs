using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _cameraSensitivity = 0.1f; // ���������������� �������� ������
    [SerializeField] private Transform _cameraTransform; // ������ �� ������, ������� ����� �������

    private Vector2 _startTouchPosition;
    private Vector2 _currentTouchPosition;
    private bool _isRotating;

    private void FixedUpdate()
    {
        // ���������� ��������� ������
        _rigidbody.velocity = new Vector3(_joystick.Horizontal * _moveSpeed, _rigidbody.velocity.y, _joystick.Vertical * _moveSpeed);
    }

    private void Update()
    {
        HandleCameraRotation();
    }

    private void HandleCameraRotation()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // ���������, ��� ����� �� ������ ������� ������
            if (touch.position.x > Screen.width / 2)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _startTouchPosition = touch.position;
                        _isRotating = true;
                        break;

                    case TouchPhase.Moved:
                        if (_isRotating)
                        {
                            _currentTouchPosition = touch.position;
                            Vector2 deltaSwipe = _currentTouchPosition - _startTouchPosition;

                            // �������� ������
                            float rotationX = deltaSwipe.x * _cameraSensitivity;
                            float rotationY = -deltaSwipe.y * _cameraSensitivity;

                            _cameraTransform.Rotate(Vector3.up, rotationX);   // �������� �����-������ �� ��� Y
                            _cameraTransform.Rotate(Vector3.right, rotationY); // �������� �����-���� �� ��� X

                            _startTouchPosition = _currentTouchPosition; // ��������� ������� ��� ������������ ��������
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        _isRotating = false;
                        break;
                }
            }
        }
    }
}
