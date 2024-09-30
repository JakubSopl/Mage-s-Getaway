using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraClamp : MonoBehaviour
{
    public float restrictionAngle = -50f;

    void Update()
    {
        // Get current rotation
        var rotation = UnityEditor.TransformUtils.GetInspectorRotation(gameObject.transform);

        // Make sure camera can't clip into player
        if (rotation.x < restrictionAngle)
        {
            // Set x rotation to max rotation and leave rest as is
            UnityEditor.TransformUtils.SetInspectorRotation(gameObject.transform, new Vector3(restrictionAngle, rotation.y, rotation.z));
        }
    }
}
