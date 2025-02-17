using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Debug.Log("Hra pozastavena");

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // Umožní klikání na UI
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.enabled = true;

        // Uvolní kurzor pro klikání na tlaèítka
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Debug.Log("Hra pokraèuje");

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Zabrání pohybu myši po obrazovce
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        Debug.Log("Restartování hry");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Debug.Log("Návrat do hlavního menu");

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
