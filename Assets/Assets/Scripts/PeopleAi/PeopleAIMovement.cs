using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

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
    public float deviationRange = 0.5f;    // Диапазон случайных отклонений от пути
    public float deviationAmount = 1.0f;   // Максимальная величина отклонения для зигзагообразного движения
    public float deviationFrequency = 0.5f; // Как часто объект будет менять направление в зигзаге
    public float zigzagDuration = 3f;      // Как долго будет длиться зигзаг
    public float straightDuration = 5f;    // Как долго будет двигаться прямо перед началом нового зигзага

    private bool isZigzagging = false;     // Переменная, показывающая, что сейчас идет зигзаг

    public Transform player;  // Ссылка на игрока
    public float detectionRadius = 5f;  // Радиус обнаружения игрока
    public float lookAtSpeed = 2f;      // Скорость, с которой ИИ поворачивается к игроку

    public LayerMask carLayer; // Слой, на котором находятся машины
    public float avoidDistance = 3f; // Дистанция для обнаружения машин
    public float avoidSpeed = 2f; // Скорость уклонения от машин

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
    public void InitializeTasks(Transform[] allTasks, Transform homePoint)
    {
        if (allTasks == null || allTasks.Length == 0)
        {
            Debug.LogError("Массив задач (allTasks) пуст или не назначен.");
            return;
        }

        if (homePoint == null)
        {
            Debug.LogError("Точка дома (homePoint) не назначена.");
            return;
        }

        home = homePoint;  // Назначаем дом

        // Определяем количество задач (например, от 2 до минимального значения между 6 и длиной allTasks)
        int numTasks = Mathf.Min(Random.Range(2, 6), allTasks.Length);

        // Создаем список для хранения уникальных задач
        List<Transform> availableTasks = new List<Transform>(allTasks);
        assignedTasks = new Transform[numTasks];

        // Назначаем случайные задачи из общего списка
        for (int i = 0; i < numTasks; i++)
        {
            if (availableTasks.Count == 0)
            {
                Debug.LogError("Нет доступных задач для назначения.");
                break;
            }

            int randomIndex = Random.Range(0, availableTasks.Count);
            assignedTasks[i] = availableTasks[randomIndex];
            availableTasks.RemoveAt(randomIndex);  // Удаляем назначенную задачу из доступных, чтобы избежать дублирования
        }

        Debug.Log("Назначено " + assignedTasks.Length + " уникальных задач.");

        // Запускаем выполнение задач
        StartCoroutine(PerformTaskRoutine());
    }

    // Основной корутин для выполнения задач
    IEnumerator PerformTaskRoutine()
    {
        for (int i = 0; i < assignedTasks.Length; i++)
        {
            currentTaskIndex = i;
            StartCoroutine(HandleZigZag(assignedTasks[currentTaskIndex]));  // Управляем зигзагом

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

    // Корутин для управления чередованием зигзага и прямого движения
    IEnumerator HandleZigZag(Transform task)
    {
        while (Vector3.Distance(transform.position, task.position) > agent.stoppingDistance)
        {
            // Двигаемся прямо
            isZigzagging = false;
            agent.SetDestination(task.position);
            yield return new WaitForSeconds(straightDuration); // Двигаемся прямо определенное время

            // Начинаем зигзаг
            isZigzagging = true;
            yield return StartCoroutine(ZigZagMovement(task.position)); // Включаем зигзаг на некоторое время

            // Возвращаемся к прямому движению после зигзага
            isZigzagging = false;
        }
    }

    // Корутин для зигзагообразного движения
    IEnumerator ZigZagMovement(Vector3 destination)
    {
        float zigzagEndTime = Time.time + zigzagDuration;  // Устанавливаем время окончания зигзага

        while (Time.time < zigzagEndTime && isZigzagging)
        {
            // Вычисляем случайное смещение влево или вправо
            Vector3 deviation = new Vector3(Random.Range(-deviationAmount, deviationAmount), 0, Random.Range(-deviationAmount, deviationAmount));

            // Задаем новую точку назначения с отклонением
            Vector3 zigzagTarget = destination + deviation;

            // Устанавливаем новую точку назначения
            agent.SetDestination(zigzagTarget);

            // Ждем немного перед следующим изменением направления
            yield return new WaitForSeconds(deviationFrequency);
        }
    }

    // Метод для ожидания, пока агент не достигнет цели
    IEnumerator WaitUntilAgentReachesDestination()
    {
        // Ждем, пока агент достигнет цели или пока обнаружен игрок
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            AvoidCars();  // Проверка и уклонение от машин
            ReactToPlayer();  // Проверка на игрока
            yield return null;  // Ждем следующий кадр
        }
    }

    // Метод для избегания столкновений с машинами
    void AvoidCars()
    {
        RaycastHit hit;
        int numRays = 8; // Количество лучей для "звезды"
        float angleStep = 360f / numRays; // Шаг угла между лучами
        bool carDetected = false;
        Vector3 avoidDirection = Vector3.zero;

        // Цикл для отправки лучей во все стороны
        for (int i = 0; i < numRays; i++)
        {
            // Рассчитываем направление луча под углом
            Vector3 rayDirection = Quaternion.Euler(0, angleStep * i, 0) * transform.forward;

            // Отображаем лучи для отладки
            Debug.DrawRay(transform.position, rayDirection * avoidDistance, Color.red);

            // Отправляем луч
            if (Physics.Raycast(transform.position, rayDirection, out hit, avoidDistance, carLayer))
            {
                Debug.Log("Обнаружена машина: " + hit.collider.gameObject.name);
                avoidDirection = transform.position - hit.point; // Направление уклонения — от машины
                carDetected = true;
                break; // Прерываем цикл, если обнаружена машина
            }
        }

        // Если машина обнаружена, меняем направление движения
        if (carDetected)
        {
            avoidDirection.y = 0; // Убираем влияние оси Y
            avoidDirection.Normalize(); // Нормализуем вектор направления

            // Вычисляем новую цель для уклонения, отклоняясь в сторону от машины
            Vector3 newDestination = transform.position + avoidDirection * avoidDistance;

            // Плавно изменяем направление движения AI
            agent.SetDestination(newDestination);
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
}
