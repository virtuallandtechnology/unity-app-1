using UnityEngine;
using System.Threading.Tasks;

public class CarController : MonoBehaviour
{
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;

    public WheelCollider frontLeft, frontRight;
    public WheelCollider rearLeft, rearRight;
    public Transform trFrontLeft, trFrontRight;
    public Transform trRearLeft, trRearRight;

    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;
    public Rigidbody body;

    void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        rearLeft.motorTorque = verticalInput * motorForce;
        rearRight.motorTorque = verticalInput * motorForce;

        float currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking(currentBrakeForce);
    }

    private void ApplyBraking(float force)
    {
        frontLeft.brakeTorque = force;
        frontRight.brakeTorque = force;
        rearLeft.brakeTorque = force;
        rearRight.brakeTorque = force;
    }

    public async Task HandBrakToStop()
    {
        while (body.linearVelocity.magnitude>1)
        {
            float currentBrakeForce = isBraking ? brakeForce : 0f;
            ApplyBraking(currentBrakeForce);
            await Task.Delay(100);
        }

    }

    private void HandleSteering()
    {
        float steerAngle = horizontalInput * maxSteerAngle;
        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeft, trFrontLeft);
        UpdateSingleWheel(frontRight, trFrontRight);
        UpdateSingleWheel(rearLeft, trRearLeft);
        UpdateSingleWheel(rearRight, trRearRight);
    }

    private void UpdateSingleWheel(WheelCollider collider, Transform transform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        transform.position = pos;
        transform.rotation = rot;
    }


}