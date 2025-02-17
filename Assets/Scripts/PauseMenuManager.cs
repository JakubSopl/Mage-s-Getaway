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

        // Umo�n� klik�n� na UI
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.enabled = true;

        // Uvoln� kurzor pro klik�n� na tla��tka
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Debug.Log("Hra pokra�uje");

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Zabr�n� pohybu my�i po obrazovce
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        Debug.Log("Restartov�n� hry");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Debug.Log("N�vrat do hlavn�ho menu");

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
