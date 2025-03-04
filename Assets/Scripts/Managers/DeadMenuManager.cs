using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class DeadMenuManager : MonoBehaviour
{
    public GameObject deadMenu;

    private void Start()
    {
        deadMenu.SetActive(false);

        // Make sure UI is interactive even if the game is "stopped"
        UnityEngine.EventSystems.EventSystem.current.enabled = true;
    }

    public void ShowDeadMenu()
    {
        Debug.Log("Hráè zemøel, zobrazujeme DeadMenu.");
        deadMenu.SetActive(true);

        // Odemyká myš, aby hráè mohl klikat na tlaèítka
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reloads the current scene;
    }


    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

}