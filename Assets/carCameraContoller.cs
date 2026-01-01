using Unity.Cinemachine;
using UnityEngine;

public class carCameraContoller : MonoBehaviour
{
    //public CarController carController;
    public float CarSpeed;
    public float driftValue;
    public Rigidbody rb;
    public Vector3 localVelocity;
    public CinemachineOrbitalFollow orbit;
    public Vector2 driftMinMax;
    public CinemachineCamera cam;

    private void Update()
    {
        
        if (!CinemachineCore.IsLive(cam))
            return;

        CarSpeed = rb.linearVelocity.magnitude;
        localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        driftValue = localVelocity.x;
        driftValue=Mathf.Round(driftValue*100/100.0f);
        driftValue=Mathf.Clamp(driftValue,driftMinMax.x,driftMinMax.y);
        orbit.HorizontalAxis.Value=driftValue;
    }

}
