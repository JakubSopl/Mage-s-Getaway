using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public CameraController cameraController;
    public bool noAngleRestrictionZone = false; // Urèuje, zda je tato zóna bez omezení úhlu pohledu

    // Když hráè vstoupí do zóny
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

    // Když hráè opustí zónu
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

    // Funkce pro nastavení kamery tak, aby se hráè mohl podívat do nebe
    private void AllowLookAtSky()
    {
        cameraController.minPitch = -45f; // Nastaví minimální úhel na -90 stupòù (pohled do nebe)
        cameraController.maxPitch = 45f;  // Maximální úhel na 15 stupòù
    }

    // Funkce pro resetování omezení kamery pøi opuštìní zóny
    private void ResetCameraRestrictions()
    {
        cameraController.minPitch = 5f; // Pùvodní minimální úhel (pøizpùsobte podle svého nastavení)
        cameraController.maxPitch = 15f;  // Pùvodní maximální úhel
    }
}
