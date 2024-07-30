using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TaskManager : MonoBehaviour
{
    private string sprintId = "sprint_{sprintNum}";
    [SerializeField] TMP_Text m_SprintIDText;

    [Header("Номер спринта")]
    [SerializeField] private int sprintNum = 1;
    [Header("Настройки времени прохождения спринта")]
    public float TimerToSprint = 120f;
    [SerializeField] TMP_Text m_TimerText;

    [Header("Настройки Арт тасков")]
    public int TargetArtTaskCount = 5;
    [SerializeField] TMP_Text m_ArtTaskTargetText;
    private int ArtTaskCompleted = 0;
    [SerializeField] Task[] m_ArtTaskPrafabs;
    public int TotalArtTasks;
    public int TotalEasyArtTasks;
    public int TotalNormalArtTasks;
    public int TotalHardArtTasks;

    [Header("Настройки ГД тасков")]
    public int TargetGDTaskCount = 10;
    [SerializeField] TMP_Text m_GDTaskTargetText;
    private int GDTaskCompleted = 0;
    [SerializeField] Task[] m_GDTaskPrafabs;
    public int TotalGDTasks;
    public int TotalEasyGDTasks;
    public int TotalNormalGDTasks;
    public int TotalHardGDTasks;

    [Header("Настройки ПР тасков")]
    public int TargetPRTaskCount = 15;
    [SerializeField] TMP_Text m_PRTaskTargetText;
    private int PRTaskCompleted = 0;
    [SerializeField] Task[] m_PRTaskPrafabs;
    public int TotalPRTasks;
    public int TotalEasyPRTasks;
    public int TotalNormalPRTasks;
    public int TotalHardPRTasks;

    [Header("Настройки Куа тасков")]
    public int TargetQATaskCount = 20;
    [SerializeField] TMP_Text m_QATaskTargetText;
    private int QATaskCompleted = 0;
    [SerializeField] Task[] m_QATaskPrafabs;
    public int TotalQATasks;
    public int TotalEasyQATasks;
    public int TotalNormalQATasks;
    public int TotalHardQATasks;

    [Header("Настройки Джокер тасков")]
    public int TotalJokerTasks;
    [SerializeField] Task[] m_JokerShitTaskPrafabs;
    private int JokerShitTaskCompleted = 0;
    public int TotalShitTasks;
    public int TotalEasyShitTasks;
    public int TotalNormalShitTasks;
    public int TotalHardShitTasks;
    [SerializeField] TMP_Text m_ShitTaskText;

    [SerializeField] Task[] m_JokerCriticalTaskPrafabs;
    private int JokerCritTaskCompleted = 0;
    public int TargetCriticalTasks;
    public int TotalEasyCriticalTasks;
    public int TotalNormalCriticalTasks;
    public int TotalHardCriticalTasks;
    [SerializeField] TMP_Text m_CritTaskText;

    [Header("Настройки коэффициент сложности тасков")]
    public float EasyRatio = 0.5f;
    public float NormalRatio = 1f;
    public float HardRatio = 1.5f;

    bool artIsComplited = false;
    bool gdIsComplited = false;
    bool prIsComplited = false;
    bool qaIsComplited = false;
    bool allTaskIsComplited = false;
    bool isPause = false;

    [Header("Панель победы")]
    [SerializeField] GameObject m_VictoriPanel;

    [Header("Панель поражения")]
    [SerializeField] GameObject m_LostPanel;

    [Header("Панель паузы")]
    [SerializeField] GameObject m_PausePanel;


    private void OnValidate()
    {
        sprintId = $"sprint_{sprintNum}";

        m_SprintIDText.text = sprintId;

        TotalArtTasks = TotalEasyArtTasks + TotalNormalArtTasks + TotalHardArtTasks;

        TotalGDTasks = TotalEasyGDTasks + TotalNormalGDTasks + TotalHardGDTasks;

        TotalPRTasks = TotalEasyPRTasks + TotalNormalPRTasks + TotalHardPRTasks;

        TotalQATasks = TotalEasyQATasks + TotalNormalQATasks + TotalHardQATasks;

        TotalShitTasks = TotalEasyShitTasks + TotalNormalShitTasks + TotalHardShitTasks;

        TargetCriticalTasks = TotalEasyCriticalTasks + TotalNormalCriticalTasks + TotalHardCriticalTasks;

        TotalJokerTasks = TotalShitTasks + TargetCriticalTasks;

        TimerHendler();
    }

    private void Start()
    {
        artIsComplited = false;
        gdIsComplited = false;
        prIsComplited = false;
        qaIsComplited = false;
        allTaskIsComplited = false;
        isPause = false;
        Time.timeScale = 1;
        m_SprintIDText.text = sprintId;
        m_ArtTaskTargetText.text = ArtTaskCompleted + "/" + TargetArtTaskCount;

        m_GDTaskTargetText.text = GDTaskCompleted + "/" + TargetGDTaskCount;

        m_PRTaskTargetText.text = PRTaskCompleted + "/" + TargetPRTaskCount;

        m_QATaskTargetText.text = QATaskCompleted + "/" + TargetQATaskCount;

        m_ShitTaskText.text = JokerShitTaskCompleted + "/ ??";

        m_CritTaskText.text = JokerCritTaskCompleted + "/ ??";

        TimerHendler();

        StartCoroutine(SpawnTaskCorutine());
    }

    private void Update()
    {
        if (isPause) return;

        if (allTaskIsComplited)
        {
            return;
        }

        TimerToSprint -= Time.deltaTime;
        TimerHendler();
        if (TimerToSprint <= 0f)
        {
            m_LostPanel.SetActive(true);
            return;
        }

        if (artIsComplited && gdIsComplited && prIsComplited && qaIsComplited)
        {
            m_VictoriPanel.SetActive(true);
            ScenesManager.Instance.Save(sprintNum + 1);
            allTaskIsComplited = true;
        }
    }

    IEnumerator SpawnTaskCorutine()
    {
        int artTaskEasy = 0;
        while (artTaskEasy < TotalEasyArtTasks)
        {
            artTaskEasy++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task easyArtTask = Instantiate(m_ArtTaskPrafabs[0], pos, Quaternion.identity);
            easyArtTask.Initialize(EasyRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int artTaskNormal = 0;
        while (artTaskNormal < TotalNormalArtTasks)
        {
            artTaskNormal++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task normArtTask = Instantiate(m_ArtTaskPrafabs[0], pos, Quaternion.identity);
            normArtTask.Initialize(NormalRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int artTaskHard = 0;
        while (artTaskHard < TotalHardArtTasks)
        {
            artTaskHard++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task hardArtTask = Instantiate(m_ArtTaskPrafabs[0], pos, Quaternion.identity);
            hardArtTask.Initialize(HardRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int gdTaskEasy = 0;
        while (gdTaskEasy < TotalEasyGDTasks)
        {
            gdTaskEasy++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task easyGDTask = Instantiate(m_GDTaskPrafabs[0], pos, Quaternion.identity);
            easyGDTask.Initialize(EasyRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int gdTaskNormal = 0;
        while (gdTaskNormal < TotalNormalGDTasks)
        {
            gdTaskNormal++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task normalGDTask = Instantiate(m_GDTaskPrafabs[0], pos, Quaternion.identity);
            normalGDTask.Initialize(NormalRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int gdTaskHard = 0;
        while (gdTaskHard < TotalHardGDTasks)
        {
            gdTaskHard++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task hardGDTask = Instantiate(m_GDTaskPrafabs[0], pos, Quaternion.identity);
            hardGDTask.Initialize(HardRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int prTaskEasy = 0;
        while (prTaskEasy < TotalEasyPRTasks)
        {
            prTaskEasy++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task easyPRTask = Instantiate(m_PRTaskPrafabs[0], pos, Quaternion.identity);
            easyPRTask.Initialize(EasyRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int prTaskNormal = 0;
        while (prTaskNormal < TotalNormalPRTasks)
        {
            prTaskNormal++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task normalPRTask = Instantiate(m_PRTaskPrafabs[0], pos, Quaternion.identity);
            normalPRTask.Initialize(NormalRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int prTaskHard = 0;
        while (prTaskHard < TotalHardPRTasks)
        {
            prTaskHard++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task hardPRTask = Instantiate(m_PRTaskPrafabs[0], pos, Quaternion.identity);
            hardPRTask.Initialize(HardRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int qaTaskEasy = 0;
        while (qaTaskEasy < TotalEasyQATasks)
        {
            qaTaskEasy++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task easyQATask = Instantiate(m_QATaskPrafabs[0], pos, Quaternion.identity);
            easyQATask.Initialize(EasyRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int qaTaskNormal = 0;
        while (qaTaskNormal < TotalNormalQATasks)
        {
            qaTaskNormal++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task normalQATask = Instantiate(m_QATaskPrafabs[0], pos, Quaternion.identity);
            normalQATask.Initialize(NormalRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int qaTaskHard = 0;
        while (qaTaskHard < TotalHardQATasks)
        {
            qaTaskHard++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task hardQATask = Instantiate(m_QATaskPrafabs[0], pos, Quaternion.identity);
            hardQATask.Initialize(HardRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int shitTaskEasy = 0;
        while (shitTaskEasy < TotalEasyShitTasks)
        {
            shitTaskEasy++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task easyShitTask = Instantiate(m_JokerShitTaskPrafabs[0], pos, Quaternion.identity);
            easyShitTask.Initialize(EasyRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int shitTaskNormal = 0;
        while (shitTaskNormal < TotalNormalShitTasks)
        {
            shitTaskNormal++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task normalShitTask = Instantiate(m_JokerShitTaskPrafabs[0], pos, Quaternion.identity);
            normalShitTask.Initialize(NormalRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int shitTaskHard = 0;
        while (shitTaskHard < TotalHardShitTasks)
        {
            shitTaskHard++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task hardShitTask = Instantiate(m_JokerShitTaskPrafabs[0], pos, Quaternion.identity);
            hardShitTask.Initialize(HardRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int critTaskEasy = 0;
        while (critTaskEasy < TotalEasyCriticalTasks)
        {
            critTaskEasy++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task easyCritTask = Instantiate(m_JokerCriticalTaskPrafabs[0], pos, Quaternion.identity);
            easyCritTask.Initialize(EasyRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int critTaskNormal = 0;
        while (critTaskNormal < TotalNormalCriticalTasks)
        {
            critTaskNormal++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task normalCritTask = Instantiate(m_JokerCriticalTaskPrafabs[0], pos, Quaternion.identity);
            normalCritTask.Initialize(NormalRatio, this);
            yield return new WaitForEndOfFrame();
        }

        int critTaskHard = 0;
        while (critTaskHard < TotalHardCriticalTasks)
        {
            critTaskHard++;
            Vector2 pos = new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
            Task hardCritTask = Instantiate(m_JokerCriticalTaskPrafabs[0], pos, Quaternion.identity);
            hardCritTask.Initialize(HardRatio, this);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    public void TaskCompleted(TaskType type)
    {
        switch (type)
        {
            case TaskType.Art:

                if (ArtTaskCompleted < TargetArtTaskCount) ArtTaskCompleted++;
                m_ArtTaskTargetText.text = ArtTaskCompleted + "/" + TargetArtTaskCount;
                if (ArtTaskCompleted == TargetArtTaskCount) artIsComplited = true;

                break;
            case TaskType.GD:

                if (GDTaskCompleted < TargetGDTaskCount) GDTaskCompleted++;
                m_GDTaskTargetText.text = GDTaskCompleted + "/" + TargetGDTaskCount;

                if (GDTaskCompleted == TargetGDTaskCount) gdIsComplited = true;
                break;
            case TaskType.PR:

                if (PRTaskCompleted < TargetPRTaskCount) PRTaskCompleted++;
                m_PRTaskTargetText.text = PRTaskCompleted + "/" + TargetPRTaskCount;

                if (PRTaskCompleted == TargetPRTaskCount) prIsComplited = true;

                break;
            case TaskType.QA:

                if (QATaskCompleted < TargetQATaskCount) QATaskCompleted++;
                m_QATaskTargetText.text = QATaskCompleted + "/" + TargetQATaskCount;

                if (QATaskCompleted == TargetQATaskCount) qaIsComplited = true;

                break;
            case TaskType.JokerShit:

                JokerShitTaskCompleted++;
                TimerToSprint -= 3f;
                m_ShitTaskText.text = JokerShitTaskCompleted + "/ ??";

                break;
            case TaskType.JokerCrit:

                int point = 1;
                if (ArtTaskCompleted < TargetArtTaskCount && point > 0)
                {
                    point = 0;
                    ArtTaskCompleted++;
                    m_ArtTaskTargetText.text = ArtTaskCompleted + "/" + TargetArtTaskCount;
                }
                if (GDTaskCompleted < TargetGDTaskCount && point > 0)
                {
                    point = 0;
                    GDTaskCompleted++;
                    m_GDTaskTargetText.text = GDTaskCompleted + "/" + TargetGDTaskCount;
                }
                if (PRTaskCompleted < TargetPRTaskCount && point > 0)
                {
                    point = 0;
                    PRTaskCompleted++;
                    m_PRTaskTargetText.text = PRTaskCompleted + "/" + TargetPRTaskCount;
                }
                if (QATaskCompleted < TargetQATaskCount && point > 0)
                {
                    point = 0;
                    QATaskCompleted++;
                    m_QATaskTargetText.text = QATaskCompleted + "/" + TargetQATaskCount;
                }

                if (ArtTaskCompleted == TargetArtTaskCount) artIsComplited = true;

                if (GDTaskCompleted == TargetGDTaskCount) gdIsComplited = true;

                if (PRTaskCompleted == TargetPRTaskCount) prIsComplited = true;

                if (QATaskCompleted == TargetQATaskCount) qaIsComplited = true;

                JokerCritTaskCompleted++;
                m_CritTaskText.text = JokerCritTaskCompleted + "/ ??";

                break;
            default: break;
        }
    }

    private void TimerHendler()
    {
        if (TimerToSprint <= 0) TimerToSprint = 0;

        int minutes = TimeSpan.FromSeconds(TimerToSprint).Minutes;
        int seconds = TimeSpan.FromSeconds(TimerToSprint).Seconds;
        m_TimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void Pause()
    {
        if (isPause)
        {
            Time.timeScale = 1;
            m_PausePanel.SetActive(false);
            isPause = false;
        }
        else
        {
            Time.timeScale = 0;
            m_PausePanel.SetActive(true);
            isPause = true;
        }
    }
}
