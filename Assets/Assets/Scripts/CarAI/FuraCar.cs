using UnityEngine;
using UnityEngine.AI;

public class TruckAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform parkingSpot;  // Точка парковки
    public Transform[] deliveryPoints;  // Точки для доставки (A, B, C и т.д.)
    private Transform currentDestination;  // Текущая точка назначения
    private int currentDeliveryIndex = 0;  // Индекс текущей доставки

    public float detectionRange = 10f;  // Радиус обнаружения препятствий
    public LayerMask detectionLayer;    // Слой для обнаружения NPC и игроков
    public bool isDelivering = true;    // В процессе доставки или возвращения на парковку

    public float stopDistance = 1.5f;  // Расстояние, на котором грузовик останавливается у цели
    public TrafficLight currentTrafficLight;  // Текущий светофор

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartDeliveryCycle();  // Начинаем цикл доставки
    }

    void Update()
    {
        HandleMovement();

        // Проверка на светофор
        if (currentTrafficLight != null && !currentTrafficLight.IsGreenLight())
        {
            agent.isStopped = true;  // Остановка на красный свет
        }
        else
        {
            agent.isStopped = false;  // Продолжаем движение
        }

        // Обнаружение NPC, игрока или других машин
        DetectObstacles();
    }

    void HandleMovement()
    {
        // Плавное замедление при поворотах
        if (agent.remainingDistance < stopDistance)
        {
            agent.speed = Mathf.Lerp(agent.speed, 0, Time.deltaTime * 2f);  // Плавное замедление при остановке
        }
        else if (agent.remainingDistance < 5f)  // Если фура приближается к повороту
        {
            agent.speed = Mathf.Lerp(agent.speed, 3f, Time.deltaTime * 2f);  // Замедляем на поворотах
        }
        else
        {
            agent.speed = Mathf.Lerp(agent.speed, 10f, Time.deltaTime * 2f);  // Восстанавливаем скорость на прямых участках
        }

        // Проверяем, достигла ли фура пункта назначения
        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            if (isDelivering)
            {
                currentDeliveryIndex++;
                if (currentDeliveryIndex < deliveryPoints.Length)
                {
                    SetNextDeliveryDestination();
                }
                else
                {
                    isDelivering = false;
                    currentDestination = parkingSpot;
                    agent.SetDestination(currentDestination.position);
                }
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }


    // Устанавливаем следующую точку доставки по порядку
    void SetNextDeliveryDestination()
    {
        if (deliveryPoints.Length > 0)
        {
            currentDestination = deliveryPoints[currentDeliveryIndex];
            agent.SetDestination(currentDestination.position);
            Debug.Log("Фура направляется к точке доставки: " + currentDestination.name);
        }
    }

    // Начинаем цикл доставки: назначаем первую точку доставки
    void StartDeliveryCycle()
    {
        if (deliveryPoints.Length > 0)
        {
            currentDeliveryIndex = 0;
            isDelivering = true;
            SetNextDeliveryDestination();  // Устанавливаем первую точку доставки
        }
    }

    // Обнаружение препятствий (NPC, игроки, другие машины)
    void DetectObstacles()
    {
        RaycastHit hit;

        // Проверка на наличие препятствий перед машиной
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, detectionLayer))
        {
            if (hit.collider.CompareTag("NPC") || hit.collider.CompareTag("Player") || hit.collider.CompareTag("Car"))
            {
                // Если фура обнаружила NPC или другого игрока, замедляем скорость
                agent.speed = 2f;
                Debug.Log("Фура замедляется, обнаружен NPC или игрок.");
            }
        }
        else
        {
            // Восстанавливаем нормальную скорость, если путь свободен
            agent.speed = 10f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Если фура входит в зону светофора, присваиваем текущий светофор
        if (other.CompareTag("TrafficLight"))
        {
            currentTrafficLight = other.GetComponent<TrafficLight>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Как только фура покидает зону светофора, продолжаем движение
        if (other.CompareTag("TrafficLight"))
        {
            currentTrafficLight = null;
        }
    }
}
