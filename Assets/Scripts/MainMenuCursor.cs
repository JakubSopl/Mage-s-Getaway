using UnityEngine;

public class MainMenuCursor : MonoBehaviour
{
    void Start()
    {
        // Zajist�, �e kurzor bude viditeln� a odem�en� v�dy, kdy� se hr�� dostane do hlavn�ho menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
