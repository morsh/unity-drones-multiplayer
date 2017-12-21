using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour {

    public GameObject drone = null;
    private void Awake()
    {
        //drone = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private Vector3 velocityCameraFollow;
    public Vector3 behindPosition = new Vector3(0, 2, -4);
    public float angle;
    private void FixedUpdate()
    {
        if (drone == null) { return;  }

        transform.position = Vector3.SmoothDamp(transform.position, drone.transform.TransformPoint(behindPosition) + Vector3.up * Input.GetAxis("Vertical"), ref velocityCameraFollow, 0.1f);

        var movementScript = drone.GetComponent<DroneMovementScript>();
        transform.rotation = Quaternion.Euler(new Vector3(angle, movementScript.currentYRotation, 0));
    }
}
