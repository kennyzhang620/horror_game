using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder_Fixed : MonoBehaviour
{

    Transform target = null;
    Rigidbody rb;
    Animator an;

    public int ZombieState = 0; // 0 Walk // 1 Aggro

    float maxSpeedAnim = 0.1f;
    float maxRBSpeed = 0.003f;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            target = other.transform;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        an = GetComponent<Animator>();
        an.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        print(rb.velocity.magnitude);

        if (ZombieState == 0)
            an.speed = maxSpeedAnim * (Mathf.Sqrt(((rb.velocity.x) * (rb.velocity.x)) + ((rb.velocity.y) * (rb.velocity.y))) / maxRBSpeed);
        else
            an.speed = 1;
        if (target != null)
        {
            transform.LookAt(target);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            if (ZombieState == 0)
            {
                an.Play("Zombie_Walk");
                rb.AddRelativeForce(Vector3.forward * 20);
            }
            if (ZombieState == 1)
            {
                an.Play("Zombie_Aggro");
                rb.AddRelativeForce(Vector3.forward * 80);
            }
           
            
        }
    }
}
