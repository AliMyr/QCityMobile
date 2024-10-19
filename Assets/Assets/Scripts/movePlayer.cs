using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 5f;  // Скорость движения персонажа
    public float mouseSensitivity = 100f;  // Чувствительность мыши
    public Transform playerCamera;  // Ссылка на камеру персонажа (для управления головой)
    private Vector3 moveVector;

    private float rotationX = 0f;  // Переменная для хранения вертикального вращения (вверх/вниз)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;  // Замораживаем вращение, чтобы персонаж не заваливался
    }

    void Update()
    {
        // Управление поворотом персонажа по горизонтали (влево/вправо)
        RotatePlayer();

        // Управление поворотом камеры (вверх/вниз)
        RotateCamera();

        // Управление движением персонажа с клавиатуры
        MovePlayerCharacter();
    }

    private void RotatePlayer()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);  // Поворачиваем тело персонажа по оси Y
    }

    private void RotateCamera()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotationX -= mouseY;  // Инвертируем направление по оси X (мышь вверх -> камера вниз)
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);  // Ограничиваем вертикальный поворот головы до 90 градусов

        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);  // Вращаем камеру по оси X
    }

    private void MovePlayerCharacter()
    {
        // Управление движением персонажа
        moveVector.x = Input.GetAxisRaw("Horizontal");
        moveVector.z = Input.GetAxisRaw("Vertical");

        // Приводим в движение персонажа в зависимости от поворота
        Vector3 movement = transform.forward * moveVector.z + transform.right * moveVector.x;
        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
    }
}
