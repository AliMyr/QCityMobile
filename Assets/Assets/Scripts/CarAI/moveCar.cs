using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] waypoints;  // ����� �������� ��� �����
    private int currentWaypointIndex = 0;

    public float detectionRange = 10f;  // ��������� ����������� NPC ��� ������
    public LayerMask detectionLayer;    // ���� ��� ����������� NPC � ������
    public float slowSpeed = 2f;        // ����������� �������� ������ ��� ����������� NPC/������
    public float normalSpeed = 10f;     // ������� �������� ������

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
        // ��������, �������� �� ������ ������� ����� ��������
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        // ����������� NPC ��� ������
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
