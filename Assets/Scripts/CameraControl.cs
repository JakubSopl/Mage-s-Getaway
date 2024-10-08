using UnityEngine;

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

    private bool isFirstPerson = false;
    public GameObject firstPersonCenter; // Objekt za hlavou hr��e pro faux first-person view

    //Pro "prvn� osobu"
    private float yaw;
    private float pitch;
    public float minPitch;   // Maxim�ln� sklon dol�
    public float maxPitch;    // Maxim�ln� sklon nahoru

    private float smoothYaw;
    private float smoothPitch;
    public float smoothTime = 0.1f; // �as pro hladk� p�echody

    void Start()
    {
        camDist = cam.transform.localPosition;
        zoomDistance = zoomDefault;
        camDist.z = zoomDistance;
        Cursor.visible = false;
        yaw = character.transform.eulerAngles.y;
        pitch = cam.transform.eulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                cam.transform.SetParent(firstPersonCenter.transform); // P�ipoj�me kameru k FirstPersonCenter
                cam.transform.localPosition = Vector3.zero; // Kamera p�esn� na pozici FirstPersonCenter
                cam.transform.localRotation = Quaternion.identity; // Reset rotace kamery
            }
            else
            {
                cam.transform.SetParent(cameraCenter.transform); // P�ipojen� zp�t k CameraCenter
                cam.transform.localPosition = camDist; // P�vodn� pozice kamery
                cam.transform.localRotation = Quaternion.identity; // Reset rotace
            }
        }

        if (isFirstPerson)
        {
            HandleFirstPersonView();
        }
        else
        {
            HandleThirdPersonView();
        }
    }

    void HandleFirstPersonView()
    {
        // Kamera bude pevn� za postavou, ne uvnit� n�
        cam.transform.position = firstPersonCenter.transform.position;
        cam.transform.rotation = firstPersonCenter.transform.rotation;

        // Voln� pohyb kamery (nahoru/dol� a voln� doleva/doprava)
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;

        // Omezit rotaci nahoru a dol�
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Pou��t interpolaci pro hlad�� p�echod rotace
        smoothYaw = Mathf.Lerp(smoothYaw, yaw, smoothTime);
        smoothPitch = Mathf.Lerp(smoothPitch, pitch, smoothTime);

        // Nastavit novou rotaci kamery
        firstPersonCenter.transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);

        // Hr�� se ot��� podle sm�ru, kam se chce pohybovat
        character.transform.rotation = Quaternion.Euler(0, smoothYaw, 0);
    }

    void HandleThirdPersonView()
    {
        // Pozice a ot��en� CameraCenter dle postavy
        var position1 = character.transform.position;
        cameraCenter.transform.position = new Vector3(position1.x, position1.y + yOffset, position1.z);

        var rotation = cameraCenter.transform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * sensitivity / 2,
            rotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, rotation.eulerAngles.z);
        cameraCenter.transform.rotation = rotation;

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

        GameObject obj = new GameObject();
        obj.transform.SetParent(cam.transform.parent);
        var position = cam.transform.localPosition;
        obj.transform.localPosition = new Vector3(position.x, position.y, position.z - collisionSensitivity);

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
