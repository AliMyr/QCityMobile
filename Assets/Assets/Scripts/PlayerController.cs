using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _cameraSensitivity = 0.1f; // Чувствительность вращения камеры
    [SerializeField] private Transform _cameraTransform; // Ссылка на камеру, которую нужно вращать

    private Vector2 _startTouchPosition;
    private Vector2 _currentTouchPosition;
    private bool _isRotating;

    private void FixedUpdate()
    {
        // Управление движением игрока
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

            // Проверяем, что свайп на правой стороне экрана
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

                            // Вращение камеры
                            float rotationX = deltaSwipe.x * _cameraSensitivity;
                            float rotationY = -deltaSwipe.y * _cameraSensitivity;

                            _cameraTransform.Rotate(Vector3.up, rotationX);   // Вращение влево-вправо по оси Y
                            _cameraTransform.Rotate(Vector3.right, rotationY); // Вращение вверх-вниз по оси X

                            _startTouchPosition = _currentTouchPosition; // Обновляем позицию для последующего движения
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
