using TMPro;
using UnityEngine;

public class Example : MonoBehaviour
{
    private Storage storage;
    private GameData gameData;
    private int SprintNum = 1;
    private string sprintId;
    [SerializeField] TMP_Text m_SprintIDText;

    private void Start()
    {
        storage = new Storage();
        Load();
    }



    public void Load()
    {
        gameData = (GameData)storage.Load(new GameData());
        SprintNum = gameData.SprintStorageNumber;
        sprintId = $"sprint_{SprintNum}";
        m_SprintIDText.text = sprintId;
    }
}
