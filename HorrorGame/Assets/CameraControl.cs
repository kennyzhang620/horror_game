using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Camera camera;
    public Rigidbody rb;
    float Sensitivity = 3f;
    float MoveSensitivity = 500;
    float JumpPwr = 1.25f;
    float maxSprint = 0.75f;
    float walkSpeed = 0.25f;
    float currentSpeed = 0;

    public bool isSprinting = false;
    public bool isJump = false;

    public int roty = 0;

    private void OnCollisionStay(Collision collision)
    {
        isJump = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        if (isSprinting)
            currentSpeed = maxSprint;
        else
            currentSpeed = walkSpeed;

        if (rb.velocity.x > currentSpeed)
        {
            rb.velocity = new Vector3(currentSpeed, rb.velocity.y, rb.velocity.z);
        }

        if (rb.velocity.x < -currentSpeed)
        {
            rb.velocity = new Vector3(-currentSpeed, rb.velocity.y, rb.velocity.z);
        }


        if (rb.velocity.z > currentSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, currentSpeed);
        }

        if (rb.velocity.z < -currentSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, -currentSpeed);
        }

        if (Input.GetAxis("Mouse X") > 0 || Input.GetKey(KeyCode.L))
        {
            transform.Rotate(Vector3.up*Sensitivity, Space.Self);
        }

        if (Input.GetAxis("Mouse X") < 0 || Input.GetKey(KeyCode.J))
        {
            transform.Rotate(-Vector3.up * Sensitivity, Space.Self);
        }

        //print(camera.transform.localRotation.eulerAngles.x);

        if ((Input.GetAxis("Mouse Y") > 0 || Input.GetKey(KeyCode.I)) && roty < 25) 
        {
            camera.transform.Rotate(-Vector3.right * Sensitivity, Space.Self);
            roty++;
        }

        if ((Input.GetAxis("Mouse Y") < 0 || Input.GetKey(KeyCode.K)) && roty > -25)
        {
            camera.transform.Rotate(Vector3.right * Sensitivity, Space.Self);
            roty--;
        }

        if (Input.GetAxis("Horizontal")  > 0)
        {
            rb.AddRelativeForce(-Vector3.left * MoveSensitivity);
        }

        if (Input.GetAxis("Horizontal") < 0)
        {
            rb.AddRelativeForce(Vector3.left * MoveSensitivity);
        }

        if (Input.GetAxis("Vertical") > 0)
        {
            rb.AddRelativeForce(Vector3.forward * MoveSensitivity);
        }

        if (Input.GetAxis("Vertical") < 0)
        {
            rb.AddRelativeForce(-Vector3.forward * MoveSensitivity);
        }

        if (Input.GetAxis("Jump") != 0 && isJump)
        {
            rb.AddRelativeForce(Vector3.up*MoveSensitivity*JumpPwr);
            isJump = false;
        }
    }
}
