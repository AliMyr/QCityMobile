using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] waypoints;  // Точки маршрута для машин
    private int currentWaypointIndex = 0;

    public float detectionRange = 10f;  // Дистанция обнаружения NPC или игрока
    public LayerMask detectionLayer;    // Слой для обнаружения NPC и игрока
    public float slowSpeed = 2f;        // Замедленная скорость машины при обнаружении NPC/игрока
    public float normalSpeed = 10f;     // Обычная скорость машины

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        // Проверка, достигла ли машина текущей точки маршрута
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        // Обнаружение NPC или игрока
        DetectObstacles();
    }

    void DetectObstacles()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, detectionLayer))
        {
            if (hit.collider.CompareTag("NPC") || hit.collider.CompareTag("Player"))
            {
                agent.speed = slowSpeed;
            }
        }
        else
        {
            agent.speed = normalSpeed;
        }
    }
}
