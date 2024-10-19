using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class CubeManager : MonoBehaviour
{
    public GameObject cubePrefab;  // ������ ����
    public Transform[] homes;      // ����� ������ (����)
    public Transform[] waypoints;  // ����� ����� (������, �������� � �.�.)
    public float spawnInterval = 5f; // �������� ��������� �����
    public float spawnOffset = 3f;   // ������������ �������� ��� ������
    public float minDistanceBetweenCubes = 1.5f; // ����������� ��������� ����� ������

    private List<GameObject> spawnedCubes = new List<GameObject>(); // ������ ��� ������������ ���������� �����

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
        if (homes == null || homes.Length == 0)
        {
            Debug.LogError("����� ������ (homes) �� ��������� � ����������!");
            return;  // ���������� ���������� ������, ���� ����� ������ �� ���������
        }

        // �������� ��������� ��� ��� ������
        Transform home = homes[Random.Range(0, homes.Length)];

        if (home == null)
        {
            Debug.LogError("��������� ����� ������ (home) ����� null!");
            return;
        }

        // ������� ����� ��� � ������� ����
        GameObject newCube = Instantiate(cubePrefab, home.position, Quaternion.identity);

        // �������� ������ ���������� �����
        CubeAI cubeAI = newCube.GetComponent<CubeAI>();

        if (cubeAI == null)
        {
            Debug.LogError("CubeAI ������ �� ������ �� ����� ����!");
            return;
        }

        // ����������� ��������� ���� ��� ������ � ���������
        cubeAI.InitializeTasks(waypoints, home);
    }


    // ��������� ���������� �������� ��� ������
    Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-spawnOffset, spawnOffset),  // �������� �� ��� X
            0,                                       // �� ������� �� ��� Y
            Random.Range(-spawnOffset, spawnOffset)   // �������� �� ��� Z
        );
    }

    // ��������, ��� ����� ������� ���������� ������ �� ���� ������������ �����
    bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject cube in spawnedCubes)
        {
            if (Vector3.Distance(cube.transform.position, position) < minDistanceBetweenCubes)
            {
                return false; // ������� ������� ������ � ������� ����
            }
        }
        return true; // ������� �������
    }
}
