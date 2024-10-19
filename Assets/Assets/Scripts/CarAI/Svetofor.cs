using System.Collections;  // �������� ��� ������������ ����
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public Renderer lightRenderer;  // �������� ��� ��������� ����� ���������
    public Color redLightColor = Color.red;
    public Color greenLightColor = Color.green;
    public bool isGreen = true;  // ������� ��������� ���������

    private float greenLightDuration = 10f;
    private float redLightDuration = 5f;

    void Start()
    {
        StartCoroutine(SwitchTrafficLight());
    }

    // ������������ ���������
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

            isGreen = !isGreen;  // ����������� ���������
        }
    }

    // ����� ��� ��������� ����� ���������
    void SetLightColor(Color color)
    {
        if (lightRenderer != null)
        {
            lightRenderer.material.color = color;
        }
    }

    // ��������, ����� �� ������� ����
    public bool IsGreenLight()
    {
        return isGreen;
    }
}
