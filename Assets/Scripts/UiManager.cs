using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UiManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Settings;
    public GameObject NextLevel;
    public float timer;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI enemyCountText;
    public EnemyChecker enemyChecker;
    public PlayerHealth playerHealth;
    private int maxEnemyCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return null;

        if (enemyChecker != null)
        {
            maxEnemyCount = enemyChecker.EnemyCount;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        timerText.text = "Time: " + Mathf.Floor(timer).ToString();

        if (enemyChecker != null && enemyCountText != null)
        {
            enemyCountText.text = "eliminate all enemies: " + enemyChecker.EnemyCount + " / " + maxEnemyCount;
        }
    }
    
    public void SettingsButton()
    {
        Settings.SetActive(true);
        MainMenu.SetActive(false);
        Debug.Log("Settings Button Pressed");
    }
    public void NextLevelButton()
    {
        NextLevel.SetActive(true);
        MainMenu.SetActive(false);
        Debug.Log("Next Level Button Pressed");
    }
    public void MainMenuButton()
    {
        MainMenu.SetActive(true);
        Settings.SetActive(false);
        NextLevel.SetActive(false);
        Debug.Log("Main Menu Button Pressed");
    }
    public void LoadLevel1()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("No next level is available in Build Settings.");
            return;
        }

        SceneManager.LoadSceneAsync(nextSceneIndex);
        Debug.Log("Prototype Level Button Pressed");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
