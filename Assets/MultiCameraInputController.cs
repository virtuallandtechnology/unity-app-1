using UnityEngine;
using Unity.Cinemachine;

public class MultiCameraInputController : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera farCamera;
    public CinemachineVirtualCamera nearCamera;
    public CinemachineVirtualCamera headCamera; // Could be FreeLook or Flyaround

    [Header("Sensitivity")]
    public float mouseSensitivity = 3f;
    public float touchSensitivity = 0.1f;
    public float smoothTime = 0.05f;

    private Vector2 lookDelta;
    private Vector2 lookVelocity;

    void Update()
    {
        Vector2 targetDelta = Vector2.zero;

#if UNITY_STANDALONE || UNITY_EDITOR
        targetDelta.x = Input.GetAxis("Mouse X") * mouseSensitivity;
        targetDelta.y = Input.GetAxis("Mouse Y") * mouseSensitivity;
#elif UNITY_ANDROID
if (Input.touchCount > 0)
{
Touch touch = Input.GetTouch(0);
if (touch.phase == TouchPhase.Moved)
{
targetDelta.x = touch.deltaPosition.x * touchSensitivity;
targetDelta.y = touch.deltaPosition.y * touchSensitivity;
}
}
#endif

        // Smooth movement  
        lookDelta = Vector2.SmoothDamp(lookDelta, targetDelta, ref lookVelocity, smoothTime);

        ApplyToCameras();
    }

    void ApplyToCameras()
    {
        if (farCamera != null) ApplyDelta(farCamera);
        if (nearCamera != null) ApplyDelta(nearCamera);
        if (headCamera != null) ApplyDelta(headCamera);
    }

    void ApplyDelta(CinemachineVirtualCamera cam)
    {
        //if (cam == null) return;

        //var freeLook = cam as CinemachineFreeLook;
        //if (freeLook != null)
        //{
        //    freeLook.m_XAxis.Value += lookDelta.x;
        //    freeLook.m_YAxis.Value += -lookDelta.y; // invert Y if needed  
        //}
        //else
        //{
        //    // Regular Virtual Camera: rotate the transform  
        //    cam.transform.Rotate(-lookDelta.y, lookDelta.x, 0f, Space.Self);
        //}
    }

}
