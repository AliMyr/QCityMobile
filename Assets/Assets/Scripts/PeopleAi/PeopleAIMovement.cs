using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    public Transform player;  // ������ �� ������
    public float detectionRadius = 5f;  // ������ ����������� ������
    public float lookAtSpeed = 2f;      // ��������, � ������� �� �������������� � ������

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
    public void InitializeTasks(Transform[] taskPoints, Transform homePoint)
    {
        home = homePoint;  // ��������� ���
        assignedTasks = taskPoints;  // ��������� ������
        Debug.Log("��������� " + assignedTasks.Length + " �����.");

        // �������� ���������� �����
        StartCoroutine(PerformTaskRoutine());
    }

    // �������� ������� ��� ���������� �����
    IEnumerator PerformTaskRoutine()
    {
        for (int i = 0; i < assignedTasks.Length; i++)
        {
            currentTaskIndex = i;
            MoveToTask(assignedTasks[currentTaskIndex]);  // ��������� � ��������� ������

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

    // ����� ��� ����������� � ������
    void MoveToTask(Transform task)
    {
        if (task != null)
        {
            Debug.Log("������������ � ������: " + task.name);
            agent.SetDestination(task.position);  // ������ ����� ����������
        }
        else
        {
            Debug.LogError("������ �� ���������!");
        }
    }

    // ����� ��� ��������, ���� ����� �� ��������� ����
    IEnumerator WaitUntilAgentReachesDestination()
    {
        // ����, ���� ����� ��������� ���� ��� ���� ��������� �����
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            ReactToPlayer();  // �������� �� ������
            yield return null;  // ���� ��������� ����
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

    // ����� ��� ������ ��������� ������
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

    // ����� ��� ������� ����� ����
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
