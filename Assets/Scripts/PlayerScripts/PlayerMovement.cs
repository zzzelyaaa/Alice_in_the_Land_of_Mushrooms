using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public VariableJoystick variableJoystick;
    [SerializeField] private float _speed;

    private Vector2 movementAmount;
    private Vector2 moveDirection;
    private Vector2 currentMovement;
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    private float _inertia = 0.1f;
    public SpriteRenderer spriteRenderer;
    public Sprite Face;
    public Sprite Ass;


    private void MoveInertion()
    {
        movementAmount = Vector2.up * variableJoystick.Vertical + Vector2.right * variableJoystick.Horizontal;
        currentMovement.x = movementAmount.x;
        currentMovement.y = movementAmount.y;

        // Двигает персонажа
        currentInputVector = Vector2.SmoothDamp(currentInputVector, movementAmount, ref smoothInputVelocity, _inertia);
        moveDirection = new Vector2(currentInputVector.x, currentInputVector.y);

        if(moveDirection.sqrMagnitude>0.3) transform.Translate(moveDirection * _speed * Time.deltaTime);
        if (moveDirection.y > 0f)
        {
            spriteRenderer.sprite = Ass;
        }
        else
        {
            spriteRenderer.sprite = Face;
        }
    }

    private void Update()
    {

        MoveInertion();
    }
}
