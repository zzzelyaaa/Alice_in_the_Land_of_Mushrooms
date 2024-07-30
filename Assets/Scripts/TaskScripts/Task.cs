using UnityEngine;
using UnityEngine.UI;

public enum TaskType
{
    Art,
    GD,
    PR,
    QA,
    JokerShit,
    JokerCrit
}



public class Task : MonoBehaviour
{
    public TaskType Type;
    [SerializeField] Image _conditionImage;
    private float complexityValue = 1;
    public float TimeToCompleted;
    public float StartTimeToCompleted;
    float time;
    float fill;

    TaskManager manager;

    bool isWorking = false;

    public void Initialize(float Complexity, TaskManager taskManager)
    {
        manager = taskManager;
        complexityValue = Complexity;
        TimeToCompleted = StartTimeToCompleted * complexityValue;
        time = TimeToCompleted;
        fill = (-1 / TimeToCompleted) * time + 1;
        _conditionImage.fillAmount = fill;
    }

    private void Update()
    {
        if (isWorking)
        {
            time -= Time.deltaTime;
            fill = (-1 / TimeToCompleted) * time + 1;
            _conditionImage.fillAmount = fill;
        }
        else
        {
            if (time < TimeToCompleted)
            {
                time += Time.deltaTime;
                fill = (-1 / TimeToCompleted) * time + 1;
                _conditionImage.fillAmount = fill;
            }
        }

        if (time < 0f)
        {
            fill = 0f;
            time = 0f;
            WorkComplite();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player)
        {
            isWorking = true;
            Debug.Log("����� ����� ��������");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player)
        {
            isWorking = false;
            Debug.Log("����� ������");
        }
    }

    private void WorkComplite()
    {
        switch (Type)
        {
            case TaskType.Art:
                Debug.Log("���� ��� ��������");
                manager.TaskCompleted(Type);
                Destroy(gameObject);
                break;
            case TaskType.GD:
                Debug.Log("���� �� ��������");
                manager.TaskCompleted(Type);
                Destroy(gameObject);
                break;
            case TaskType.PR:
                Debug.Log("���� �� ��������");
                manager.TaskCompleted(Type);
                Destroy(gameObject);
                break;
            case TaskType.QA:
                Debug.Log("���� ��� ��������");
                manager.TaskCompleted(Type);
                Destroy(gameObject);
                break;
            case TaskType.JokerShit:
                Debug.Log("���� ?? ��������");
                manager.TaskCompleted(Type);
                Destroy(gameObject);
                break;
            case TaskType.JokerCrit:
                Debug.Log("���� ?? ��������");
                manager.TaskCompleted(Type);
                Destroy(gameObject);
                break;
            default: break;
        }
    }
}
