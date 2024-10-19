using UnityEngine;
using UnityEngine.AI;

public class TruckAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform parkingSpot;  // ����� ��������
    public Transform[] deliveryPoints;  // ����� ��� �������� (A, B, C � �.�.)
    private Transform currentDestination;  // ������� ����� ����������
    private int currentDeliveryIndex = 0;  // ������ ������� ��������

    public float detectionRange = 10f;  // ������ ����������� �����������
    public LayerMask detectionLayer;    // ���� ��� ����������� NPC � �������
    public bool isDelivering = true;    // � �������� �������� ��� ����������� �� ��������

    public float stopDistance = 1.5f;  // ����������, �� ������� �������� ��������������� � ����
    public TrafficLight currentTrafficLight;  // ������� ��������

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartDeliveryCycle();  // �������� ���� ��������
    }

    void Update()
    {
        HandleMovement();

        // �������� �� ��������
        if (currentTrafficLight != null && !currentTrafficLight.IsGreenLight())
        {
            agent.isStopped = true;  // ��������� �� ������� ����
        }
        else
        {
            agent.isStopped = false;  // ���������� ��������
        }

        // ����������� NPC, ������ ��� ������ �����
        DetectObstacles();
    }

    void HandleMovement()
    {
        // ������� ���������� ��� ���������
        if (agent.remainingDistance < stopDistance)
        {
            agent.speed = Mathf.Lerp(agent.speed, 0, Time.deltaTime * 2f);  // ������� ���������� ��� ���������
        }
        else if (agent.remainingDistance < 5f)  // ���� ���� ������������ � ��������
        {
            agent.speed = Mathf.Lerp(agent.speed, 3f, Time.deltaTime * 2f);  // ��������� �� ���������
        }
        else
        {
            agent.speed = Mathf.Lerp(agent.speed, 10f, Time.deltaTime * 2f);  // ��������������� �������� �� ������ ��������
        }

        // ���������, �������� �� ���� ������ ����������
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


    // ������������� ��������� ����� �������� �� �������
    void SetNextDeliveryDestination()
    {
        if (deliveryPoints.Length > 0)
        {
            currentDestination = deliveryPoints[currentDeliveryIndex];
            agent.SetDestination(currentDestination.position);
            Debug.Log("���� ������������ � ����� ��������: " + currentDestination.name);
        }
    }

    // �������� ���� ��������: ��������� ������ ����� ��������
    void StartDeliveryCycle()
    {
        if (deliveryPoints.Length > 0)
        {
            currentDeliveryIndex = 0;
            isDelivering = true;
            SetNextDeliveryDestination();  // ������������� ������ ����� ��������
        }
    }

    // ����������� ����������� (NPC, ������, ������ ������)
    void DetectObstacles()
    {
        RaycastHit hit;

        // �������� �� ������� ����������� ����� �������
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, detectionLayer))
        {
            if (hit.collider.CompareTag("NPC") || hit.collider.CompareTag("Player") || hit.collider.CompareTag("Car"))
            {
                // ���� ���� ���������� NPC ��� ������� ������, ��������� ��������
                agent.speed = 2f;
                Debug.Log("���� �����������, ��������� NPC ��� �����.");
            }
        }
        else
        {
            // ��������������� ���������� ��������, ���� ���� ��������
            agent.speed = 10f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ���� ���� ������ � ���� ���������, ����������� ������� ��������
        if (other.CompareTag("TrafficLight"))
        {
            currentTrafficLight = other.GetComponent<TrafficLight>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // ��� ������ ���� �������� ���� ���������, ���������� ��������
        if (other.CompareTag("TrafficLight"))
        {
            currentTrafficLight = null;
        }
    }
}
