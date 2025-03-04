using UnityEngine;

public class MainMenuCursor : MonoBehaviour
{
    void Start()
    {
        // Zajistí, že kurzor bude viditelný a odemèený vždy, když se hráè dostane do hlavního menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
