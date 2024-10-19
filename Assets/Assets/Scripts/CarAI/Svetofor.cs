using System.Collections;  // Добавьте это пространство имен
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public Renderer lightRenderer;  // Рендерер для изменения цвета светофора
    public Color redLightColor = Color.red;
    public Color greenLightColor = Color.green;
    public bool isGreen = true;  // Текущее состояние светофора

    private float greenLightDuration = 10f;
    private float redLightDuration = 5f;

    void Start()
    {
        StartCoroutine(SwitchTrafficLight());
    }

    // Переключение светофора
    IEnumerator SwitchTrafficLight()
    {
        while (true)
        {
            if (isGreen)
            {
                SetLightColor(greenLightColor);
                yield return new WaitForSeconds(greenLightDuration);
            }
            else
            {
                SetLightColor(redLightColor);
                yield return new WaitForSeconds(redLightDuration);
            }

            isGreen = !isGreen;  // Переключаем состояние
        }
    }

    // Метод для установки цвета светофора
    void SetLightColor(Color color)
    {
        if (lightRenderer != null)
        {
            lightRenderer.material.color = color;
        }
    }

    // Проверка, горит ли зеленый свет
    public bool IsGreenLight()
    {
        return isGreen;
    }
}
