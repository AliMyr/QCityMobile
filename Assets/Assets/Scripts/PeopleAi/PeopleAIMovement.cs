using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CubeAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform home;
    private Transform[] assignedTasks;  // Уникальные задачи для каждого объекта
    private int currentTaskIndex = 0;
    private bool allTasksComplete = false; // Проверка завершения всех задач
    private float workPauseTime = 5f;   // Пауза между задачами (время выполнения работы)
    private float restTime = 10f;       // Время отдыха
    public string[] disappearTaskNames; // Массив с именами задач, где объекты должны исчезать

    public float stoppingDistance = 1.0f;  // Расстояние остановки

    public Transform player;  // Ссылка на игрока
    public float detectionRadius = 5f;  // Радиус обнаружения игрока
    public float lookAtSpeed = 2f;      // Скорость, с которой ИИ поворачивается к игроку

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;  // Устанавливаем расстояние остановки

        // Если задачи уже назначены, запускаем выполнение
        if (assignedTasks != null && assignedTasks.Length > 0)
        {
            StartCoroutine(PerformTaskRoutine());
        }
    }

    // Метод для инициализации задач и дома (используется CubeManager)
    public void InitializeTasks(Transform[] taskPoints, Transform homePoint)
    {
        home = homePoint;  // Назначаем дом
        assignedTasks = taskPoints;  // Назначаем задачи
        Debug.Log("Назначено " + assignedTasks.Length + " задач.");

        // Начинаем выполнение задач
        StartCoroutine(PerformTaskRoutine());
    }

    // Основной корутин для выполнения задач
    IEnumerator PerformTaskRoutine()
    {
        for (int i = 0; i < assignedTasks.Length; i++)
        {
            currentTaskIndex = i;
            MoveToTask(assignedTasks[currentTaskIndex]);  // Переходим к следующей задаче

            // Ждем, пока бот не дойдет до цели или столкнется с препятствием
            yield return StartCoroutine(WaitUntilAgentReachesDestination());

            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Debug.Log("Задача " + (i + 1) + " выполнена.");
                yield return new WaitForSeconds(workPauseTime);  // Пауза на "работе"

                // Решаем, хочет ли объект отдохнуть после задачи
                if (ShouldRest())
                {
                    Debug.Log("Объект решил отдохнуть.");
                    yield return StartCoroutine(RestRoutine());
                }
            }
            else
            {
                Debug.Log("Путь заблокирован, объект ждёт.");
                yield return new WaitForSeconds(3f);  // Ждем перед новой попыткой
            }
        }

        // Все задачи выполнены, объект возвращается домой
        allTasksComplete = true;
        MoveToHome();

        // Ждем, пока объект вернется домой
        yield return new WaitUntil(() => agent.remainingDistance < agent.stoppingDistance && !agent.pathPending);
        Debug.Log("Объект вернулся домой.");

        // Исчезновение после завершения работы
        Disappear();
    }

    // Метод для перемещения к задаче
    void MoveToTask(Transform task)
    {
        if (task != null)
        {
            Debug.Log("Направляемся к задаче: " + task.name);
            agent.SetDestination(task.position);  // Задаем точку назначения
        }
        else
        {
            Debug.LogError("Задача не назначена!");
        }
    }

    // Метод для ожидания, пока агент не достигнет цели
    IEnumerator WaitUntilAgentReachesDestination()
    {
        // Ждем, пока агент достигнет цели или пока обнаружен игрок
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            ReactToPlayer();  // Проверка на игрока
            yield return null;  // Ждем следующий кадр
        }
    }

    // Метод для реакции на игрока
    void ReactToPlayer()
    {
        // Проверка, находится ли игрок в радиусе обнаружения
        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            // Поворачиваемся к игроку
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookAtSpeed);
        }
    }

    // Корутин для выполнения отдыха
    IEnumerator RestRoutine()
    {
        Debug.Log("Объект отдыхает в течение " + restTime + " секунд.");
        yield return new WaitForSeconds(restTime);
    }

    // Метод для проверки на необходимость отдыха
    bool ShouldRest()
    {
        return Random.Range(0f, 1f) < 0.2f;  // 20% шанс на отдых после каждой задачи
    }

    // Перемещение домой
    void MoveToHome()
    {
        if (home != null)
        {
            Debug.Log("Объект возвращается домой.");
            agent.SetDestination(home.position);  // Задаем дом как точку назначения
        }
        else
        {
            Debug.LogError("Дом для объекта не назначен!");
        }
    }

    // Исчезновение объекта
    void Disappear()
    {
        if (allTasksComplete)
        {
            Debug.Log("Объект завершил все задачи и исчезает.");
            Destroy(gameObject);  // Удаляем объект
        }
    }

    // Метод для поиска ближайшей задачи
    Transform FindClosestTask()
    {
        Transform closestTask = null;
        float shortestPath = Mathf.Infinity;

        foreach (Transform task in assignedTasks)
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(task.position, path);

            float pathLength = CalculatePathLength(path);

            if (path.status == NavMeshPathStatus.PathComplete && pathLength < shortestPath)
            {
                shortestPath = pathLength;
                closestTask = task;
            }
        }

        return closestTask;
    }

    // Метод для расчета длины пути
    float CalculatePathLength(NavMeshPath path)
    {
        float length = 0f;
        if (path.corners.Length < 2) return length;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }
}
