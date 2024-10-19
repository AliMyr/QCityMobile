using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class CubeManager : MonoBehaviour
{
    public GameObject cubePrefab;  // Префаб куба
    public Transform[] homes;      // Точки спавна (дома)
    public Transform[] waypoints;  // Точки целей (работы, магазины и т.д.)
    public float spawnInterval = 5f; // Интервал появления кубов
    public float spawnOffset = 3f;   // Максимальное смещение при спавне
    public float minDistanceBetweenCubes = 1.5f; // Минимальная дистанция между кубами

    private List<GameObject> spawnedCubes = new List<GameObject>(); // Список для отслеживания спавненных кубов

    void Start()
    {
        // Запускаем корутин для периодического спавна кубов
        StartCoroutine(SpawnCubes());
    }

    IEnumerator SpawnCubes()
    {
        while (true)
        {
            // Спавн куба с случайными параметрами
            SpawnRandomCube();

            // Ждем перед следующим спавном
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Метод для спавна куба с случайными параметрами
    void SpawnRandomCube()
    {
        if (homes == null || homes.Length == 0)
        {
            Debug.LogError("Точки спавна (homes) не назначены в инспекторе!");
            return;  // Прекращаем выполнение метода, если точки спавна не назначены
        }

        // Выбираем случайный дом для спавна
        Transform home = homes[Random.Range(0, homes.Length)];

        if (home == null)
        {
            Debug.LogError("Выбранная точка спавна (home) равна null!");
            return;
        }

        // Создаем новый куб в позиции дома
        GameObject newCube = Instantiate(cubePrefab, home.position, Quaternion.identity);

        // Получаем скрипт управления кубом
        CubeAI cubeAI = newCube.GetComponent<CubeAI>();

        if (cubeAI == null)
        {
            Debug.LogError("CubeAI скрипт не найден на новом кубе!");
            return;
        }

        // Присваиваем случайные цели для работы и блуждания
        cubeAI.InitializeTasks(waypoints, home);
    }


    // Генерация случайного смещения для спавна
    Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-spawnOffset, spawnOffset),  // Смещение по оси X
            0,                                       // Не смещаем по оси Y
            Random.Range(-spawnOffset, spawnOffset)   // Смещение по оси Z
        );
    }

    // Проверка, что новая позиция достаточно далеко от всех существующих кубов
    bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject cube in spawnedCubes)
        {
            if (Vector3.Distance(cube.transform.position, position) < minDistanceBetweenCubes)
            {
                return false; // Позиция слишком близко к другому кубу
            }
        }
        return true; // Позиция валидна
    }
}
