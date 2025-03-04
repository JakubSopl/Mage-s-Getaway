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
        Debug.Log("Hr�� zem�el, zobrazujeme DeadMenu.");
        deadMenu.SetActive(true);

        // Odemyk� my�, aby hr�� mohl klikat na tla��tka
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