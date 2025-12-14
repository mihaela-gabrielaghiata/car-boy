using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Header("Target")]
    public Transform target;  

    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 10f, 0f);
    public float smoothSpeed = 5f;

    [Header("Rotation")]
    public bool rotateWithTarget = false;

    void Update()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = desiredPosition;

        if (rotateWithTarget)
        {
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

}
