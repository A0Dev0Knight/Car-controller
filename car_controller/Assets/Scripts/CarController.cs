using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using System;

public class CarController : MonoBehaviour
{
    private Rigidbody carRigidbody;

    // car properties
    [Space]
    [BoxGroup("Car Properties")] [SerializeField] Transform[] rayPoints;
    [BoxGroup("Car Properties")] [SerializeField] LayerMask drivable;


    // spring parameters;
    [Space]
    [BoxGroup("Suspension Settings")] [SerializeField] float springStiffness = 10f;
    [BoxGroup("Suspension Settings")] [SerializeField] float restLength = 1f;
    [BoxGroup("Suspension Settings")] [SerializeField] float maxSpringTarvel = .4f;
    [BoxGroup("Suspension Settings")] [SerializeField] float damperStiffness = 10f;
    [BoxGroup("Suspension Settings")] [SerializeField] float wheelRadius;
    
    void Suspension()
    {
        foreach(Transform rayPoint in rayPoints)
        {
            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out RaycastHit rayHitInfo, restLength + wheelRadius + maxSpringTarvel))
            {
                float springLength = rayHitInfo.distance - wheelRadius;
                float springCompression = (restLength - springLength) / maxSpringTarvel;
                float springForce = springStiffness * springCompression;

                float springVelocity = Vector3.Dot(carRigidbody.GetPointVelocity(transform.TransformPoint(rayPoint.position)), rayPoint.up);
                float dampingForce = springVelocity * damperStiffness;

                float netForce = springForce - dampingForce;

                carRigidbody.AddForceAtPosition(rayPoint.up * netForce, rayPoint.position);

                Debug.DrawLine(rayPoint.position, rayHitInfo.point, Color.red);
            }
            else
            {
                Debug.DrawLine(rayPoint.position, rayPoint.position + (-rayPoint.up) * (restLength + wheelRadius + maxSpringTarvel), Color.green);

            }
        }

    }

    private void DamperStiffnessBounds()
    {
        float zetaMin = .2f;
        float zetaMax = 1f;
        double minStiffness = 2 * Math.Sqrt((double)springStiffness * carRigidbody.mass) * zetaMin;
        double maxStiffness = 2 * Math.Sqrt((double)springStiffness * carRigidbody.mass) * zetaMax;

        Debug.Log(minStiffness + "   ---   " + maxStiffness);
    }
    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        DamperStiffnessBounds();
    }

    private void FixedUpdate()
    {
        Suspension();
    }

    /* 
    // steering parameters
    [SerializeField] AnimationCurve BackTirelookupCurve;
    [SerializeField] AnimationCurve FrontTirelookupCurve;

    // acceleration curve
    [SerializeField] AnimationCurve AccelerationCurve;



     void OX_Forces(Transform tireTransform)
     {
         RaycastHit tireRay;
         bool rayDidHit = Physics.Raycast(tireTransform.position, -tireTransform.transform.up, out tireRay, (restLength + maxSpringTarvel));

         // viteza unei roti in R^3
         Vector3 tireVelocity = carRigidbody.GetPointVelocity(tireTransform.position);

         if (rayDidHit && Vector3.Magnitude(tireVelocity) != 0)
         {
             // versor pt directia ox a rotii
             Vector3 steerDir = tireTransform.transform.right;


             // viteza unei roti pe componenta ox
             float tireVelocityOX = Vector3.Dot(steerDir, tireVelocity);

             // cat % din viteza unei roti este regasita pe ox, am nevoie de modul sa nu obtin valori negative pt procent
             float percentage = Mathf.Abs(tireVelocityOX / Vector3.Magnitude(tireVelocity));

             *//* cat grip sa aiba o roata la un moment dat
                pt un procent de 0% inseamna ca ma duc 100% inainte, nu in laterale,
                asadar am nevoie de complementul lui y fata de 1 si ala este gripul
              *//*
             float grip = 1 - BackTirelookupCurve.Evaluate(percentage);

             // aici obtin acceleratia, F = m*a, Time.fixedDeltaTime este ca o secunda, o unitate fundamentala de masura
             float accelaratie = (-tireVelocityOX * grip)/Time.fixedDeltaTime;

             // aplic forta
             carRigidbody.AddForceAtPosition(steerDir * TireMass * accelaratie, tireTransform.position);
         }
     }

     void Forces(Transform tireTransform)
     {
         *//*Suspension(tireTransform);*//*
         OX_Forces(tireTransform);
     }
 */
}
