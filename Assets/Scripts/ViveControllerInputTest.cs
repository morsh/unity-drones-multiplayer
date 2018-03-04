using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to test vivde controllers' interactions using debug messages
/// </summary>
public class ViveControllerInputTest : MonoBehaviour
{
    private Rigidbody drone;

    // 1
    private SteamVR_TrackedObject trackedObj;
    // 2
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        drone = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {
        MovementUpDown();
        MovementForward();

        var movement = new Vector3(Controller.GetAxis().x, 0, Controller.GetAxis().y);

        drone.AddRelativeForce(Vector3.up * upForce);
        drone.rotation = Quaternion.Euler(
            new Vector3(
                /*tiltAmountForward*/0, 
                Vector3.Angle(drone.transform.forward, movement), 0)
        );
    }

    public float upForce;
    void MovementUpDown()
    {
        upForce = 98.1f;
    }

    private float movementForwardSpeed = 100.0f;
    [HideInInspector]
    public float tiltAmountForward = 0;
    private float tiltVelocityForward;
    void MovementForward()
    {
        Vector2 axis = Controller.GetAxis();
        if (axis != Vector2.zero)
        {
            drone.AddRelativeForce(new Vector3(axis.x, 0, axis.y) * movementForwardSpeed);
            tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward,20 * axis.magnitude, ref tiltVelocityForward, 0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Controller.GetAxis() != Vector2.zero)
        {
            Debug.Log(gameObject.name + Controller.GetAxis());
        }

        if (Controller.GetHairTriggerDown())
        {
            Debug.Log(gameObject.name + " Trigger Press");
        }

        if (Controller.GetHairTriggerUp())
        {
            Debug.Log(gameObject.name + " Trigger Release");
        }

        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            Debug.Log(gameObject.name + " Grip Press");
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            Debug.Log(gameObject.name + " Grip Release");
        }
    }
}
