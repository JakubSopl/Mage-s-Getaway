using UnityEngine;
using UnityEngine.UI; // Pøidáno pro práci s UI

public class CameraController : MonoBehaviour
{
    public GameObject character;
    public GameObject cameraCenter;
    public float yOffset;
    public float sensitivity;
    public Camera cam;

    private RaycastHit _camHit;
    public Vector3 camDist;
    public float scrollSensitivity = 2f;
    public float scrollDampening = 6f;
    public float zoomMin = 3.5f;
    public float zoomMax = 15f;
    public float zoomDefault = 10f;
    public float zoomDistance;
    public float collisionSensitivity = 2.5f;

    public bool isFirstPerson = false;
    public GameObject firstPersonCenter; 

    private float yaw;
    private float pitch;
    public float minPitch;
    public float maxPitch;

    private float smoothYaw;
    private float smoothPitch;
    public float smoothTime = 0.1f; 

    public bool isTransitioning = false;
    private float transitionProgress = 0f;
    public float transitionDuration = 0.3f; 

    private Vector3 startCamPos;
    private Quaternion startCamRot;
    private Vector3 targetCamPos;
    private Quaternion targetCamRot;

    // Pøidáno: Reference na UI Image pro èerný obrázek
    public Image fadeImage;

    void Start()
    {
        camDist = new Vector3(0, yOffset, -zoomDefault); 
        zoomDistance = zoomDefault;
        Cursor.visible = false;
        yaw = character.transform.eulerAngles.y;
        pitch = cam.transform.eulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;

        // Nastav alfa na èerném obrázku na 0 (úplnì prùhledný)
        if (fadeImage != null)
        {
            Color fadeColor = fadeImage.color;
            fadeColor.a = 0;
            fadeImage.color = fadeColor;
        }
    }

    void Update()
    {
        if (isTransitioning)
        {
            HandleCameraTransition();
        }
        else
        {
            if (isFirstPerson)
            {
                HandleFirstPersonView();
            }
            else
            {
                HandleThirdPersonView();
            }
        }
    }

    public void EnterFirstPersonView()
    {
        if (!isFirstPerson && !isTransitioning)
        {
            isFirstPerson = true;
            StartCameraTransition(firstPersonCenter.transform.position, firstPersonCenter.transform.rotation);
        }
    }

    public void EnterThirdPersonView()
    {
        if (isFirstPerson && !isTransitioning)
        {
            isFirstPerson = false;
            Vector3 targetPosition = character.transform.position + (character.transform.forward * camDist.z) + new Vector3(0, yOffset, 0);
            StartCameraTransition(targetPosition, character.transform.rotation);
        }
    }

    private void StartCameraTransition(Vector3 targetPosition, Quaternion targetRotation)
    {
        isTransitioning = true;
        transitionProgress = 0f;

        startCamPos = cam.transform.position;
        startCamRot = cam.transform.rotation;

        targetCamPos = targetPosition;
        targetCamRot = targetRotation;

        // Reset alfa na 0 pøi zaèátku pøechodu
        if (fadeImage != null)
        {
            Color fadeColor = fadeImage.color;
            fadeColor.a = 0;
            fadeImage.color = fadeColor;
        }
    }

    private void HandleCameraTransition()
    {
        transitionProgress += Time.deltaTime / transitionDuration;

        // Lerp pozice a rotace kamery
        cam.transform.position = Vector3.Lerp(startCamPos, targetCamPos, transitionProgress);
        cam.transform.rotation = Quaternion.Lerp(startCamRot, targetCamRot, transitionProgress);

        // Zvyšuj alfa èerného obrázku bìhem pøechodu
        if (fadeImage != null)
        {
            Color fadeColor = fadeImage.color;
            fadeColor.a = Mathf.Lerp(0, 1, transitionProgress); // Plynulý nárùst alfa od 0 do 1
            fadeImage.color = fadeColor;
        }

        if (transitionProgress >= 1f)
        {
            isTransitioning = false; // Pøechod dokonèen

            // Nastavit alfa èerného obrázku zpìt na 0 (zmizení)
            if (fadeImage != null)
            {
                Color fadeColor = fadeImage.color;
                fadeColor.a = 0;
                fadeImage.color = fadeColor;
            }
        }
    }


    void HandleFirstPersonView()
    {
        cam.transform.position = firstPersonCenter.transform.position;
        cam.transform.rotation = firstPersonCenter.transform.rotation;

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        smoothYaw = Mathf.Lerp(smoothYaw, yaw, smoothTime);
        smoothPitch = Mathf.Lerp(smoothPitch, pitch, smoothTime);

        firstPersonCenter.transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);
        character.transform.rotation = Quaternion.Euler(0, smoothYaw, 0);
    }

    void HandleThirdPersonView()
    {
        // Nastavení pozice kamery za postavou
        Vector3 desiredPosition = character.transform.position + (character.transform.forward * camDist.z) + new Vector3(0, yOffset, 0);
        cam.transform.position = desiredPosition;

        // Umístìní støedu kamery na pozici postavy
        cameraCenter.transform.position = character.transform.position + new Vector3(0, yOffset, 0);

        // Rotace kamery pomocí myši
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraCenter.transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // Nastavení posunu kamery
        cam.transform.localPosition = camDist;
        cam.transform.rotation = cameraCenter.transform.rotation;

        // Detekce koleèka myši pro zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            var scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            scrollAmount *= (zoomDistance * 0.3f);
            zoomDistance += scrollAmount * -1f;
            zoomDistance = Mathf.Clamp(zoomDistance, zoomMin, zoomMax);
        }

        if (camDist.z != zoomDistance * -1f)
        {
            camDist.z = Mathf.Lerp(camDist.z, -zoomDistance, Time.deltaTime * scrollDampening);
        }

        cam.transform.localPosition = camDist;

        // Vytvoøíme bezpeènostní buffer, aby kamera nereagovala tìsnì pøed pøekážkou
        float preCollisionBuffer = 1f; // Pøedstih pro zaèátek pøibližování

        // Vytvoøíme pomocný objekt pro kontrolu kolize
        GameObject obj = new GameObject();
        obj.transform.SetParent(cam.transform.parent);
        var position = cam.transform.localPosition;
        obj.transform.localPosition = new Vector3(position.x, position.y, position.z - collisionSensitivity - preCollisionBuffer); // Buffer pro pøiblížení

        // Detekujeme kolizi s objektem pøed kamerou
        if (Physics.Linecast(cameraCenter.transform.position, obj.transform.position, out _camHit))
        {
            var transform1 = cam.transform;
            transform1.position = _camHit.point;
            var localPosition = transform1.localPosition;
            localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z + collisionSensitivity);
            transform1.localPosition = localPosition;
        }

        // Znièení pomocného objektu po kontrole
        Destroy(obj);

        // Zajištìní, aby kamera nikdy neprošla zdí ani pøi pøiblížení
        if (cam.transform.localPosition.z > -1f)
        {
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, -1f);
        }
    }

}
