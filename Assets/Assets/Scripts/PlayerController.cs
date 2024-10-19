using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Transform _cameraTransform; // Ссылка на камеру

    [SerializeField] private float _moveSpeed;

    private void Start()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform; // Если камера не указана, найти основную камеру
        }
    }

    private void FixedUpdate()
    {
        // Получаем вход от джойстика
        float horizontalInput = _joystick.Horizontal;
        float verticalInput = _joystick.Vertical;

        // Получаем направления камеры
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        // Игнорируем вертикальную составляющую направления
        forward.y = 0f;
        right.y = 0f;

        // Нормализуем направления, чтобы не было ускорения при движении по диагонали
        forward.Normalize();
        right.Normalize();

        // Вычисляем вектор движения
        Vector3 moveDirection = forward * verticalInput + right * horizontalInput;

        // Задаем скорость персонажа с учётом направления камеры
        _rigidbody.velocity = new Vector3(moveDirection.x * _moveSpeed, _rigidbody.velocity.y, moveDirection.z * _moveSpeed);
    }
}
