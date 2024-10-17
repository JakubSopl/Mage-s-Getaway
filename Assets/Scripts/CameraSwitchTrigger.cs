using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public CameraController cameraController;

    // Když hráè vstoupí do zóny (vnitøní prostory)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.EnterFirstPersonView();
        }
    }

    // Když hráè opustí zónu (vnìjší prostory)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.EnterThirdPersonView();
        }
    }
}
