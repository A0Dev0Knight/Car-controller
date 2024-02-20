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
    
    // spring parameters;
    [SerializeField] float Strength = 10f;
    [SerializeField] float RestDist = 10f;
    [SerializeField] float MaxOffset = 1f; // |offset| <= RestDist
    [SerializeField] float Dampning = 10f;
    [SerializeField] float TireMass = 5f;

    // steering parameters
    [SerializeField] AnimationCurve BackTirelookupCurve;
    [SerializeField] AnimationCurve FrontTirelookupCurve;

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
    
    void OX_Forces(Transform tireTransform)
    {
        RaycastHit tireRay;
        bool rayDidHit = Physics.Raycast(tireTransform.position, -tireTransform.transform.up, out tireRay, (RestDist + MaxOffset));

        if (rayDidHit)
        {
            // versor pt directia ox a rotii
            Vector3 steerDir = tireTransform.transform.right;
            
            // viteza unei roti in R^3
            Vector3 tireVelocity = carRigidbody.GetPointVelocity(tireTransform.position);
            
            // viteza unei roti pe componenta ox
            float tireVelocityOX = Vector3.Dot(steerDir, tireVelocity);

            // cat % din viteza unei roti este regasita pe ox, am nevoie de modul sa nu obtin valori negative pt procent
            float percentage = Mathf.Abs(tireVelocityOX / Vector3.Magnitude(tireVelocity));
           
            /* cat grip sa aiba o roata la un moment dat
               pt un procent de 0% inseamna ca ma duc 100% inainte, nu in laterale,
               asadar am nevoie de complementul lui y fata de 1 si ala este gripul
             */
            float grip = 1 - BackTirelookupCurve.Evaluate(percentage);
            
            // aici obtin acceleratia, F = m*a, Time.fixedDeltaTime este ca o secunda, o unitate fundamentala de masura
            float accelaratie = (-tireVelocityOX * grip)/Time.fixedDeltaTime;

            // aplic forta
            carRigidbody.AddForceAtPosition(steerDir * TireMass * accelaratie, tireTransform.position);
        }
    }
   
    void Forces(Transform tireTransform)
    {
        OY_Forces(tireTransform);
        OX_Forces(tireTransform);
    }
    private void FixedUpdate()
    {
        Forces(L01_TireTransform);
        Forces(R01_TireTransform);
        Forces(L02_TireTransform);
        Forces(R02_TireTransform);
    }
}
