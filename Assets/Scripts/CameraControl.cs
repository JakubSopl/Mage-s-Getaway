using UnityEngine;
using UnityEngine.UI;

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

        cam.transform.position = Vector3.Lerp(startCamPos, targetCamPos, transitionProgress);
        cam.transform.rotation = Quaternion.Lerp(startCamRot, targetCamRot, transitionProgress);

        if (fadeImage != null)
        {
            Color fadeColor = fadeImage.color;
            fadeColor.a = Mathf.Lerp(0, 1, transitionProgress);
            fadeImage.color = fadeColor;
        }

        if (transitionProgress >= 1f)
        {
            isTransitioning = false;

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
        // Nastavení rotace kamery podle myši
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);  // Omezení úhlu pohledu nahoru a dolù

        smoothYaw = Mathf.Lerp(smoothYaw, yaw, smoothTime);
        smoothPitch = Mathf.Lerp(smoothPitch, pitch, smoothTime);

        // Nastavení rotace postavy horizontálnì a kamery vertikálnì
        character.transform.rotation = Quaternion.Euler(0, smoothYaw, 0);
        firstPersonCenter.transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);

        // Výchozí pozice kamery na pozici firstPersonCenter
        Vector3 targetPosition = firstPersonCenter.transform.position;

        // Detekce kolize na trase mezi hráèem a cílovou pozicí kamery
        if (Physics.Linecast(character.transform.position + Vector3.up * yOffset, targetPosition, out _camHit))
        {
            // Pokud je detekována kolize, posuneme kameru blíž k hráèi, ale stále na pozici firstPersonCenter
            cam.transform.position = _camHit.point + _camHit.normal * 0.1f;
        }
        else
        {
            // Pokud není detekována kolize, nastavíme kameru na pozici firstPersonCenter
            cam.transform.position = targetPosition;
        }

        // Kamera sleduje pohled hráèe s respektováním vertikální rotace
        cam.transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);
    }




    void HandleThirdPersonView()
    {
        Vector3 desiredPosition = character.transform.position + (character.transform.forward * camDist.z) + new Vector3(0, yOffset, 0);
        cam.transform.position = desiredPosition;

        cameraCenter.transform.position = character.transform.position + new Vector3(0, yOffset, 0);

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraCenter.transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        cam.transform.localPosition = camDist;
        cam.transform.rotation = cameraCenter.transform.rotation;

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

        float preCollisionBuffer = 1f; 

        GameObject obj = new GameObject();
        obj.transform.SetParent(cam.transform.parent);
        var position = cam.transform.localPosition;
        obj.transform.localPosition = new Vector3(position.x, position.y, position.z - collisionSensitivity - preCollisionBuffer); // Buffer pro pøiblížení

        if (Physics.Linecast(cameraCenter.transform.position, obj.transform.position, out _camHit))
        {
            var transform1 = cam.transform;
            transform1.position = _camHit.point;
            var localPosition = transform1.localPosition;
            localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z + collisionSensitivity);
            transform1.localPosition = localPosition;
        }

        Destroy(obj);

        if (cam.transform.localPosition.z > -1f)
        {
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, -1f);
        }
    }

}
