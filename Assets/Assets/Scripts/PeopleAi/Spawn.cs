using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CubeManager : MonoBehaviour
{
    public GameObject cubePrefab;  // ������ ����
    public Transform[] homes;      // ����� ������ (����)
    public Transform[] waypoints;  // ����� ����� (������, �������� � �.�.)
    public float spawnInterval = 5f; // �������� ��������� �����

    void Start()
    {
        // ��������� ������� ��� �������������� ������ �����
        StartCoroutine(SpawnCubes());
    }

    IEnumerator SpawnCubes()
    {
        while (true)
        {
            // ����� ���� � ���������� �����������
            SpawnRandomCube();

            // ���� ����� ��������� �������
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ����� ��� ������ ���� � ���������� �����������
    void SpawnRandomCube()
    {
        // �������� ��������� ��� ��� ������
        Transform home = homes[Random.Range(0, homes.Length)];

        // ������� ����� ��� � ������� ����
        GameObject newCube = Instantiate(cubePrefab, home.position, Quaternion.identity);

        // �������� ������ ���������� �����
        CubeAI cubeAI = newCube.GetComponent<CubeAI>();

        // ��������� �� 2 �� 6 ��������� ����� (waypoints) ��� ����� ����
        int taskCount = Random.Range(2, 7);
        Transform[] assignedWaypoints = new Transform[taskCount];

        // ��������� ������ �����
        for (int i = 0; i < taskCount; i++)
        {
            assignedWaypoints[i] = waypoints[Random.Range(0, waypoints.Length)];
        }

        // ������������� ���� � ������������ �������� � ������ ������ (�����)
        cubeAI.InitializeTasks(assignedWaypoints, home);
    }
}
