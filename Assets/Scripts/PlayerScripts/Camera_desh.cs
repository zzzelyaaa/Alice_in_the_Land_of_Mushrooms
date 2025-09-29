using System.Collections.Generic;
using UnityEngine;

public class Camera_desh : MonoBehaviour
{
    [Header("������� ���")]
    [SerializeField] private GameObject cube;
    [Header("Path Settings")]
    [SerializeField] private List<Transform> pathPoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float reachThreshold = 0.1f;

    [Header("Intro Completion")]
    [SerializeField] private bool introCompleted = false;

    private int currentTargetIndex = 0;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = transform;
        if (pathPoints.Count == 0)
        {
            Debug.LogWarning("Path points list is empty! Add target positions in the inspector.");
        }
    }

    void Update()
    {
        HandleInput();

        if (isMoving)
        {
            MoveCamera();
        }
    }

    void HandleInput()
    {
        // �������� ����� �����
        if (Input.GetMouseButtonDown(0))
        {
            ProcessCameraMovement();
        }

        // �������� ���� �� ��������� �����������
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ProcessCameraMovement();
        }
    }

    void ProcessCameraMovement()
    {
        if (!isMoving && currentTargetIndex < pathPoints.Count && !introCompleted)
        {
            PrepareMovementToNextPoint();
        }
    }

    void PrepareMovementToNextPoint()
    {
        // ��������� ������� ��������� ����� ��������
        Vector3 targetPos = pathPoints[currentTargetIndex].position;
        Vector3 direction = targetPos - cameraTransform.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();
            targetPosition = targetPos - direction * stopDistance;
        }
        else
        {
            targetPosition = targetPos;
        }

        isMoving = true;
    }

    void MoveCamera()
    {
        // ������� ����������� ������ � ������� �������
        cameraTransform.position = Vector3.MoveTowards(
            cameraTransform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // ������� �������� ������ � ������� �������
        Vector3 directionToTarget = targetPosition - cameraTransform.position;
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        if (currentTargetIndex == pathPoints.Count - 1) reachThreshold = 30f;
        // �������� ���������� ������� �������
        if (Vector3.Distance(cameraTransform.position, targetPosition) <= reachThreshold)
        {
            CompleteMovementToCurrentPoint();
        }
    }

    void CompleteMovementToCurrentPoint()
    {
        isMoving = false;

        // ������� � ��������� �����
        currentTargetIndex++;

        // �������� ���������� �����
        if (currentTargetIndex >= pathPoints.Count)
        {
            CompleteIntro();
        }
    }

    void CompleteIntro()
    {
        introCompleted = true;
        CameraController.canMove = true;
        cube.SetActive(false);
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] != null)
            {
                pathPoints[i].gameObject.SetActive(false);
            }
        }
    }
}
