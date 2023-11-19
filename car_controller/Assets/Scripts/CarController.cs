using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] Transform L01_Tire;
    //calculate OY forces;
    [SerializeField] float Strength = 10f;
    [SerializeField] float RestDist = 1f; // |Offset| <= MaxOffset
    [SerializeField] float Dampning = 10f;


    Rigidbody carRigidbody;
    // Start is called before the first frame update
    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //suspension force
        bool rayHit = Physics.Raycast(transform.position, Vector3.down);
        if (rayHit) {

            Vector3 springDir = L01_Tire.up;
            
            Vector3 tireWorldVel = carRigidbody.GetPointVelocity(L01_Tire.position);

            RaycastHit tireRay;
            Physics.Raycast(L01_Tire.position, Vector3.down, out tireRay);
            float offset = RestDist - tireRay.distance;

            float vel = Vector3.Dot(springDir, tireWorldVel);

            float force = (offset * Strength) - (vel * Dampning);

            carRigidbody.AddForceAtPosition(springDir * force, L01_Tire.position);
        }
    }
}
