using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public CameraController cameraController;

    // Kdy� hr�� vstoup� do z�ny (vnit�n� prostory)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.EnterFirstPersonView();
        }
    }

    // Kdy� hr�� opust� z�nu (vn�j�� prostory)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.EnterThirdPersonView();
        }
    }
}
