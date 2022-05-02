using UnityEngine;
using System;

[Serializable]
public enum DriveType
{
    RearWheelDrive,
    FrontWheelDrive,
    AllWheelDrive
}

[Serializable]
public enum BrakeType
{
    Standard,
    ABS
}

public class WheelDrive : MonoBehaviour
{
    [Tooltip("Maximum steering angle of the wheels")]
    public float maxAngle = 30f;
    [Tooltip("Maximum torque applied to the driving wheels")]
    public float maxTorque = 300f;
    [Tooltip("Maximum brake torque applied to the driving wheels")]
    public float brakeTorque = 30000f;
    [Tooltip("Highest speed that the vehicle can accelerate to,")]
    public float MaxSpeed = 260;
    [Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here. (km/h)")]
    public GameObject wheelShape;

    public string HorizontalAxes = "Horizontal";
    public string ThrottleAxes = "Vertical";
    public string LightsAxes = "Fire1";
    public string HornAxes = "Fire2";
    public string HandBrakeAxes = "Fire3";
    public string GearstickAxes = "GearStick";

    [Header("Lighting Effects")]
    public Light LowBeam1;
    public Light LowBeam2;
    public Light HighBeam1;
    public Light HighBeam2;
    public static bool LB1;
    public static bool HB1;
    public Light BrakeLight1;
    public Light BrakeLight2;
    public Light Signals1;
    public Light Signals2;
    public Light Signals3;
    public Light Signals4;
    public Light Backup1;
    public Light Backup2;
    public static bool ActiveLight; // 0-none 1-brakelight 2- ABS warning
    public static int ActiveSignal; // 0-none 1-Lsignal 2-Rsignal 3-Hazards

    [Header("Audio FX")]
    public AudioSource AudioSrc;
    public AudioSource Horn;
    public AudioSource HandBrake;
    public AudioSource CrashSound;
    public AudioClip SkidSound;
    public GameObject PreCrash;
    public AudioClip CollisionSound;
    public AudioClip CollisionScrape;

    [Header("Controls")]
    public float XAxis; //steering
    public float YAxis; //braking
    public float GAxis; //gearbox selector
    public float HAxis; //horn
    public float LAxis; //lights
    public float BAxis; //handbrake
    public bool UseInput = true;
    public float Sensitivity = 0.85f;

    [Tooltip("The simulation mode used for the vehicle.")]
    public int Mode;
    [Tooltip("For automatic gearboxes, the currently selected gear for the transmission (Mode 1)")]
    public static int Gear;
    [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
    public float criticalSpeed = 5f;
    [Tooltip("Simulation sub-steps when the speed is above critical.")]
    public int stepsBelow = 5;
    [Tooltip("Simulation sub-steps when the speed is below critical.")]
    public int stepsAbove = 1;

    [Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
    public DriveType driveType;
    public BrakeType brakeType;

    private WheelCollider[] m_Wheels;
    private Rigidbody VehicularRB;
    private bool Toggle;
    private bool Toggle2;
    private bool Park;
    private bool Reverse;
    private float RotationalSpd;
    float torque;

    public static int CurrentSpeed;
    public static int EngineSpeed;
    public static float Xsensitivity = 1f;
    public static int Ysensitivity;
    private bool Skid;
    // Find all the WheelColliders down in the hierarchy.
    void Start()
    {
        m_Wheels = GetComponentsInChildren<WheelCollider>();
        VehicularRB = GetComponent<Rigidbody>();

        for (int i = 0; i < m_Wheels.Length; ++i)
        {
            var wheel = m_Wheels[i];

            // Create wheel shapes only when needed.
            if (wheelShape != null)
            {
                var ws = Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;
            }
        }

    }


    // This is a really simple approach to updating wheels.
    // We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
    // This helps us to figure our which wheels are front ones and which are rear.
    /*
    void Update()
    {
        m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

        if (UseInput == true)
        {
            XAxis = Input.GetAxis(HorizontalAxes) * Sensitivity * Xsensitivity;
            YAxis = Input.GetAxis(ThrottleAxes) * Sensitivity; // replace horizontal.
            HAxis = Input.GetAxis(HornAxes) * Sensitivity; //  horn
            GAxis = Input.GetAxis(GearstickAxes) * Sensitivity;
            LAxis = Input.GetAxis(LightsAxes) * Sensitivity; // lights
            BAxis = Input.GetAxis(HandBrakeAxes);
        }

        //      print(VehicularRB.velocity.x*3.6f +" " + VehicularRB.velocity.y*3.6f + " " + VehicularRB.velocity.z*3.6f);
        float angle = maxAngle * XAxis;

        if ((Mode == 0) && (YAxis > 0))
            torque = maxTorque * YAxis * (1 - (VehicularRB.velocity.magnitude * 3.6f / MaxSpeed));

        if ((Mode == 1) && (YAxis == 0))
            torque = maxTorque * 0.075f;

        if ((Mode == 1) && (YAxis > 0) && (Gear == 3))
            torque = maxTorque * YAxis * 1 - (VehicularRB.velocity.magnitude * 3.6f / MaxSpeed);

        if (((Mode == 1) && (YAxis > 0) && (Gear == 1)) || ((Mode == 0) && (YAxis < 0)))
            torque = maxTorque * YAxis * Mathf.Abs(1 - (VehicularRB.velocity.magnitude * 3.6f / 25));


        float handBrake = (HandBrake.enabled) ? brakeTorque : 0;

        if (BAxis != 0)
        {
            if (VehicularRB.velocity.magnitude * 3.6 < 5)
            {
                if (Toggle == false)
                {
                    if (HandBrake.enabled == false)
                    {
                        HandBrake.enabled = true;
                    }
                    else
                    {
                        HandBrake.enabled = false;
                    }
                    Toggle = true;
                }
            }
            else
            {
                HandBrake.enabled = true;
            }
        }
        else
        {
            Toggle = false;

            if (VehicularRB.velocity.magnitude * 3.6 > 5)
                HandBrake.enabled = false;
        }

        if (Mode == 1)
        {
            if (GAxis > 0)
                if (Gear <= 2)
                    Gear++;

            if (GAxis < 0)
                if (Gear > 0)
                    Gear--;

            if (Gear > 3)
                Gear = 3;

            if (Gear == 0)
            {
                Park = true;
                Backup1.enabled = false;
                Backup2.enabled = false;
            }
            else
            {
                Park = false;
            }

            if (Gear == 1)
            {
                Backup1.enabled = true;
                Backup2.enabled = true;
            }

            if (Gear >= 2)
            {
                Backup1.enabled = false;
                Backup2.enabled = false;
            }
        }

        if (LAxis > 0)
        {
            if (Toggle == false)
            {
                if (HighBeam1.enabled == false)
                {
                    HighBeam1.enabled = true;
                    HighBeam2.enabled = true;
                    HB1 = true;
                }
                else
                {
                    HighBeam1.enabled = false;
                    HighBeam2.enabled = false;
                    HB1 = false;
                }
                Toggle = true;
            }
        }

        if ((LAxis == 0) && (BAxis == 0))
        {
            Toggle = false;
            Toggle2 = false;
            ActiveLight = false;
        }
        if (LAxis < 0)
        {
            if ((Toggle2 == false))
            {
                if (LowBeam1.enabled == false)
                {
                    LowBeam1.enabled = true;
                    LowBeam2.enabled = true;
                    LB1 = true;
                }
                else
                {
                    LowBeam1.enabled = false;
                    LowBeam2.enabled = false;
                    LB1 = false;
                }
                Toggle2 = true;
            }
        }

        foreach (WheelCollider wheel in m_Wheels)
        {

            if ((YAxis == 0) && (CurrentSpeed > 12))
            {
                if (Reverse == false)
                    wheel.motorTorque = -maxTorque;
                else
                    wheel.motorTorque = 0;

                if (Reverse == true)
                    wheel.brakeTorque = maxTorque;
                else
                    wheel.motorTorque = 0;
            }

            // A simple car where front wheels steer while rear ones drive.
            if ((wheel.transform.localPosition.z > 0))
            {
                //    print("F: " + wheel.rpm);

                if ((brakeType == BrakeType.ABS) || ((YAxis > -0.8) && (brakeType == BrakeType.Standard)))
                    wheel.steerAngle = angle;
            }

            if (wheel.transform.localPosition.z < 0)
            {
                //     print("R: " + wheel.rpm);
                wheel.brakeTorque = handBrake / 25;

                if (Park == true)
                    wheel.brakeTorque = 12500f;

            }

            if (Mode == 1)
            {
                if ((YAxis < 0))
                {
                    if (wheel.transform.localPosition.z > 0)
                    {
                        if (brakeType == BrakeType.ABS)
                        {
                            if (((wheel.motorTorque > 0) || (wheel.motorTorque < 0)))
                            {
                                wheel.brakeTorque = wheel.brakeTorque * (-YAxis);
                            }
                            else
                            {
                                wheel.brakeTorque = 0f;
                                if (CurrentSpeed < 10)
                                    wheel.brakeTorque = VehicularRB.mass * CurrentSpeed * brakeTorque * Mathf.Abs((YAxis * 0.5f));
                            }
                        }

                        if (brakeType == BrakeType.Standard)
                            wheel.brakeTorque = VehicularRB.mass * CurrentSpeed * brakeTorque * Mathf.Abs((YAxis));

                    }

                    if (((wheel.motorTorque > 0) || (wheel.motorTorque < 0)) && (wheel.transform.localPosition.z < 0))
                    {
                        wheel.brakeTorque = (brakeTorque / (145000 / (CurrentSpeed * 0.01f))) * Mathf.Abs((YAxis));
                    }

                    if (CurrentSpeed == 0)
                    {
                        wheel.motorTorque = 0;
                        torque = 0;
                    }

                    BrakeLight1.enabled = true;
                    BrakeLight2.enabled = true;
                }
                else
                {
                    BrakeLight1.enabled = false;
                    BrakeLight2.enabled = false;

                    wheel.brakeTorque = 0f;
                }
            }

            if (wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive)
            {

                if ((CurrentSpeed == 0) && (YAxis < 0))
                    Reverse = true;
                if ((CurrentSpeed == 0) && (YAxis > 0))
                    Reverse = false;

                if (Mode == 0)
                {
                    if (YAxis != 0)
                        wheel.motorTorque = torque;


                    if (((YAxis < 0) && (Reverse == false)) || (((YAxis > 0) && (Reverse == true))))
                    {
                        BrakeLight1.enabled = true;
                        BrakeLight2.enabled = true;
                    }
                    else
                    {
                        BrakeLight1.enabled = false;
                        BrakeLight2.enabled = false;
                    }

                    if ((YAxis < 0) && (CurrentSpeed > 25))
                    {
                        if (Reverse == true)
                        {
                            wheel.motorTorque = -torque;
                        }

                        if (CurrentSpeed > 25)
                            wheel.brakeTorque = 56000f;
                    }

                    if (Reverse == true)
                    {
                        Backup1.enabled = true;
                        Backup2.enabled = true;
                    }
                    else
                    {
                        Backup1.enabled = false;
                        Backup2.enabled = false;
                    }


                }

                if ((Mode == 1) && (Gear == 1) && (YAxis >= 0))
                {
                    wheel.motorTorque = -Mathf.Abs(torque);
                    if (CurrentSpeed > 30)
                        wheel.brakeTorque = 56000f;
                }

                if ((Mode == 1) && (Gear == 3) && (YAxis >= 0))
                    wheel.motorTorque = Mathf.Abs(torque);

            }

            if (wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive)
            {

                if ((CurrentSpeed == 0) && (YAxis < 0))
                    Reverse = true;
                if ((CurrentSpeed == 0) && (YAxis > 0))
                    Reverse = false;

                if (Mode == 0)
                {
                    if (YAxis != 0)
                        wheel.motorTorque = torque;

                    if (((YAxis < 0) && (Reverse == false)) || (((YAxis > 0) && (Reverse == true))))
                    {
                        BrakeLight1.enabled = true;
                        BrakeLight2.enabled = true;
                    }
                    else
                    {
                        BrakeLight1.enabled = false;
                        BrakeLight2.enabled = false;
                    }

                    if ((YAxis < 0) && (CurrentSpeed > 25))
                    {
                        if (Reverse == true)
                            wheel.motorTorque = -torque;/////////


                        if (CurrentSpeed > 25)
                            wheel.brakeTorque = 56000f;
                    }

                    if (Reverse == true)
                    {
                        Backup1.enabled = true;
                        Backup2.enabled = true;
                    }
                    else
                    {
                        Backup1.enabled = false;
                        Backup2.enabled = false;
                    }

                }


                if ((Mode == 1) && (Gear == 1) && (YAxis >= 0))
                {
                    wheel.motorTorque = -Mathf.Abs(torque);
                    if (CurrentSpeed > 30)
                        wheel.brakeTorque = 56000f;
                }
                if ((Mode == 1) && (Gear == 3) && (YAxis >= 0))
                    wheel.motorTorque = Mathf.Abs(torque);

            }

            // Update visual wheels if any.
            if (wheelShape)
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                // Assume that the only child of the wheelcollider is the wheel shape.
                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
            }

            if (Mode == 0)
            {
                if (((wheel.motorTorque < 0) && (YAxis < 0)) && (AudioSrc.pitch < 5.5))
                    AudioSrc.pitch = AudioSrc.pitch + (0.15f * -YAxis);

                if (((wheel.motorTorque > 0) && (YAxis > 0)) && (AudioSrc.pitch < 5.5))
                    AudioSrc.pitch = AudioSrc.pitch + (0.15f * YAxis);
            }
            else
            {
                if (((YAxis > 0)) && (AudioSrc.pitch < 5.5))
                    AudioSrc.pitch = AudioSrc.pitch + (0.15f * YAxis);
            }

            if ((((Mathf.Abs(VehicularRB.velocity.z) > Mathf.Abs(VehicularRB.velocity.x)) && (VehicularRB.velocity.magnitude * 3.6f > 50) && ((Mathf.Abs(VehicularRB.transform.eulerAngles.y) > 45) && (Mathf.Abs(VehicularRB.transform.eulerAngles.y) < 90)) || ((Mathf.Abs(VehicularRB.transform.eulerAngles.y) > 90) && (Mathf.Abs(VehicularRB.transform.eulerAngles.y) < 135))) || (((Mathf.Abs(VehicularRB.velocity.x) > Mathf.Abs(VehicularRB.velocity.z)) && (VehicularRB.velocity.magnitude * 3.6f > 50) && ((Mathf.Abs(VehicularRB.transform.eulerAngles.y) > 0) && (Mathf.Abs(VehicularRB.transform.eulerAngles.y) < 45)) || ((Mathf.Abs(VehicularRB.transform.eulerAngles.y) > 135) && (Mathf.Abs(VehicularRB.transform.eulerAngles.y) < 180))))))
            {
                if ((Mode == 0) || ((Gear == 1) || (Gear == 3)))
                {
                    CrashSound.clip = SkidSound;
                    CrashSound.enabled = false;
                    CrashSound.enabled = true;
                    CrashSound.loop = true;
                    PreCrash.SetActive(CrashSound.enabled);
                }
            }
            else
            if (((VehicularRB.velocity.magnitude * 3.6 < 10) && (YAxis > 0.8)) || ((VehicularRB.velocity.magnitude * 3.6 <= 0) && (YAxis > 0.1f)) || ((Mode == 1) && (YAxis < -0.8f) && (VehicularRB.velocity.magnitude * 3.6f >= 30)) || ((Mode == 0) && (((Reverse == true) && (YAxis >= 0.8)) || (((Reverse == false) && (YAxis < -0.8))) && (VehicularRB.velocity.magnitude * 3.6 > 30))) || ((HandBrake.enabled == true) && (VehicularRB.velocity.magnitude * 3.6f > 30)))
            {
                if ((Mode == 0) || ((Gear == 1) || (Gear == 3)))
                {
                    CrashSound.clip = SkidSound;
                    CrashSound.enabled = false;
                    CrashSound.enabled = true;
                    CrashSound.loop = true;
                    PreCrash.SetActive(CrashSound.enabled);
                }
            }
            else
            {
                if ((CrashSound.clip != CollisionSound))
                {
                    print("off");
                    CrashSound.enabled = false;
                    PreCrash.SetActive(CrashSound.enabled);
                }
            }
        }

        if (HAxis != 0)
        {
            Horn.enabled = true;
        }
        else
        {
            Horn.enabled = false;
        }

        if (VehicularRB.velocity.magnitude > 0)
        {
            CurrentSpeed = (int)(VehicularRB.velocity.magnitude * 3.6f);
            //  print("X= " + VehicularRB.velocity.x + "Y= " +VehicularRB.velocity.y + "Z= "+ VehicularRB.velocity.z);
        }

        EngineSpeed = (int)(8000 * (AudioSrc.pitch / 5.5f));

        if (AudioSrc.pitch > 1.25)
        {
            AudioSrc.pitch = AudioSrc.pitch - 0.25f;
        }

    }

    void OnCollisionEnter(Collision x)
    {

        print("Collision!");
        print("Points colliding: " + x.contacts.Length);
        print("First point that collided: " + x.contacts[0].point);

        if (CurrentSpeed > 0)
        {
            CrashSound.clip = CollisionScrape;
            CrashSound.enabled = false;
            CrashSound.loop = false;
            CrashSound.enabled = true;
        }
        if (CurrentSpeed > 6.5)
        {
            CrashSound.clip = CollisionSound;
            CrashSound.enabled = false;
            CrashSound.loop = false;
            CrashSound.enabled = true;
        }
        if (CurrentSpeed > 50)
        {
            UseInput = false;
            YAxis = 0;
            maxTorque = 0;
            Gear = 2;
            AudioSrc.enabled = false;

        }

    }

    */
    public void ChangeCtrlType(int x)
    {
        Mode = x;
    }

    public void ChangeSensitivity(float s)
    {
        Sensitivity = s;
    }

    public void ChangeSteerSensitivity(float steer)
    {
        Xsensitivity = -steer;
    }

    public void brakeForce(float b)
    {
        brakeTorque = b * 1e5f;
    }

    public void MaxVelocity(float velocity)
    {
        MaxSpeed = velocity;
    }

    public void ChangeOperatingMode(float mode)
    {
        Mode = (int)mode;
    }

}
