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
    private float horizontal;
    private float vertical;

    [Serializable]
    public struct WheelStruct
    {
        public Transform rayPoint;
        public bool hasPower;
        public bool canSteer;
        public bool isGrounded;
        public float tireGrip;
        public bool isFrontWheel;
        public Transform wheelVisual;
        
    }

    // car properties
    [Space]
    [BoxGroup("Car Properties")] [SerializeField] WheelStruct[] carWheels;
    [BoxGroup("Car Properties")] [SerializeField] LayerMask drivable;



    // car engine properties
    [Space]
    [BoxGroup("Care Engine Settings")] [SerializeField] float acceleration = 25f;
    [BoxGroup("Care Engine Settings")] [SerializeField] float maxSpeed = 100f;
    [BoxGroup("Care Engine Settings")] [SerializeField] float decelaration = 10f;



    #region Steering Functions
    // car steering properties
    [Space]
    [BoxGroup("Car Steering Settings")] [SerializeField] private float maxSteeringAngle = 30f;
    [BoxGroup("Car Steering Settings")] [SerializeField] private float tireMass = 30f;
    [BoxGroup("Car Steering Settings")] [SerializeField] private AnimationCurve frontTiresSlideCurve;
    [BoxGroup("Car Steering Settings")] [SerializeField] private AnimationCurve backTiresSlideCurve;


    private void Steering()
    {
        foreach (WheelStruct wheel in carWheels)
        {
            if (wheel.isGrounded)
            {
                Vector3 steerDirection = wheel.rayPoint.right;
                Vector3 wheelVelocity = carRigidbody.GetPointVelocity(transform.TransformPoint(wheel.rayPoint.position));
                Debug.DrawLine(wheel.rayPoint.position, wheel.rayPoint.position + wheelVelocity, Color.yellow);
                
                float wheelSteeringVelocity = Vector3.Dot(steerDirection, wheelVelocity);

                float slidingRatio = wheelSteeringVelocity / Vector3.Magnitude(wheelVelocity);

                float tireGripFactor;
                if (wheel.isFrontWheel)
                {
                    tireGripFactor = frontTiresSlideCurve.Evaluate(slidingRatio);
                }
                else
                {
                    tireGripFactor = backTiresSlideCurve.Evaluate(slidingRatio);
                }

                float desiredVelocityChange = -wheelSteeringVelocity * tireGripFactor;

                float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

                carRigidbody.AddForceAtPosition(wheel.rayPoint.right * tireMass * desiredAcceleration, wheel.rayPoint.position);
            
             
                Debug.DrawLine(wheel.rayPoint.position, wheel.rayPoint.position + wheel.rayPoint.right * tireMass * desiredAcceleration, Color.blue);
            }

        }
    }
    #endregion

    #region Suspension Functions
    // spring parameters;
    [Space]
    [BoxGroup("Suspension Settings")] [SerializeField] float springStiffness = 10f;
    [BoxGroup("Suspension Settings")] [SerializeField] float restLength = 1f;
    [BoxGroup("Suspension Settings")] [SerializeField] float maxSpringTarvel = .4f;
    [BoxGroup("Suspension Settings")] [SerializeField] float damperStiffness = 10f;
    [BoxGroup("Suspension Settings")] [SerializeField] float wheelRadius;
    
    void Suspension()
    {
        for (int i = 0; i < carWheels.Length; i++)
        {
            if (Physics.Raycast(carWheels[i].rayPoint.position, -carWheels[i].rayPoint.up, out RaycastHit rayHitInfo, restLength + wheelRadius + maxSpringTarvel))
            {
                Vector3 springDir = carWheels[i].rayPoint.up;

                Vector3 tireWorldVel = carRigidbody.GetPointVelocity(carWheels[i].rayPoint.position);

                float springLength = rayHitInfo.distance - wheelRadius;
                float springCompression = restLength - springLength;
                
                float springVelocity = Vector3.Dot(springDir, tireWorldVel);

                float springForce = springStiffness * springCompression;
                float dampingForce = springVelocity * damperStiffness;

                float netForce = springForce - dampingForce;

                carRigidbody.AddForceAtPosition(springDir * netForce, carWheels[i].rayPoint.position);

                carWheels[i].isGrounded = true;

                Debug.Log("wheel " + i + ": " + carWheels[i].rayPoint.up * netForce);
                Debug.DrawLine(carWheels[i].rayPoint.position, rayHitInfo.point, Color.red);
            }
            else
            {
                carWheels[i].isGrounded = false;
                
                
                Debug.DrawLine(carWheels[i].rayPoint.position, carWheels[i].rayPoint.position + (-carWheels[i].rayPoint.up) * (restLength + wheelRadius + maxSpringTarvel), Color.green);
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

    #endregion

    #region Debug Functions
    private void DrawOrientation(Transform transform)
    {
        float size = 1;
        Debug.DrawLine(transform.position, transform.position + transform.up * size, Color.green);
        Debug.DrawLine(transform.position, transform.position + transform.forward * size, Color.blue);
        Debug.DrawLine(transform.position, transform.position + transform.right * size, Color.red);
    }
    #endregion
    
    #region Unity Functions
    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        DamperStiffnessBounds();
    }

    private void FixedUpdate()
    {
        Suspension();
        Steering();
    }

    private void Update()
    {
        vertical = 0;
        horizontal = 0;

        if ( Input.GetKey(KeyCode.W))
        {
            vertical = +1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            vertical = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            horizontal = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontal = +1;
        }

        foreach (WheelStruct wheel in carWheels)
        {
            DrawOrientation(wheel.rayPoint);
        }

        // rotating the raypoint in local space, relative to the car itself, not global
        for (int i = 0; i < carWheels.Length; i++)
        {
            if (carWheels[i].canSteer)
            {
                Vector3 wheelSteeringDirection = new Vector3(0, horizontal * maxSteeringAngle, 0);
                carWheels[i].rayPoint.localEulerAngles= wheelSteeringDirection;
            }
        }

        for (int i = 0; i < carWheels.Length; i++)
        {
            Vector3 wheelPosition = carWheels[i].rayPoint.localPosition;
            carWheels[i].wheelVisual.transform.localPosition= wheelPosition;
            carWheels[i].wheelVisual.transform.localRotation = carWheels[i].rayPoint.localRotation;
        }

    }
    #endregion

    #region OLD CODE
    /* OLD CODE
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

    #endregion
}
