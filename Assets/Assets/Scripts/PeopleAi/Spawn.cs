using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CubeManager : MonoBehaviour
{
    public GameObject cubePrefab;  // Префаб куба
    public Transform[] homes;      // Точки спавна (дома)
    public Transform[] waypoints;  // Точки целей (работы, магазины и т.д.)
    public float spawnInterval = 5f; // Интервал появления кубов

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
        // Выбираем случайный дом для спавна
        Transform home = homes[Random.Range(0, homes.Length)];

        // Создаем новый куб в позиции дома
        GameObject newCube = Instantiate(cubePrefab, home.position, Quaternion.identity);

        // Получаем скрипт управления кубом
        CubeAI cubeAI = newCube.GetComponent<CubeAI>();

        // Присваиваем случайные цели для работы и блуждания
        cubeAI.InitializeTasks(waypoints, home);
    }
}
