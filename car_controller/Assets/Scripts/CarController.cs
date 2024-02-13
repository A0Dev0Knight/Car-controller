using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // Tire refrence;
    [SerializeField] Transform L01_TireTransform;
    [SerializeField] Transform R01_TireTransform;
    [SerializeField] Transform L02_TireTransform;
    [SerializeField] Transform R02_TireTransform;
    
    //spring parameters;
    [SerializeField] float Strength = 10f;
    [SerializeField] float RestDist = 10f;
    [SerializeField] float MaxOffset = 1f; // |offset| <= RestDist
    [SerializeField] float Dampning = 10f;

    Rigidbody carRigidbody;
    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }
    
    void OY_Forces(Transform tireTransform)
    {
        //suspension force for one tire only

        // store raycast info in tireRay
        RaycastHit tireRay;
        
        // get the info on the raycast and also store the boolean
        bool rayDidHit = Physics.Raycast(tireTransform.position, -tireTransform.transform.up, out tireRay, (RestDist + MaxOffset));
        
        if (rayDidHit)
        {
            Vector3 springDir = tireTransform.transform.up;

            Vector3 tireWorldVel = carRigidbody.GetPointVelocity(tireTransform.position);

            float offset = RestDist - tireRay.distance;

            float vel = Vector3.Dot(springDir, tireWorldVel);

            float force = (offset * Strength) - (vel * Dampning);

            carRigidbody.AddForceAtPosition(springDir * force, tireTransform.position);
        }
        
    }
    private void FixedUpdate()
    {
        OY_Forces(L01_TireTransform);
        OY_Forces(R01_TireTransform);
        OY_Forces(L02_TireTransform);
        OY_Forces(R02_TireTransform);
    }
}
