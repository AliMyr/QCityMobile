using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class CubeAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform home;
    private Transform[] assignedTasks;  // ���������� ������ ��� ������� �������
    private int currentTaskIndex = 0;
    private bool allTasksComplete = false; // �������� ���������� ���� �����
    private float workPauseTime = 5f;   // ����� ����� �������� (����� ���������� ������)
    private float restTime = 10f;       // ����� ������
    public string[] disappearTaskNames; // ������ � ������� �����, ��� ������� ������ ��������

    public float stoppingDistance = 1.0f;  // ���������� ���������
    public float deviationRange = 0.5f;    // �������� ��������� ���������� �� ����
    public float deviationAmount = 1.0f;   // ������������ �������� ���������� ��� ���������������� ��������
    public float deviationFrequency = 0.5f; // ��� ����� ������ ����� ������ ����������� � �������
    public float zigzagDuration = 3f;      // ��� ����� ����� ������� ������
    public float straightDuration = 5f;    // ��� ����� ����� ��������� ����� ����� ������� ������ �������

    private bool isZigzagging = false;     // ����������, ������������, ��� ������ ���� ������

    public Transform player;  // ������ �� ������
    public float detectionRadius = 5f;  // ������ ����������� ������
    public float lookAtSpeed = 2f;      // ��������, � ������� �� �������������� � ������

    public LayerMask carLayer; // ����, �� ������� ��������� ������
    public float avoidDistance = 3f; // ��������� ��� ����������� �����
    public float avoidSpeed = 2f; // �������� ��������� �� �����

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;  // ������������� ���������� ���������

        // ���� ������ ��� ���������, ��������� ����������
        if (assignedTasks != null && assignedTasks.Length > 0)
        {
            StartCoroutine(PerformTaskRoutine());
        }
    }

    // ����� ��� ������������� ����� � ���� (������������ CubeManager)
    public void InitializeTasks(Transform[] allTasks, Transform homePoint)
    {
        if (allTasks == null || allTasks.Length == 0)
        {
            Debug.LogError("������ ����� (allTasks) ���� ��� �� ��������.");
            return;
        }

        if (homePoint == null)
        {
            Debug.LogError("����� ���� (homePoint) �� ���������.");
            return;
        }

        home = homePoint;  // ��������� ���

        // ���������� ���������� ����� (��������, �� 2 �� ������������ �������� ����� 6 � ������ allTasks)
        int numTasks = Mathf.Min(Random.Range(2, 6), allTasks.Length);

        // ������� ������ ��� �������� ���������� �����
        List<Transform> availableTasks = new List<Transform>(allTasks);
        assignedTasks = new Transform[numTasks];

        // ��������� ��������� ������ �� ������ ������
        for (int i = 0; i < numTasks; i++)
        {
            if (availableTasks.Count == 0)
            {
                Debug.LogError("��� ��������� ����� ��� ����������.");
                break;
            }

            int randomIndex = Random.Range(0, availableTasks.Count);
            assignedTasks[i] = availableTasks[randomIndex];
            availableTasks.RemoveAt(randomIndex);  // ������� ����������� ������ �� ���������, ����� �������� ������������
        }

        Debug.Log("��������� " + assignedTasks.Length + " ���������� �����.");

        // ��������� ���������� �����
        StartCoroutine(PerformTaskRoutine());
    }

    // �������� ������� ��� ���������� �����
    IEnumerator PerformTaskRoutine()
    {
        for (int i = 0; i < assignedTasks.Length; i++)
        {
            currentTaskIndex = i;
            StartCoroutine(HandleZigZag(assignedTasks[currentTaskIndex]));  // ��������� ��������

            // ����, ���� ��� �� ������ �� ���� ��� ���������� � ������������
            yield return StartCoroutine(WaitUntilAgentReachesDestination());

            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Debug.Log("������ " + (i + 1) + " ���������.");
                yield return new WaitForSeconds(workPauseTime);  // ����� �� "������"

                // ������, ����� �� ������ ��������� ����� ������
                if (ShouldRest())
                {
                    Debug.Log("������ ����� ���������.");
                    yield return StartCoroutine(RestRoutine());
                }
            }
            else
            {
                Debug.Log("���� ������������, ������ ���.");
                yield return new WaitForSeconds(3f);  // ���� ����� ����� ��������
            }
        }

        // ��� ������ ���������, ������ ������������ �����
        allTasksComplete = true;
        MoveToHome();

        // ����, ���� ������ �������� �����
        yield return new WaitUntil(() => agent.remainingDistance < agent.stoppingDistance && !agent.pathPending);
        Debug.Log("������ �������� �����.");

        // ������������ ����� ���������� ������
        Disappear();
    }

    // ������� ��� ���������� ������������ ������� � ������� ��������
    IEnumerator HandleZigZag(Transform task)
    {
        while (Vector3.Distance(transform.position, task.position) > agent.stoppingDistance)
        {
            // ��������� �����
            isZigzagging = false;
            agent.SetDestination(task.position);
            yield return new WaitForSeconds(straightDuration); // ��������� ����� ������������ �����

            // �������� ������
            isZigzagging = true;
            yield return StartCoroutine(ZigZagMovement(task.position)); // �������� ������ �� ��������� �����

            // ������������ � ������� �������� ����� �������
            isZigzagging = false;
        }
    }

    // ������� ��� ���������������� ��������
    IEnumerator ZigZagMovement(Vector3 destination)
    {
        float zigzagEndTime = Time.time + zigzagDuration;  // ������������� ����� ��������� �������

        while (Time.time < zigzagEndTime && isZigzagging)
        {
            // ��������� ��������� �������� ����� ��� ������
            Vector3 deviation = new Vector3(Random.Range(-deviationAmount, deviationAmount), 0, Random.Range(-deviationAmount, deviationAmount));

            // ������ ����� ����� ���������� � �����������
            Vector3 zigzagTarget = destination + deviation;

            // ������������� ����� ����� ����������
            agent.SetDestination(zigzagTarget);

            // ���� ������� ����� ��������� ���������� �����������
            yield return new WaitForSeconds(deviationFrequency);
        }
    }

    // ����� ��� ��������, ���� ����� �� ��������� ����
    IEnumerator WaitUntilAgentReachesDestination()
    {
        // ����, ���� ����� ��������� ���� ��� ���� ��������� �����
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            AvoidCars();  // �������� � ��������� �� �����
            ReactToPlayer();  // �������� �� ������
            yield return null;  // ���� ��������� ����
        }
    }

    // ����� ��� ��������� ������������ � ��������
    void AvoidCars()
    {
        RaycastHit hit;
        int numRays = 8; // ���������� ����� ��� "������"
        float angleStep = 360f / numRays; // ��� ���� ����� ������
        bool carDetected = false;
        Vector3 avoidDirection = Vector3.zero;

        // ���� ��� �������� ����� �� ��� �������
        for (int i = 0; i < numRays; i++)
        {
            // ������������ ����������� ���� ��� �����
            Vector3 rayDirection = Quaternion.Euler(0, angleStep * i, 0) * transform.forward;

            // ���������� ���� ��� �������
            Debug.DrawRay(transform.position, rayDirection * avoidDistance, Color.red);

            // ���������� ���
            if (Physics.Raycast(transform.position, rayDirection, out hit, avoidDistance, carLayer))
            {
                Debug.Log("���������� ������: " + hit.collider.gameObject.name);
                avoidDirection = transform.position - hit.point; // ����������� ��������� � �� ������
                carDetected = true;
                break; // ��������� ����, ���� ���������� ������
            }
        }

        // ���� ������ ����������, ������ ����������� ��������
        if (carDetected)
        {
            avoidDirection.y = 0; // ������� ������� ��� Y
            avoidDirection.Normalize(); // ����������� ������ �����������

            // ��������� ����� ���� ��� ���������, ���������� � ������� �� ������
            Vector3 newDestination = transform.position + avoidDirection * avoidDistance;

            // ������ �������� ����������� �������� AI
            agent.SetDestination(newDestination);
        }
    }

    // ����� ��� ������� �� ������
    void ReactToPlayer()
    {
        // ��������, ��������� �� ����� � ������� �����������
        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            // �������������� � ������
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookAtSpeed);
        }
    }

    // ������� ��� ���������� ������
    IEnumerator RestRoutine()
    {
        Debug.Log("������ �������� � ������� " + restTime + " ������.");
        yield return new WaitForSeconds(restTime);
    }

    // ����� ��� �������� �� ������������� ������
    bool ShouldRest()
    {
        return Random.Range(0f, 1f) < 0.2f;  // 20% ���� �� ����� ����� ������ ������
    }

    // ����������� �����
    void MoveToHome()
    {
        if (home != null)
        {
            Debug.Log("������ ������������ �����.");
            agent.SetDestination(home.position);  // ������ ��� ��� ����� ����������
        }
        else
        {
            Debug.LogError("��� ��� ������� �� ��������!");
        }
    }

    // ������������ �������
    void Disappear()
    {
        if (allTasksComplete)
        {
            Debug.Log("������ �������� ��� ������ � ��������.");
            Destroy(gameObject);  // ������� ������
        }
    }
}
