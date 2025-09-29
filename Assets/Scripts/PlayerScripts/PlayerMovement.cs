using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    public VariableJoystick variableJoystick;

    [Header("Movement Settings")]
    [SerializeField] private float _baseSpeed = 5f;   // ������� ��������
    [SerializeField] private float _inertia = 0.1f;

    [Header("Rendering")]
    public SpriteRenderer spriteRenderer;
    public Sprite Face;
    public Sprite Ass;

    [Header("World Layers")]
    public Tilemap sandLayer;
    public Tilemap grassLayer;
    public Tilemap mountainLayer;

    private Vector2 movementAmount;
    private Vector2 moveDirection;
    private Vector2 currentMovement;
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;

    private void Update()
    {
        if (!CameraController.canMove) return;
        MoveInertia();
    }

    private void MoveInertia()
    {
        // ��������� ��������
        movementAmount = Vector2.up * variableJoystick.Vertical + Vector2.right * variableJoystick.Horizontal;
        currentMovement.x = movementAmount.x;
        currentMovement.y = movementAmount.y;

        // ������� �������
        currentInputVector = Vector2.SmoothDamp(currentInputVector, movementAmount, ref smoothInputVelocity, _inertia);
        moveDirection = new Vector2(currentInputVector.x, currentInputVector.y);

        if (moveDirection.sqrMagnitude > 0.3f)
        {
            // ����� ������� ����������� �������� �� ����
            float tileSpeedMultiplier = GetSpeedMultiplierAtPosition(transform.position);

            // ������� ������ � ������ ������������
            transform.Translate(moveDirection * (_baseSpeed * tileSpeedMultiplier) * Time.deltaTime);
        }

        // ����� �������
        if (moveDirection.y > 0f)
        {
            spriteRenderer.sprite = Ass;
        }
        else
        {
            spriteRenderer.sprite = Face;
        }
    }

    private float GetSpeedMultiplierAtPosition(Vector3 worldPos)
    {
        Vector3Int cellPos;

        // ��������� ���� � ������
        if (sandLayer != null)
        {
            cellPos = sandLayer.WorldToCell(worldPos);
            if (sandLayer.GetTile(cellPos) != null)
                return 0.75f; // -25% ��������
        }

        // ��������� ���� � ������
        if (grassLayer != null)
        {
            cellPos = grassLayer.WorldToCell(worldPos);
            if (grassLayer.GetTile(cellPos) != null)
                return 1f; // ��� ������
        }

        // ��������� ���� � ������
        if (mountainLayer != null)
        {
            cellPos = mountainLayer.WorldToCell(worldPos);
            if (mountainLayer.GetTile(cellPos) != null)
                return 0.25f; // -75% ��������
        }

        return 1f; // ������ ���� �� ����� ����
    }
}
