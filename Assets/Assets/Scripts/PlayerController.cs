using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Transform _cameraTransform; // Ссылка на камеру

    [SerializeField] private float _moveSpeed = 5f;

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

        // Если нет ввода, не двигаем персонажа
        if (horizontalInput == 0 && verticalInput == 0)
            return;

        // Получаем направление, куда смотрит камера
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        // Игнорируем вертикальную составляющую направления камеры
        forward.y = 0f;
        right.y = 0f;

        // Нормализуем направления, чтобы избежать ускорения при движении по диагонали
        forward.Normalize();
        right.Normalize();

        // Вычисляем направление движения с учетом направления камеры
        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // Применяем движение персонажа
        _rigidbody.MovePosition(_rigidbody.position + moveDirection * _moveSpeed * Time.fixedDeltaTime);
    }
}
