using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    public Camera thirdPersonCamera; // Kamera t�et� osoby
    public Camera battleCamera;      // Kamera pro boj

    public void EnterBattleMode(Transform battleCameraPosition)
    {
        Debug.Log("Entering battle mode. Switching camera position.");

        if (battleCamera == null || thirdPersonCamera == null)
        {
            Debug.LogError("BattleCamera or ThirdPersonCamera is not assigned!");
            return;
        }

        battleCamera.transform.position = battleCameraPosition.position;
        battleCamera.transform.rotation = battleCameraPosition.rotation;

        thirdPersonCamera.gameObject.SetActive(false);
        battleCamera.gameObject.SetActive(true);

        Debug.Log("Camera switched to battle mode.");
    }


    public void ExitBattleMode()
    {
        // P�epni zp�t na kameru t�et� osoby
        battleCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);
    }
}
