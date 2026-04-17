using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UiManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Settings;
    public GameObject NextLevel;
    public float timer;
    public TextMeshProUGUI timerText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        timerText.text = "Time: " + Mathf.Floor(timer).ToString();
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
        SceneManager.LoadSceneAsync(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
