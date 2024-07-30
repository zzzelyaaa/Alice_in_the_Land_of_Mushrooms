using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;

    private int sprintNumber = 1;
    private Storage storage;
    private GameData gameData;


    private void Awake()
    {
        Instance = this;
        storage = new Storage();
        Load();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sprintNumber);
    }

    public void LoadNewGame()
    {
        Save(1);
        SceneManager.LoadScene(sprintNumber);
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Save(int sprintID)
    {
        sprintNumber = sprintID;
        gameData.SprintStorageNumber = sprintNumber;
        storage.Save(gameData);
    }

    public void Load()
    {
        gameData = (GameData)storage.Load(new GameData());
        sprintNumber = gameData.SprintStorageNumber;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
