using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public CameraController cameraController;
    public bool noAngleRestrictionZone = false; // Ur�uje, zda je tato z�na bez omezen� �hlu pohledu

    // Kdy� hr�� vstoup� do z�ny
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (noAngleRestrictionZone)
            {
                AllowLookAtSky();
            }
            else
            {
                cameraController.EnterFirstPersonView();
            }
        }
    }

    // Kdy� hr�� opust� z�nu
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (noAngleRestrictionZone)
            {
                ResetCameraRestrictions();
            }
            else
            {
                cameraController.EnterThirdPersonView();
            }
        }
    }

    // Funkce pro nastaven� kamery tak, aby se hr�� mohl pod�vat do nebe
    private void AllowLookAtSky()
    {
        cameraController.minPitch = -45f; // Nastav� minim�ln� �hel na -90 stup�� (pohled do nebe)
        cameraController.maxPitch = 45f;  // Maxim�ln� �hel na 15 stup��
    }

    // Funkce pro resetov�n� omezen� kamery p�i opu�t�n� z�ny
    private void ResetCameraRestrictions()
    {
        cameraController.minPitch = 5f; // P�vodn� minim�ln� �hel (p�izp�sobte podle sv�ho nastaven�)
        cameraController.maxPitch = 15f;  // P�vodn� maxim�ln� �hel
    }
}
